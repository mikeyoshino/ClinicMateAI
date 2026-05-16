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

namespace ClinicMateAI.Web.Tests.Inbox;

public class InboxActionEndpointsTests
{
    // ── POST /read ────────────────────────────────────────────────────────

    [Fact]
    public async Task MarkRead_Returns200_AndSetsIsRead()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var clinicId = Guid.NewGuid();

        // Create a conversation via webhook
        var webhookResp = await client.PostAsJsonAsync("/webhooks/facebook", new
        {
            clinicId,
            externalConversationId = "line-read-1",
            customerDisplayName = "Customer",
            text = "hello",
            receivedAt = DateTimeOffset.UtcNow
        });
        webhookResp.StatusCode.Should().Be(HttpStatusCode.OK);

        var convId = (await webhookResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("conversationId").GetGuid();

        // Mark as read
        var readResp = await client.PostAsync(
            $"/api/inbox/conversations/{convId}/read?clinicId={clinicId}", null);

        readResp.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var conv = db.Conversations.Single(x => x.Id == convId);
        conv.IsRead.Should().BeTrue();
        conv.UnreadCount.Should().Be(0);
    }

    [Fact]
    public async Task MarkRead_ReturnsBadRequest_WhenClinicIdMissing()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var resp = await client.PostAsync(
            $"/api/inbox/conversations/{Guid.NewGuid()}/read?clinicId={Guid.Empty}", null);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── POST /claim ───────────────────────────────────────────────────────

    [Fact]
    public async Task ClaimConversation_Returns200_AndAssignsStaff()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var clinicId = Guid.NewGuid();
        var webhookResp = await client.PostAsJsonAsync("/webhooks/facebook", new
        {
            clinicId,
            externalConversationId = "line-claim-1",
            customerDisplayName = "Customer",
            text = "hello",
            receivedAt = DateTimeOffset.UtcNow
        });
        webhookResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var convId = (await webhookResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("conversationId").GetGuid();

        var claimResp = await client.PostAsync(
            $"/api/inbox/conversations/{convId}/claim?clinicId={clinicId}&staffName=Alice", null);

        claimResp.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var conv = db.Conversations.Single(x => x.Id == convId);
        conv.AssignedStaff.Should().Be("Alice");
        conv.Status.Should().Be("InProgress");
    }

    [Fact]
    public async Task ClaimConversation_Returns409_WhenAlreadyClaimedByOther()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var clinicId = Guid.NewGuid();
        var webhookResp = await client.PostAsJsonAsync("/webhooks/facebook", new
        {
            clinicId,
            externalConversationId = "line-claim-2",
            customerDisplayName = "Customer",
            text = "hello",
            receivedAt = DateTimeOffset.UtcNow
        });
        var convId = (await webhookResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("conversationId").GetGuid();

        // Bob claims first
        await client.PostAsync(
            $"/api/inbox/conversations/{convId}/claim?clinicId={clinicId}&staffName=Bob", null);

        // Alice tries to claim
        var aliceResp = await client.PostAsync(
            $"/api/inbox/conversations/{convId}/claim?clinicId={clinicId}&staffName=Alice", null);

        aliceResp.StatusCode.Should().Be(HttpStatusCode.Conflict);
        var body = await aliceResp.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("conflict").GetString().Should().Be("Bob");
    }

    [Fact]
    public async Task ClaimConversation_ReturnsBadRequest_WhenStaffNameMissing()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var resp = await client.PostAsync(
            $"/api/inbox/conversations/{Guid.NewGuid()}/claim?clinicId={Guid.NewGuid()}&staffName=", null);

        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── POST /release ─────────────────────────────────────────────────────

    [Fact]
    public async Task ReleaseConversation_Returns200_AndClearsClaim()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var clinicId = Guid.NewGuid();
        var webhookResp = await client.PostAsJsonAsync("/webhooks/facebook", new
        {
            clinicId,
            externalConversationId = "line-release-1",
            customerDisplayName = "Customer",
            text = "hello",
            receivedAt = DateTimeOffset.UtcNow
        });
        var convId = (await webhookResp.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("conversationId").GetGuid();

        // Claim first
        await client.PostAsync(
            $"/api/inbox/conversations/{convId}/claim?clinicId={clinicId}&staffName=Alice", null);

        // Release
        var releaseResp = await client.PostAsync(
            $"/api/inbox/conversations/{convId}/release?clinicId={clinicId}", null);

        releaseResp.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var conv = db.Conversations.Single(x => x.Id == convId);
        conv.AssignedStaff.Should().BeNull();
        conv.Status.Should().Be("Open");
    }

    // ── GET /conversations — new DTO fields ───────────────────────────────

    [Fact]
    public async Task GetConversations_ReturnsNewDtoFields()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var clinicId = Guid.NewGuid();
        await client.PostAsJsonAsync("/webhooks/facebook", new
        {
            clinicId,
            externalConversationId = "line-fields-1",
            customerDisplayName = "Customer",
            text = "สวัสดี",
            receivedAt = DateTimeOffset.UtcNow
        });

        var resp = await client.GetAsync($"/api/inbox/conversations?clinicId={clinicId}&take=10");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await resp.Content.ReadFromJsonAsync<JsonElement>();
        var conv = json[0];

        conv.TryGetProperty("isRead", out _).Should().BeTrue();
        conv.TryGetProperty("unreadCount", out _).Should().BeTrue();
        conv.TryGetProperty("aiStatus", out _).Should().BeTrue();
        conv.GetProperty("unreadCount").GetInt32().Should().Be(1);
    }

    // ── Idempotency ───────────────────────────────────────────────────────

    [Fact]
    public async Task ReceiveMessage_IsIdempotent_WhenExternalMessageIdRepeated()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var clinicId = Guid.NewGuid();
        var payload = new
        {
            clinicId,
            externalConversationId = "line-idempotent",
            customerDisplayName = "Customer",
            text = "hello",
            receivedAt = DateTimeOffset.UtcNow,
            externalMessageId = "msg-unique-99"
        };

        // Send twice (simulating LINE retry)
        var r1 = await client.PostAsJsonAsync("/webhooks/facebook", payload);
        var r2 = await client.PostAsJsonAsync("/webhooks/facebook", payload);

        r1.StatusCode.Should().Be(HttpStatusCode.OK);
        r2.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Messages.Count(m => m.ClinicId == clinicId && m.ExternalMessageId == "msg-unique-99")
            .Should().Be(1, "duplicate external message IDs must be ignored");
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
