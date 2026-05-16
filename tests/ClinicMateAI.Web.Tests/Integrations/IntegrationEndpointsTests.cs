using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ClinicMateAI.Application.Abstractions.Messaging;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Infrastructure.Data;
using ClinicMateAI.Web.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ClinicMateAI.Web.Tests.Integrations;

public class IntegrationEndpointsTests
{
    [Fact]
    public async Task GetOverview_ReturnsLineAndFacebookStatuses()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var clinicId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        var response = await client.GetAsync($"/api/integrations/overview?clinicId={clinicId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await response.Content.ReadAsStringAsync();
        json.Should().Contain("\"channel\":\"LINE\"");
        json.Should().Contain("\"channel\":\"Facebook\"");
    }

    [Fact]
    public async Task SaveLineConfig_ReturnsNoContent_AndStoresPendingVerificationStatus()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var response = await client.PostAsJsonAsync("/api/integrations/line/save", new
        {
            clinicId,
            channelSecret = "line-secret",
            accessToken = "line-access-token"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config = await db.ClinicChannelConfigs.SingleAsync(x => x.ClinicId == clinicId && x.Channel == "LINE");
        config.ConnectionStatus.Should().Be(ChannelConnectionStatus.PendingVerification);
        config.LastError.Should().BeEmpty();
        config.Secret.Should().Be("line-secret");
        config.AccessToken.Should().Be("line-access-token");
    }

    [Fact]
    public async Task TestLineConfig_ReturnsOk_AndMarksLineConnected_WhenTesterSucceeds()
    {
        await using var factory = new ClinicMateWebFactory(new LineConnectionTestResult(true, string.Empty));
        using var client = factory.CreateClient();

        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        await client.PostAsJsonAsync("/api/integrations/line/save", new
        {
            clinicId,
            channelSecret = "line-secret",
            accessToken = "line-access-token"
        });

        var response = await client.PostAsJsonAsync("/api/integrations/line/test", new
        {
            clinicId
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("isSuccess").GetBoolean().Should().BeTrue();

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config = await db.ClinicChannelConfigs.SingleAsync(x => x.ClinicId == clinicId && x.Channel == "LINE");
        config.ConnectionStatus.Should().Be(ChannelConnectionStatus.Connected);
        config.LastVerifiedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task TestLineConfig_ReturnsNotFound_WhenLineConfigIsMissing()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var clinicId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var response = await client.PostAsJsonAsync("/api/integrations/line/test", new
        {
            clinicId
        });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("LINE channel is not configured.");
    }

    [Fact]
    public async Task StartFacebookConnection_ReturnsAuthorizationUrl()
    {
        var clinicId = Guid.Parse("33333333-3333-3333-3333-333333333333");
        await using var factory = new ClinicMateWebFactory(
            facebookConnectionResult: new FacebookConnectionResult(
                PageId: "123456789",
                PageName: "The Glow Clinic Bangkok",
                AccessToken: "page-token",
                LongLivedToken: "renew-token",
                TokenExpiresAtUtc: new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc)),
            facebookAuthorizationUrl: $"https://facebook.example/connect?clinicId={clinicId}");
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/integrations/facebook/start?clinicId={clinicId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("authorizationUrl").GetString()
            .Should().Be($"https://facebook.example/connect?clinicId={clinicId}");
    }

    [Fact]
    public async Task CompleteFacebookConnection_ReturnsNoContent_AndMarksFacebookConnected()
    {
        var clinicId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        await using var factory = new ClinicMateWebFactory(
            facebookConnectionResult: new FacebookConnectionResult(
                PageId: "123456789",
                PageName: "The Glow Clinic Bangkok",
                AccessToken: "page-token",
                LongLivedToken: "renew-token",
                TokenExpiresAtUtc: new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc)));
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/integrations/facebook/complete", new
        {
            clinicId,
            authorizationCode = "auth-code"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config = await db.ClinicChannelConfigs.SingleAsync(x => x.ClinicId == clinicId && x.Channel == "Facebook");
        config.ConnectionStatus.Should().Be(ChannelConnectionStatus.Connected);
        config.AccessToken.Should().Be("page-token");
        config.RefreshTokenOrLongLivedToken.Should().Be("renew-token");
        config.TokenExpiresAtUtc.Should().Be(new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc));
        config.ExternalPageId.Should().Be("123456789");
        config.LastError.Should().BeEmpty();
        config.IsEnabled.Should().BeTrue();
        config.LastVerifiedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task CompleteFacebookConnection_WithRegisteredDemoProvider_ReturnsNoContent_AndPersistsConnection()
    {
        var clinicId = Guid.Parse("45454545-4545-4545-4545-454545454545");
        await using var factory = new ClinicMateWebFactory(useRealFacebookProvider: true);
        using var client = factory.CreateClient();
        var before = DateTime.UtcNow;

        var response = await client.PostAsJsonAsync("/api/integrations/facebook/complete", new
        {
            clinicId,
            authorizationCode = "demo-auth-code"
        });

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config = await db.ClinicChannelConfigs.SingleAsync(x => x.ClinicId == clinicId && x.Channel == "Facebook");
        config.ConnectionStatus.Should().Be(ChannelConnectionStatus.Connected);
        config.AccessToken.Should().Be("demo-facebook-access-token");
        config.RefreshTokenOrLongLivedToken.Should().Be("demo-facebook-long-lived-token");
        config.ExternalPageId.Should().StartWith("demo-page-");
        config.LastError.Should().BeEmpty();
        config.IsEnabled.Should().BeTrue();
        config.LastVerifiedAtUtc.Should().NotBeNull();
        config.TokenExpiresAtUtc.Should().BeCloseTo(before.AddDays(60), TimeSpan.FromMinutes(2));
    }

    [Fact]
    public async Task CompleteFacebookConnection_ReturnsValidationProblem_WhenRequestInvalid()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/integrations/facebook/complete", new
        {
            clinicId = Guid.Empty,
            authorizationCode = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("errors").TryGetProperty("ClinicId", out _).Should().BeTrue();
        body.GetProperty("errors").TryGetProperty("AuthorizationCode", out _).Should().BeTrue();
    }

    [Fact]
    public async Task RenewFacebookConnection_ReturnsOk_AndMarksReconnectRequired_WhenRenewalFails()
    {
        var clinicId = Guid.Parse("56565656-5656-5656-5656-565656565656");
        await using var factory = new ClinicMateWebFactory(
            facebookRenewalResult: new FacebookTokenRenewalResult(
                IsSuccess: false,
                AccessToken: string.Empty,
                LongLivedToken: string.Empty,
                TokenExpiresAtUtc: null,
                ErrorMessage: "Facebook permissions expired."));
        using var client = factory.CreateClient();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.ClinicChannelConfigs.Add(new ClinicChannelConfig
            {
                ClinicId = clinicId,
                Channel = "Facebook",
                AccessToken = "expired-page-token",
                RefreshTokenOrLongLivedToken = "expired-long-lived-token",
                TokenExpiresAtUtc = DateTime.UtcNow.AddDays(-1),
                ConnectionStatus = ChannelConnectionStatus.Connected,
                IsEnabled = true
            });
            await db.SaveChangesAsync();
        }

        var response = await client.PostAsJsonAsync("/api/integrations/facebook/renew", new
        {
            clinicId
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("isSuccess").GetBoolean().Should().BeFalse();
        body.GetProperty("errorMessage").GetString().Should().Be("Facebook permissions expired.");
        body.TryGetProperty("accessToken", out _).Should().BeFalse();
        body.TryGetProperty("longLivedToken", out _).Should().BeFalse();

        using var assertionScope = factory.Services.CreateScope();
        var assertionDb = assertionScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var config = await assertionDb.ClinicChannelConfigs.SingleAsync(x => x.ClinicId == clinicId && x.Channel == "Facebook");
        config.ConnectionStatus.Should().Be(ChannelConnectionStatus.ReconnectRequired);
        config.LastError.Should().Be("Facebook permissions expired.");
        config.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task RenewFacebookConnection_ReturnsSafePayload_WhenRenewalSucceeds()
    {
        var clinicId = Guid.Parse("57575757-5757-5757-5757-575757575757");
        await using var factory = new ClinicMateWebFactory(
            facebookRenewalResult: new FacebookTokenRenewalResult(
                IsSuccess: true,
                AccessToken: "renewed-page-token",
                LongLivedToken: "renewed-long-lived-token",
                TokenExpiresAtUtc: new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                ErrorMessage: string.Empty));
        using var client = factory.CreateClient();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.ClinicChannelConfigs.Add(new ClinicChannelConfig
            {
                ClinicId = clinicId,
                Channel = "Facebook",
                AccessToken = "expired-page-token",
                RefreshTokenOrLongLivedToken = "expired-long-lived-token",
                TokenExpiresAtUtc = DateTime.UtcNow.AddDays(-1),
                ConnectionStatus = ChannelConnectionStatus.ReconnectRequired,
                IsEnabled = false
            });
            await db.SaveChangesAsync();
        }

        var response = await client.PostAsJsonAsync("/api/integrations/facebook/renew", new
        {
            clinicId
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("isSuccess").GetBoolean().Should().BeTrue();
        body.GetProperty("errorMessage").GetString().Should().BeEmpty();
        body.GetProperty("tokenExpiresAtUtc").GetDateTime().Should().Be(new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc));
        body.TryGetProperty("accessToken", out _).Should().BeFalse();
        body.TryGetProperty("longLivedToken", out _).Should().BeFalse();
    }

    private sealed class ClinicMateWebFactory(
        LineConnectionTestResult? lineTestResult = null,
        FacebookConnectionResult? facebookConnectionResult = null,
        string? facebookAuthorizationUrl = null,
        FacebookTokenRenewalResult? facebookRenewalResult = null,
        bool useRealFacebookProvider = false) : WebApplicationFactory<Program>
    {
        private readonly string _appDbName = $"app-{Guid.NewGuid()}";
        private readonly string _identityDbName = $"identity-{Guid.NewGuid()}";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSetting("Logging:EventLog:LogLevel:Default", "None");
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<ILineChannelConnectionTester>();
                services.RemoveAll<IFacebookTokenRenewalProvider>();
                if (!useRealFacebookProvider)
                {
                    services.RemoveAll<IFacebookConnectionProvider>();
                }

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(_appDbName));
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(_identityDbName));
                services.AddSingleton<ILineChannelConnectionTester>(
                    new StubLineChannelConnectionTester(lineTestResult ?? new LineConnectionTestResult(true, string.Empty)));
                if (!useRealFacebookProvider)
                {
                    services.AddSingleton<IFacebookConnectionProvider>(
                        new StubFacebookConnectionProvider(
                            facebookAuthorizationUrl ?? "https://facebook.example/connect",
                            facebookConnectionResult ?? new FacebookConnectionResult(
                                PageId: "123456789",
                                PageName: "The Glow Clinic Bangkok",
                                AccessToken: "page-token",
                                LongLivedToken: "renew-token",
                                TokenExpiresAtUtc: new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc))));
                }
                services.AddSingleton<IFacebookTokenRenewalProvider>(
                    new StubFacebookTokenRenewalProvider(
                        facebookRenewalResult ?? new FacebookTokenRenewalResult(
                            IsSuccess: true,
                            AccessToken: "renewed-page-token",
                            LongLivedToken: "renewed-long-lived-token",
                            TokenExpiresAtUtc: new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
                            ErrorMessage: string.Empty)));
            });
        }
    }

    private sealed class StubLineChannelConnectionTester(LineConnectionTestResult result) : ILineChannelConnectionTester
    {
        public Task<LineConnectionTestResult> TestAsync(
            string channelSecret,
            string accessToken,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);
    }

    private sealed class StubFacebookConnectionProvider(
        string authorizationUrl,
        FacebookConnectionResult result) : IFacebookConnectionProvider
    {
        public string BuildAuthorizationUrl(Guid clinicId)
            => authorizationUrl;

        public Task<FacebookConnectionResult> CompleteAsync(
            Guid clinicId,
            string authorizationCode,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);
    }

    private sealed class StubFacebookTokenRenewalProvider(FacebookTokenRenewalResult result) : IFacebookTokenRenewalProvider
    {
        public Task<FacebookTokenRenewalResult> RenewAsync(
            string longLivedToken,
            CancellationToken cancellationToken = default)
            => Task.FromResult(result);
    }
}
