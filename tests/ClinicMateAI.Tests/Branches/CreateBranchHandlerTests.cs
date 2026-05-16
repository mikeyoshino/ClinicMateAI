using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Branches;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Domain.Errors;
using ClinicMateAI.Domain.Packages;
using ClinicMateAI.Logic.Branches;
using FluentAssertions;

namespace ClinicMateAI.Tests.Branches;

public class CreateBranchHandlerTests
{
    [Fact]
    public async Task HandleAsync_Throws_WhenStarterClinicAlreadyHasOneBranch()
    {
        var clinic = new Clinic { Id = Guid.NewGuid() };
        clinic.SetPackageContract(PackageTier.Starter, null);
        var existingBranch = new Branch
        {
            ClinicId = clinic.Id,
            Name = "Main",
            Address = "Addr",
            Phone = "02-1",
            MapUrl = "map",
            BusinessHours = "Mon-Sun 10:00-19:00",
            IsDefault = true
        };

        var handler = CreateHandler(clinic, [existingBranch]);

        var act = () => handler.HandleAsync(new CreateBranchCommand(
            clinic.Id,
            "Second",
            "Addr 2",
            "02-2",
            "map-2",
            "Mon-Sun 10:00-19:00"));

        await act.Should().ThrowAsync<BusinessException>()
            .Where(x => x.Code == BusinessErrorCode.BranchLimitExceeded);
    }

    [Fact]
    public async Task HandleAsync_AddsBranch_ForEnterpriseClinic()
    {
        var clinic = new Clinic { Id = Guid.NewGuid() };
        clinic.SetPackageContract(PackageTier.Enterprise, null);
        var branchRepository = new FakeBranchRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CreateBranchHandler(
            new CreateBranchCommandValidator(),
            new FakeClinicRepository(clinic),
            branchRepository,
            unitOfWork);

        var branch = await handler.HandleAsync(new CreateBranchCommand(
            clinic.Id,
            "สุขุมวิท",
            "Bangkok",
            "02-123-4567",
            "https://maps.example/sukhumvit",
            "Mon-Sun 10:00-19:00"));

        branchRepository.Items.Should().ContainSingle(x => x.Name == "สุขุมวิท");
        branch.ClinicId.Should().Be(clinic.Id);
        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }

    private static CreateBranchHandler CreateHandler(Clinic clinic, IEnumerable<Branch> branches)
    {
        return new CreateBranchHandler(
            new CreateBranchCommandValidator(),
            new FakeClinicRepository(clinic),
            new FakeBranchRepository(branches),
            new FakeUnitOfWork());
    }

    private sealed class FakeClinicRepository(Clinic clinic) : IClinicRepository
    {
        public Task<Clinic?> GetByIdAsync(Guid clinicId, CancellationToken cancellationToken = default)
            => Task.FromResult<Clinic?>(clinic.Id == clinicId ? clinic : null);

        public Task<IReadOnlyList<Clinic>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Clinic>>([clinic]);

        public Task AddAsync(Clinic clinic, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdateAsync(Clinic clinic, CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<(IReadOnlyList<Clinic> Items, int TotalCount)> SearchAsync(
            string? name,
            DateTime? createdFromUtc,
            DateTime? createdToExclusiveUtc,
            string? status,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
            => Task.FromResult(((IReadOnlyList<Clinic>)[clinic], 1));
    }

    private sealed class FakeBranchRepository : IBranchRepository
    {
        public List<Branch> Items { get; } = [];

        public FakeBranchRepository()
        {
        }

        public FakeBranchRepository(IEnumerable<Branch> seed)
        {
            Items.AddRange(seed);
        }

        public Task<Branch?> GetByIdAsync(Guid clinicId, Guid branchId, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(x => x.ClinicId == clinicId && x.Id == branchId));

        public Task<IReadOnlyList<Branch>> ListByClinicAsync(Guid clinicId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Branch>>(Items.Where(x => x.ClinicId == clinicId).ToList());

        public Task<Branch?> GetDefaultAsync(Guid clinicId, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(x => x.ClinicId == clinicId && x.IsDefault));

        public Task AddAsync(Branch branch, CancellationToken cancellationToken = default)
        {
            Items.Add(branch);
            return Task.CompletedTask;
        }

        public Task<bool> ExistsByNameAsync(Guid clinicId, string name, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.Any(x => x.ClinicId == clinicId && x.Name == name));
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }
}
