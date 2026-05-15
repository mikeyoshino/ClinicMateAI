namespace ClinicMateAI.Domain.Packages;

public sealed record PackageLimit(
    PackageTier Tier,
    int MonthlyAiReplies,
    int MaxServices,
    int MaxAdminSeats,
    int MaxChannels,
    int MaxBranches);
