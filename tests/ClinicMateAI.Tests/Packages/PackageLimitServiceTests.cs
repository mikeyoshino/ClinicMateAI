using ClinicMateAI.Domain.Packages;
using ClinicMateAI.Logic.Packages;
using FluentAssertions;

namespace ClinicMateAI.Tests.Packages;

public class PackageLimitServiceTests
{
    [Theory]
    [InlineData(PackageTier.Starter, 1000, 20, 1, 1, 1)]
    [InlineData(PackageTier.Enterprise, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue)]
    public void GetLimits_ReturnsConfiguredQuota(
        PackageTier tier,
        int aiReplies,
        int services,
        int admins,
        int channels,
        int branches)
    {
        var limits = PackageLimitService.GetLimits(tier);

        limits.MonthlyAiReplies.Should().Be(aiReplies);
        limits.MaxServices.Should().Be(services);
        limits.MaxAdminSeats.Should().Be(admins);
        limits.MaxChannels.Should().Be(channels);
        limits.MaxBranches.Should().Be(branches);
    }

    [Theory]
    [InlineData(999, false)]
    [InlineData(1000, false)]
    [InlineData(1001, true)]
    public void IsOverAiReplyQuota_StarterBoundary(int usage, bool expected)
    {
        var result = PackageLimitService.IsOverAiReplyQuota(PackageTier.Starter, usage);

        result.Should().Be(expected);
    }

    [Fact]
    public void GetLimits_Throws_WhenPackageTierIsUnsupported()
    {
        var act = () => PackageLimitService.GetLimits((PackageTier)999);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("tier");
    }
}
