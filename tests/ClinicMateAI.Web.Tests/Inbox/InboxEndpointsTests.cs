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

public class InboxEndpointsTests
{
    [Fact]
    public async Task GetClinics_ReturnsSeededClinicList()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/inbox/clinics");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var clinics = await response.Content.ReadFromJsonAsync<JsonElement>();
        clinics.GetArrayLength().Should().BeGreaterThan(0);
        clinics[0].TryGetProperty("clinicId", out _).Should().BeTrue();
        clinics[0].TryGetProperty("name", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetConversationsAndMessages_ReturnsTenantScopedData()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var clinicA = Guid.NewGuid();
        var clinicB = Guid.NewGuid();

        var aResponse = await client.PostAsJsonAsync("/webhooks/line", new
        {
            clinicId = clinicA,
            externalConversationId = "line-a",
            customerDisplayName = "A",
            text = "A message",
            receivedAt = DateTimeOffset.UtcNow
        });
        aResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var bResponse = await client.PostAsJsonAsync("/webhooks/line", new
        {
            clinicId = clinicB,
            externalConversationId = "line-b",
            customerDisplayName = "B",
            text = "B message",
            receivedAt = DateTimeOffset.UtcNow
        });
        bResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var aJson = await aResponse.Content.ReadFromJsonAsync<JsonElement>();
        var aConversationId = aJson.GetProperty("conversationId").GetGuid();

        var conversationsResponse = await client.GetAsync($"/api/inbox/conversations?clinicId={clinicA}&take=20");
        conversationsResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var conversations = await conversationsResponse.Content.ReadFromJsonAsync<JsonElement>();
        conversations.GetArrayLength().Should().Be(1);
        conversations[0].GetProperty("externalConversationId").GetString().Should().Be("line-a");

        var messagesResponse = await client.GetAsync($"/api/inbox/conversations/{aConversationId}/messages?clinicId={clinicA}");
        messagesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var messages = await messagesResponse.Content.ReadFromJsonAsync<JsonElement>();
        messages.GetArrayLength().Should().Be(1);
        messages[0].GetProperty("text").GetString().Should().Be("A message");
    }

    [Fact]
    public async Task GetConversations_ReturnsMostRecentFirst_AndHonorsTake()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var clinicId = Guid.NewGuid();

        var oldResponse = await client.PostAsJsonAsync("/webhooks/line", new
        {
            clinicId,
            externalConversationId = "line-old",
            customerDisplayName = "Old",
            text = "old message",
            receivedAt = DateTimeOffset.UtcNow.AddMinutes(-5)
        });
        oldResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var newResponse = await client.PostAsJsonAsync("/webhooks/line", new
        {
            clinicId,
            externalConversationId = "line-new",
            customerDisplayName = "New",
            text = "new message",
            receivedAt = DateTimeOffset.UtcNow
        });
        newResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var response = await client.GetAsync($"/api/inbox/conversations?clinicId={clinicId}&take=1");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var conversations = await response.Content.ReadFromJsonAsync<JsonElement>();
        conversations.GetArrayLength().Should().Be(1);
        conversations[0].GetProperty("externalConversationId").GetString().Should().Be("line-new");
    }

    [Fact]
    public async Task GetConversations_ReturnsValidationProblem_WhenClinicIdMissing()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/inbox/conversations?clinicId=00000000-0000-0000-0000-000000000000");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("errors").TryGetProperty("clinicId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetConversations_ReturnsValidationProblem_WhenTakeIsOutOfRange()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();
        var clinicId = Guid.NewGuid();

        var responseTooLow = await client.GetAsync($"/api/inbox/conversations?clinicId={clinicId}&take=0");
        responseTooLow.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var bodyTooLow = await responseTooLow.Content.ReadFromJsonAsync<JsonElement>();
        bodyTooLow.GetProperty("errors").TryGetProperty("take", out _).Should().BeTrue();

        var responseTooHigh = await client.GetAsync($"/api/inbox/conversations?clinicId={clinicId}&take=201");
        responseTooHigh.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var bodyTooHigh = await responseTooHigh.Content.ReadFromJsonAsync<JsonElement>();
        bodyTooHigh.GetProperty("errors").TryGetProperty("take", out _).Should().BeTrue();
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
