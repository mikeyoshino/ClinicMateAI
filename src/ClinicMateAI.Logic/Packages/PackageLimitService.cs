using ClinicMateAI.Domain.Packages;

namespace ClinicMateAI.Logic.Packages;

public static class PackageLimitService
{
    public static PackageLimit GetLimits(PackageTier tier) => tier switch
    {
        PackageTier.Starter => new PackageLimit(tier, 1000, 20, 1, 1, 1),
        PackageTier.Growth => new PackageLimit(tier, 3000, 50, 3, 2, 1),
        PackageTier.ProClinic => new PackageLimit(tier, 8000, int.MaxValue, 10, 3, int.MaxValue),
        PackageTier.Enterprise => new PackageLimit(tier, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue),
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unsupported package tier.")
    };

    public static bool IsOverAiReplyQuota(PackageTier tier, int monthlyAiRepliesUsed)
    {
        var limit = GetLimits(tier);
        return monthlyAiRepliesUsed > limit.MonthlyAiReplies;
    }
}
