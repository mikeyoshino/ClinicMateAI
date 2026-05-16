using ClinicMateAI.Domain.Packages;

namespace ClinicMateAI.Domain.Clinics;

public sealed class Clinic
{
    private PackageTier _packageTier = PackageTier.Starter;
    private decimal? _additionalBranchMonthlyPrice;

    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ClinicStatus Status { get; set; } = ClinicStatus.Active;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string MapUrl { get; set; } = string.Empty;

    public PackageTier PackageTier
    {
        get => _packageTier;
        private set
        {
            EnsureSupportedPackageTier(value, nameof(value));
            _packageTier = value;
        }
    }

    public decimal? AdditionalBranchMonthlyPrice
    {
        get => _packageTier == PackageTier.Enterprise ? _additionalBranchMonthlyPrice : null;
        private set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Additional branch monthly price cannot be negative.");
            }

            _additionalBranchMonthlyPrice = value;
        }
    }

    public void SetPackageContract(PackageTier packageTier, decimal? additionalBranchMonthlyPrice)
    {
        EnsureSupportedPackageTier(packageTier, nameof(packageTier));
        if (additionalBranchMonthlyPrice < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(additionalBranchMonthlyPrice),
                additionalBranchMonthlyPrice,
                "Additional branch monthly price cannot be negative.");
        }

        PackageTier = packageTier;
        AdditionalBranchMonthlyPrice = packageTier == PackageTier.Enterprise
            ? additionalBranchMonthlyPrice
            : null;
    }

    private static void EnsureSupportedPackageTier(PackageTier value, string parameterName)
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Unsupported package tier.");
        }
    }
}
