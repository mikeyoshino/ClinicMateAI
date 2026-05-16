using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Branches;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Domain.Errors;
using ClinicMateAI.Logic.Branches;
using FluentAssertions;

namespace ClinicMateAI.Tests.Branches;

public class AssignUserToBranchHandlerTests
{
    [Fact]
    public async Task HandleAsync_AssignsBranchAdmin_ToRequestedBranch()
    {
        var clinicId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var profile = new ClinicUserProfile { UserId = "user-1", ClinicId = clinicId, Role = ClinicUserRole.BranchAdmin };
        var assignmentRepository = new FakeAssignmentRepository();
        var handler = new AssignUserToBranchHandler(
            new AssignUserToBranchCommandValidator(),
            new FakeClinicUserProfileRepository(profile),
            new FakeBranchRepository(new Branch { Id = branchId, ClinicId = clinicId, Name = "สุขุมวิท", Address = "Bangkok", Phone = "02", MapUrl = "map", BusinessHours = "Mon" }),
            assignmentRepository,
            new FakeUnitOfWork());

        await handler.HandleAsync(new AssignUserToBranchCommand(clinicId, "user-1", branchId));

        assignmentRepository.Items.Should().ContainSingle(x => x.UserId == "user-1" && x.BranchId == branchId);
    }

    [Fact]
    public async Task HandleAsync_Throws_WhenUserBelongsToAnotherClinic()
    {
        var handler = new AssignUserToBranchHandler(
            new AssignUserToBranchCommandValidator(),
            new FakeClinicUserProfileRepository(new ClinicUserProfile
            {
                UserId = "user-1",
                ClinicId = Guid.NewGuid(),
                Role = ClinicUserRole.BranchAdmin
            }),
            new FakeBranchRepository(new Branch
            {
                Id = Guid.NewGuid(),
                ClinicId = Guid.NewGuid(),
                Name = "สุขุมวิท",
                Address = "Bangkok",
                Phone = "02",
                MapUrl = "map",
                BusinessHours = "Mon"
            }),
            new FakeAssignmentRepository(),
            new FakeUnitOfWork());

        var act = () => handler.HandleAsync(new AssignUserToBranchCommand(Guid.NewGuid(), "user-1", Guid.NewGuid()));

        await act.Should().ThrowAsync<BusinessException>()
            .Where(x => x.Code == BusinessErrorCode.AccessDenied);
    }

    private sealed class FakeClinicUserProfileRepository(ClinicUserProfile? profile) : IClinicUserProfileRepository
    {
        public Task<ClinicUserProfile?> GetByUserAndClinicAsync(string userId, Guid clinicId, CancellationToken cancellationToken = default)
            => Task.FromResult(profile is not null && profile.UserId == userId && profile.ClinicId == clinicId ? profile : null);

        public Task AddAsync(ClinicUserProfile profile, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Update(ClinicUserProfile profile) { }
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

    private sealed class FakeAssignmentRepository : IUserBranchAssignmentRepository
    {
        public List<UserBranchAssignment> Items { get; } = [];

        public Task<IReadOnlyList<Guid>> GetBranchIdsForUserAsync(string userId, Guid clinicId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Guid>>(Items.Where(x => x.UserId == userId && x.ClinicId == clinicId).Select(x => x.BranchId).ToList());

        public Task AddAsync(UserBranchAssignment assignment, CancellationToken cancellationToken = default)
        {
            Items.Add(assignment);
            return Task.CompletedTask;
        }

        public Task<bool> IsAssignedAsync(string userId, Guid branchId, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.Any(x => x.UserId == userId && x.BranchId == branchId));

        public Task RemoveAsync(string userId, Guid branchId, CancellationToken cancellationToken = default)
        {
            Items.RemoveAll(x => x.UserId == userId && x.BranchId == branchId);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    }
}
