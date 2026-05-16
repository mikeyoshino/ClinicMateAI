using ClinicMateAI.Domain.Packages;

namespace ClinicMateAI.Application.Clinics;

public sealed record CreateClinicCommand(
    string Name,
    string Address,
    string Phone,
    string? MapUrl,
    string Status,
    PackageTier PackageTier,
    decimal? AdditionalBranchMonthlyPrice = null);
