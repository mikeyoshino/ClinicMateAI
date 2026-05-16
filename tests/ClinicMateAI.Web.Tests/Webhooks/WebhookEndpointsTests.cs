using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Infrastructure.Data;
using ClinicMateAI.Web.Data;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ClinicMateAI.Web.Tests.Webhooks;

public class WebhookEndpointsTests
{
    private static readonly Guid TestClinicId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private const string TestChannelSecret = "test-secret-123";
    private const string TestAccessToken = "test-access-token";

    // Build a minimal real LINE webhook payload
    private static string BuildLinePayload(string userId, string messageId, string messageText)
        => $$"""
             {
               "destination": "Uxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
               "events": [
                 {
                   "type": "message",
                   "replyToken": "nHuyWiB7yP5Zw52FIkcQobQuGDXCTA",
                   "source": { "userId": "{{userId}}", "type": "user" },
                   "message": { "id": "{{messageId}}", "type": "text", "text": "{{messageText}}" },
                   "timestamp": 1625665242211
                 }
               ]
             }
             """;

    private static string ComputeSignature(string body, string secret)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        using var hmac = new HMACSHA256(key);
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(body)));
    }

    [Fact]
    public async Task LineWebhook_PersistsConversationAndMessage()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        // Pre-seed ClinicChannelConfig so the endpoint finds the clinic's LINE config
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.ClinicChannelConfigs.Add(new ClinicChannelConfig
            {
                ClinicId = TestClinicId,
                Channel = "LINE",
                AccessToken = TestAccessToken,
                Secret = TestChannelSecret,
                ExternalPageId = "Uxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
            });
            await db.SaveChangesAsync();
        }

        var body = BuildLinePayload("U-user-001", "msg-001", "โบท็อกกรามเท่าไรคะ");
        var sig = ComputeSignature(body, TestChannelSecret);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/webhooks/line/{TestClinicId}")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Line-Signature", sig);

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var verifyScope = factory.Services.CreateScope();
        var verifyDb = verifyScope.ServiceProvider.GetRequiredService<AppDbContext>();
        verifyDb.Conversations.Should().ContainSingle(x =>
            x.ClinicId == TestClinicId
            && x.Channel == "LINE"
            && x.ExternalConversationId == "U-user-001");

        var conversation = verifyDb.Conversations.Single(x =>
            x.ClinicId == TestClinicId && x.ExternalConversationId == "U-user-001");
        verifyDb.Messages.Should().ContainSingle(x =>
            x.ClinicId == TestClinicId
            && x.ConversationId == conversation.Id
            && x.Text == "โบท็อกกรามเท่าไรคะ");
    }

    [Fact]
    public async Task LineWebhook_Returns400_WhenSignatureInvalid()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.ClinicChannelConfigs.Add(new ClinicChannelConfig
            {
                ClinicId = TestClinicId,
                Channel = "LINE",
                AccessToken = TestAccessToken,
                Secret = TestChannelSecret,
                ExternalPageId = "Uxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
            });
            await db.SaveChangesAsync();
        }

        var body = BuildLinePayload("U-user-002", "msg-002", "hello");
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/webhooks/line/{TestClinicId}")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Line-Signature", "invalid-signature");

        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task LineWebhook_Returns404_WhenClinicNotConfigured()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var unknownClinicId = Guid.NewGuid();
        var body = BuildLinePayload("U-user-003", "msg-003", "test");
        var sig = ComputeSignature(body, TestChannelSecret);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/webhooks/line/{unknownClinicId}")
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        request.Headers.Add("X-Line-Signature", sig);

        var response = await client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task FacebookWebhook_ReturnsValidationProblem_WhenPayloadInvalid()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var payload = new
        {
            clinicId = Guid.Empty,
            externalConversationId = "",
            customerDisplayName = "",
            text = "",
            receivedAt = default(DateTimeOffset)
        };

        var response = await client.PostAsJsonAsync("/webhooks/facebook", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("errors").TryGetProperty("ClinicId", out _).Should().BeTrue();
        json.GetProperty("errors").TryGetProperty("ExternalConversationId", out _).Should().BeTrue();
        json.GetProperty("errors").TryGetProperty("CustomerDisplayName", out _).Should().BeTrue();
        json.GetProperty("errors").TryGetProperty("Text", out _).Should().BeTrue();
    }

    private sealed class ClinicMateWebFactory : WebApplicationFactory<Program>
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

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(_appDbName));
                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseInMemoryDatabase(_identityDbName));
            });
        }
    }
}

