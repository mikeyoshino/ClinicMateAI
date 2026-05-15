using ClinicMateAI.Domain.Packages;
using ClinicMateAI.Logic.Packages;
using FluentAssertions;

namespace ClinicMateAI.Tests.Packages;

public class PackageLimitServiceTests
{
    [Theory]
    [InlineData(PackageTier.Starter, 1000, 20, 1)]
    [InlineData(PackageTier.Growth, 3000, 50, 3)]
    [InlineData(PackageTier.ProClinic, 8000, int.MaxValue, 10)]
    public void GetLimits_ReturnsConfiguredQuota(PackageTier tier, int aiReplies, int services, int admins)
    {
        var limits = PackageLimitService.GetLimits(tier);

        limits.MonthlyAiReplies.Should().Be(aiReplies);
        limits.MaxServices.Should().Be(services);
        limits.MaxAdminSeats.Should().Be(admins);
    }

    [Fact]
    public void IsOverAiReplyQuota_ReturnsTrueWhenUsageExceedsLimit()
    {
        var result = PackageLimitService.IsOverAiReplyQuota(PackageTier.Starter, 1001);

        result.Should().BeTrue();
    }
}
