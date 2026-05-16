using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Domain.Packages;
using FluentAssertions;

namespace ClinicMateAI.Tests.Clinics;

public class ClinicTests
{
    [Fact]
    public void SetPackageContract_SetsEnterpriseTierAndBranchPriceTogether()
    {
        var clinic = new Clinic();

        clinic.SetPackageContract(PackageTier.Enterprise, 3500m);

        clinic.PackageTier.Should().Be(PackageTier.Enterprise);
        clinic.AdditionalBranchMonthlyPrice.Should().Be(3500m);
    }

    [Fact]
    public void SetPackageContract_Throws_WhenAdditionalBranchMonthlyPriceIsNegative()
    {
        var clinic = new Clinic();

        var act = () => clinic.SetPackageContract(PackageTier.Enterprise, -1m);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithMessage("*cannot be negative*");
    }
}
