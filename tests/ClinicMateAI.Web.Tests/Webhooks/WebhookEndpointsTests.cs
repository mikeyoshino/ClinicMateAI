using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
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
    [Fact]
    public async Task LineWebhook_PersistsConversationAndMessage()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var clinicId = Guid.NewGuid();
        var payload = new
        {
            clinicId,
            externalConversationId = "line-conv-1",
            customerDisplayName = "Customer A",
            text = "โบท็อกกรามเท่าไรคะ",
            receivedAt = DateTimeOffset.UtcNow
        };

        var response = await client.PostAsJsonAsync("/webhooks/line", payload);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Conversations.Should().ContainSingle(x =>
            x.ClinicId == clinicId
            && x.Channel == "LINE"
            && x.ExternalConversationId == "line-conv-1");

        var conversation = db.Conversations.Single(x => x.ClinicId == clinicId && x.ExternalConversationId == "line-conv-1");
        db.Messages.Should().ContainSingle(x =>
            x.ClinicId == clinicId
            && x.ConversationId == conversation.Id
            && x.Text == "โบท็อกกรามเท่าไรคะ");
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
