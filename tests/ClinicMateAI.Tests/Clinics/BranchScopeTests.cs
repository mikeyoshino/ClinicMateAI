using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Domain.Promotions;
using ClinicMateAI.Domain.Services;
using FluentAssertions;

namespace ClinicMateAI.Tests.Clinics;

public class BranchScopeTests
{
    [Fact]
    public void Promotion_AppliesToBranch_ReturnsTrue_ForAllBranchPromotion()
    {
        var promotion = new Promotion
        {
            ClinicId = Guid.NewGuid(),
            BranchId = null,
            Name = "All branches"
        };

        promotion.AppliesToBranch(Guid.NewGuid()).Should().BeTrue();
    }

    [Fact]
    public void ClinicService_AppliesToBranch_ReturnsTrue_OnlyForMatchingBranch_WhenScoped()
    {
        var branchId = Guid.NewGuid();
        var service = new ClinicService
        {
            ClinicId = Guid.NewGuid(),
            BranchId = branchId,
            Name = "Botox"
        };

        service.AppliesToBranch(branchId).Should().BeTrue();
        service.AppliesToBranch(Guid.NewGuid()).Should().BeFalse();
    }

    [Fact]
    public void Branch_DefaultsToActiveStatus()
    {
        var branch = new Branch();

        branch.Status.Should().Be(BranchStatus.Active);
        branch.IsDefault.Should().BeFalse();
    }
}
