using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Clinics;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Domain.Packages;
using ClinicMateAI.Logic.Clinics;
using FluentAssertions;

namespace ClinicMateAI.Tests.Clinics;

public class CreateClinicHandlerTests
{
    [Fact]
    public async Task HandleAsync_Throws_WhenStatusIsUnsupported()
    {
        var repository = new FakeClinicRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CreateClinicHandler(repository, unitOfWork);

        var act = () => handler.HandleAsync(new CreateClinicCommand(
            Name: "Broken Clinic",
            Address: "Bangkok",
            Phone: "02-111-1111",
            MapUrl: null,
            Status: "Archived",
            PackageTier: PackageTier.Starter,
            AdditionalBranchMonthlyPrice: null));

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Status*");
        repository.SavedClinic.Should().BeNull();
        unitOfWork.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_Throws_WhenPackageTierIsUnsupported()
    {
        var repository = new FakeClinicRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CreateClinicHandler(repository, unitOfWork);

        var act = () => handler.HandleAsync(new CreateClinicCommand(
            Name: "Broken Clinic",
            Address: "Bangkok",
            Phone: "02-111-1111",
            MapUrl: null,
            Status: "Active",
            PackageTier: (PackageTier)999,
            AdditionalBranchMonthlyPrice: 3500m));

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*Unsupported package tier.*");
        repository.SavedClinic.Should().BeNull();
        unitOfWork.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_SetsEnterpriseBranchPricing_WhenEnterprisePackageIsSelected()
    {
        var repository = new FakeClinicRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CreateClinicHandler(repository, unitOfWork);

        var result = await handler.HandleAsync(new CreateClinicCommand(
            Name: "Chain Clinic",
            Address: "Bangkok",
            Phone: "02-000-0000",
            MapUrl: "https://maps.example/chain",
            Status: "Active",
            PackageTier: PackageTier.Enterprise,
            AdditionalBranchMonthlyPrice: 3500m));

        result.ClinicId.Should().NotBeEmpty();
        repository.SavedClinic.Should().NotBeNull();
        repository.SavedClinic!.PackageTier.Should().Be(PackageTier.Enterprise);
        repository.SavedClinic.AdditionalBranchMonthlyPrice.Should().Be(3500m);
        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_ClearsAdditionalBranchPricing_WhenStarterPackageIsSelected()
    {
        var repository = new FakeClinicRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new CreateClinicHandler(repository, unitOfWork);

        await handler.HandleAsync(new CreateClinicCommand(
            Name: "Solo Clinic",
            Address: "Bangkok",
            Phone: "02-999-0000",
            MapUrl: null,
            Status: "Active",
            PackageTier: PackageTier.Starter,
            AdditionalBranchMonthlyPrice: 3500m));

        repository.SavedClinic.Should().NotBeNull();
        repository.SavedClinic!.PackageTier.Should().Be(PackageTier.Starter);
        repository.SavedClinic.AdditionalBranchMonthlyPrice.Should().BeNull();
    }

    private sealed class FakeClinicRepository : IClinicRepository
    {
        public Clinic? SavedClinic { get; private set; }

        public Task<Clinic?> GetByIdAsync(Guid clinicId, CancellationToken cancellationToken = default)
            => Task.FromResult<Clinic?>(SavedClinic?.Id == clinicId ? SavedClinic : null);

        public Task<IReadOnlyList<Clinic>> ListAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<Clinic>>(SavedClinic is null ? [] : [SavedClinic]);

        public Task AddAsync(Clinic clinic, CancellationToken cancellationToken = default)
        {
            SavedClinic = clinic;
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Clinic clinic, CancellationToken cancellationToken = default)
        {
            SavedClinic = clinic;
            return Task.CompletedTask;
        }

        public Task<(IReadOnlyList<Clinic> Items, int TotalCount)> SearchAsync(
            string? name,
            DateTime? createdFromUtc,
            DateTime? createdToExclusiveUtc,
            string? status,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Clinic> items = SavedClinic is null ? [] : [SavedClinic];
            return Task.FromResult((items, items.Count));
        }
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
