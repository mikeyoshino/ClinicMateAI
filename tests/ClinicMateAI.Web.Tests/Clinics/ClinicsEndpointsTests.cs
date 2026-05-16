using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ClinicMateAI.Infrastructure.Data;
using ClinicMateAI.Web.Data;

namespace ClinicMateAI.Web.Tests.Clinics;

public class ClinicsEndpointsTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(99)]
    [InlineData(-1)]
    public async Task CreateClinic_Returns400_WhenPackageTierIsInvalidInteger(int invalidTier)
    {
        await using var factory = new ClinicsWebFactory();
        using var client = factory.CreateClient();

        var payload = new
        {
            name = "Test Clinic",
            address = "123 Test St",
            phone = "0812345678",
            packageTier = invalidTier,
            additionalBranchMonthlyPrice = (decimal?)null
        };

        var response = await client.PostAsJsonAsync("/api/clinics", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("errors").TryGetProperty("packageTier", out _).Should().BeTrue(
            $"expected 'packageTier' validation error for invalid tier value {invalidTier}");
    }

    [Theory]
    [InlineData("")]
    [InlineData("Archived")]
    public async Task CreateClinic_Returns400_WhenStatusIsInvalid(string invalidStatus)
    {
        await using var factory = new ClinicsWebFactory();
        using var client = factory.CreateClient();

        var payload = new
        {
            name = "Test Clinic",
            address = "123 Test St",
            phone = "0812345678",
            status = invalidStatus,
            packageTier = 1,
            additionalBranchMonthlyPrice = (decimal?)null
        };

        var response = await client.PostAsJsonAsync("/api/clinics", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("errors").TryGetProperty("status", out _).Should().BeTrue(
            $"expected 'status' validation error for invalid status value '{invalidStatus}'");
    }

    [Theory]
    [InlineData("name")]
    [InlineData("address")]
    [InlineData("phone")]
    public async Task CreateClinic_Returns400_WhenRequiredFieldIsBlank(string fieldName)
    {
        await using var factory = new ClinicsWebFactory();
        using var client = factory.CreateClient();

        var payload = new Dictionary<string, object?>
        {
            ["name"] = "Test Clinic",
            ["address"] = "123 Test St",
            ["phone"] = "0812345678",
            ["packageTier"] = 1,
            ["additionalBranchMonthlyPrice"] = null
        };

        payload[fieldName] = " ";

        var response = await client.PostAsJsonAsync("/api/clinics", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("errors").TryGetProperty(fieldName, out _).Should().BeTrue(
            $"expected '{fieldName}' validation error for blank required field");
    }

    [Theory]
    [InlineData(1)] // Starter
    [InlineData(2)] // Enterprise
    public async Task CreateClinic_AcceptsValidPackageTierIntegers(int validTier)
    {
        await using var factory = new ClinicsWebFactory();
        using var client = factory.CreateClient();

        var payload = new
        {
            name = "Test Clinic",
            address = "123 Test St",
            phone = "0812345678",
            packageTier = validTier,
            additionalBranchMonthlyPrice = validTier == 2 ? (decimal?)500m : null
        };

        var response = await client.PostAsJsonAsync("/api/clinics", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created,
            $"tier {validTier} is a valid PackageTier and should be created successfully");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.TryGetProperty("clinicId", out var clinicIdProp).Should().BeTrue(
            "response body should contain a clinicId for the created clinic");
        Guid.TryParse(clinicIdProp.GetString(), out var clinicId).Should().BeTrue(
            "clinicId should be a valid GUID");
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"/api/clinics/{clinicId}");
    }

    [Fact]
    public async Task CreateClinic_PersistsEnterprisePricingFields()
    {
        await using var factory = new ClinicsWebFactory();
        using var client = factory.CreateClient();

        var payload = new
        {
            name = "Enterprise Clinic",
            address = "123 Enterprise Ave",
            phone = "0812345678",
            packageTier = 2,
            additionalBranchMonthlyPrice = 3500m
        };

        var response = await client.PostAsJsonAsync("/api/clinics", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        await using var scope = factory.Services.CreateAsyncScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var savedClinic = await appDbContext.Clinics.SingleAsync(x => x.Name == "Enterprise Clinic");

        savedClinic.PackageTier.Should().Be((ClinicMateAI.Domain.Packages.PackageTier)2);
        savedClinic.AdditionalBranchMonthlyPrice.Should().Be(3500m);
    }

    [Fact]
    public async Task CreateClinic_AcceptsStarterBranchPricingInputButNormalizesItAwayBeforePersistence()
    {
        await using var factory = new ClinicsWebFactory();
        using var client = factory.CreateClient();

        var payload = new
        {
            name = "Starter Clinic",
            address = "123 Starter Ave",
            phone = "0812345678",
            packageTier = 1,
            additionalBranchMonthlyPrice = 3500m
        };

        var response = await client.PostAsJsonAsync("/api/clinics", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.TryGetProperty("clinicId", out var clinicIdProp).Should().BeTrue(
            "starter clinic creation should still succeed and return a clinicId even when additionalBranchMonthlyPrice is submitted");
        Guid.TryParse(clinicIdProp.GetString(), out var clinicId).Should().BeTrue(
            "clinicId should be a valid GUID");
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"/api/clinics/{clinicId}");

        await using var scope = factory.Services.CreateAsyncScope();
        var appDbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var savedClinic = await appDbContext.Clinics.SingleAsync(x => x.Name == "Starter Clinic");

        savedClinic.PackageTier.Should().Be((ClinicMateAI.Domain.Packages.PackageTier)1);
        savedClinic.AdditionalBranchMonthlyPrice.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0.0)]
    public async Task CreateClinic_AcceptsEnterpriseWithoutRequiringAdditionalBranchMonthlyPrice(double? additionalBranchMonthlyPrice)
    {
        await using var factory = new ClinicsWebFactory();
        using var client = factory.CreateClient();

        decimal? price = additionalBranchMonthlyPrice.HasValue ? (decimal)additionalBranchMonthlyPrice.Value : null;
        var payload = new
        {
            name = "Enterprise Clinic",
            address = "123 Enterprise Ave",
            phone = "0812345678",
            packageTier = 2, // Enterprise
            additionalBranchMonthlyPrice = price
        };

        var response = await client.PostAsJsonAsync("/api/clinics", payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created,
            $"Enterprise tier should be accepted even when additionalBranchMonthlyPrice={additionalBranchMonthlyPrice}");
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.TryGetProperty("clinicId", out var clinicIdProp).Should().BeTrue(
            "response body should contain a clinicId for the created clinic");
        Guid.TryParse(clinicIdProp.GetString(), out var clinicId).Should().BeTrue(
            "clinicId should be a valid GUID");
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Be($"/api/clinics/{clinicId}");
    }

    [Fact]
    public async Task CreateClinic_Returns400_WhenAdditionalBranchMonthlyPriceIsNegative()
    {
        await using var factory = new ClinicsWebFactory();
        using var client = factory.CreateClient();

        var payload = new
        {
            name = "Enterprise Clinic",
            address = "123 Enterprise Ave",
            phone = "0812345678",
            packageTier = 2,
            additionalBranchMonthlyPrice = -100m
        };

        var response = await client.PostAsJsonAsync("/api/clinics", payload);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        json.GetProperty("errors").TryGetProperty("additionalBranchMonthlyPrice", out _).Should().BeTrue(
            "negative additionalBranchMonthlyPrice should be rejected");
    }

    private sealed class ClinicsWebFactory : WebApplicationFactory<Program>
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
