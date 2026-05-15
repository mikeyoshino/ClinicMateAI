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

namespace ClinicMateAI.Web.Tests.Promotions;

public class PromotionsEndpointsTests
{
    [Fact]
    public async Task GetAvailablePromotions_ReturnsOnlyAiAvailablePromotions()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var response = await client.GetAsync($"/api/promotions/available?clinicId={clinicId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var promotions = await response.Content.ReadFromJsonAsync<JsonElement>();
        promotions.GetArrayLength().Should().Be(1);
        promotions[0].GetProperty("name").GetString().Should().Be("Botox Jaw New Customer");
    }

    [Fact]
    public async Task GetAvailablePromotions_ReturnsValidationProblem_WhenClinicIdMissing()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/promotions/available?clinicId=00000000-0000-0000-0000-000000000000");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("errors").TryGetProperty("clinicId", out _).Should().BeTrue();
    }

    [Fact]
    public async Task PublishAndDisablePromotion_UpdatesManageListStatus()
    {
        await using var factory = new ClinicMateWebFactory();
        using var client = factory.CreateClient();

        var clinicId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var listResponse = await client.GetAsync($"/api/promotions/manage?clinicId={clinicId}");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var list = await listResponse.Content.ReadFromJsonAsync<JsonElement>();
        var draftPromotion = list.EnumerateArray().First(x => x.GetProperty("status").GetInt32() == 1);
        var promotionId = draftPromotion.GetProperty("promotionId").GetGuid();

        var publishResponse = await client.PostAsync($"/api/promotions/{promotionId}/publish?clinicId={clinicId}", content: null);
        publishResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterPublishResponse = await client.GetAsync($"/api/promotions/manage?clinicId={clinicId}");
        afterPublishResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var afterPublish = await afterPublishResponse.Content.ReadFromJsonAsync<JsonElement>();
        var published = afterPublish.EnumerateArray().First(x => x.GetProperty("promotionId").GetGuid() == promotionId);
        published.GetProperty("status").GetInt32().Should().Be(2);

        var disableResponse = await client.PostAsync($"/api/promotions/{promotionId}/disable?clinicId={clinicId}", content: null);
        disableResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var afterDisableResponse = await client.GetAsync($"/api/promotions/manage?clinicId={clinicId}");
        afterDisableResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var afterDisable = await afterDisableResponse.Content.ReadFromJsonAsync<JsonElement>();
        var disabled = afterDisable.EnumerateArray().First(x => x.GetProperty("promotionId").GetGuid() == promotionId);
        disabled.GetProperty("status").GetInt32().Should().Be(3);
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
