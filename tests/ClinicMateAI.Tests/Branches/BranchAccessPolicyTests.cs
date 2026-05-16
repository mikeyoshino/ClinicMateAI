using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Logic.Branches;
using FluentAssertions;

namespace ClinicMateAI.Tests.Branches;

public class BranchAccessPolicyTests
{
    [Fact]
    public async Task GetAccessibleBranchIdsAsync_ReturnsAllBranches_ForOwner()
    {
        var clinicId = Guid.NewGuid();
        var profile = new ClinicUserProfile
        {
            UserId = "owner",
            ClinicId = clinicId,
            Role = ClinicUserRole.Owner
        };
        var branches = new[]
        {
            new Branch { Id = Guid.NewGuid(), ClinicId = clinicId, Name = "A", Address = "A", Phone = "1", MapUrl = "map-a", BusinessHours = "Mon" },
            new Branch { Id = Guid.NewGuid(), ClinicId = clinicId, Name = "B", Address = "B", Phone = "2", MapUrl = "map-b", BusinessHours = "Mon" },
            new Branch { Id = Guid.NewGuid(), ClinicId = clinicId, Name = "C", Address = "C", Phone = "3", MapUrl = "map-c", BusinessHours = "Mon" }
        };
        var policy = new BranchAccessPolicy(
            new FakeClinicUserProfileRepository(profile),
            new FakeAssignmentRepository(),
            new FakeBranchRepository(branches));

        var branchIds = await policy.GetAccessibleBranchIdsAsync("owner", clinicId);

        branchIds.Should().HaveCount(3);
    }

    [Fact]
    public async Task CanAccessBranchAsync_ReturnsFalse_ForUnassignedStaffBranch()
    {
        var clinicId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var profile = new ClinicUserProfile
        {
            UserId = "staff-1",
            ClinicId = clinicId,
            Role = ClinicUserRole.Staff
        };
        var policy = new BranchAccessPolicy(
            new FakeClinicUserProfileRepository(profile),
            new FakeAssignmentRepository(),
            new FakeBranchRepository());

        var allowed = await policy.CanAccessBranchAsync("staff-1", clinicId, branchId);

        allowed.Should().BeFalse();
    }

    private sealed class FakeClinicUserProfileRepository(ClinicUserProfile? profile) : IClinicUserProfileRepository
    {
        public Task<ClinicUserProfile?> GetByUserAndClinicAsync(string userId, Guid clinicId, CancellationToken cancellationToken = default)
            => Task.FromResult(profile is not null && profile.UserId == userId && profile.ClinicId == clinicId ? profile : null);

        public Task AddAsync(ClinicUserProfile profile, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(ClinicUserProfile profile) { }
    }

    private sealed class FakeAssignmentRepository : IUserBranchAssignmentRepository
    {
        public Task<IReadOnlyList<Guid>> GetBranchIdsForUserAsync(string userId, Guid clinicId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Guid>>([]);

        public Task AddAsync(UserBranchAssignment assignment, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> IsAssignedAsync(string userId, Guid branchId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task RemoveAsync(string userId, Guid branchId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeBranchRepository(params Branch[] branches) : IBranchRepository
    {
        private readonly List<Branch> _branches = branches.ToList();

        public Task<Branch?> GetByIdAsync(Guid clinicId, Guid branchId, CancellationToken cancellationToken = default)
            => Task.FromResult(_branches.FirstOrDefault(x => x.ClinicId == clinicId && x.Id == branchId));

        public Task<IReadOnlyList<Branch>> ListByClinicAsync(Guid clinicId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Branch>>(_branches.Where(x => x.ClinicId == clinicId).ToList());

        public Task<Branch?> GetDefaultAsync(Guid clinicId, CancellationToken cancellationToken = default)
            => Task.FromResult(_branches.FirstOrDefault(x => x.ClinicId == clinicId && x.IsDefault));

        public Task AddAsync(Branch branch, CancellationToken cancellationToken = default)
        {
            _branches.Add(branch);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByNameAsync(Guid clinicId, string name, CancellationToken cancellationToken = default)
            => Task.FromResult(_branches.Any(x => x.ClinicId == clinicId && x.Name == name));
    }
}
