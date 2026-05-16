using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Domain.Promotions;
using ClinicMateAI.Logic.Promotions;
using FluentAssertions;

namespace ClinicMateAI.Tests.Promotions;

public class PromotionServiceTests
{
    [Fact]
    public async Task PublishAsync_ChangesStatusToPublished_AndSaves()
    {
        var clinicId = Guid.NewGuid();
        var promotion = new Promotion
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            Name = "Draft Promo",
            StartsOn = new DateOnly(2026, 6, 1),
            EndsOn = new DateOnly(2026, 6, 30),
            Status = PromotionStatus.Draft
        };
        var repository = new InMemoryPromotionRepository([promotion]);
        var unitOfWork = new FakeUnitOfWork();
        var service = new PromotionService(repository, unitOfWork);

        await service.PublishAsync(clinicId, promotion.Id);

        repository.Items.Single().Status.Should().Be(PromotionStatus.Published);
        unitOfWork.SaveCalls.Should().Be(1);
    }

    [Fact]
    public async Task DisableAsync_ChangesStatusToDisabled_AndSaves()
    {
        var clinicId = Guid.NewGuid();
        var promotion = new Promotion
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            Name = "Published Promo",
            StartsOn = new DateOnly(2026, 5, 1),
            EndsOn = new DateOnly(2026, 5, 31),
            Status = PromotionStatus.Published
        };
        var repository = new InMemoryPromotionRepository([promotion]);
        var unitOfWork = new FakeUnitOfWork();
        var service = new PromotionService(repository, unitOfWork);

        await service.DisableAsync(clinicId, promotion.Id);

        repository.Items.Single().Status.Should().Be(PromotionStatus.Disabled);
        unitOfWork.SaveCalls.Should().Be(1);
    }

    [Fact]
    public async Task ListByClinicAsync_ReturnsAllStatuses_ForClinic()
    {
        var clinicId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var repository = new InMemoryPromotionRepository(
        [
            new Promotion
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicId,
                BranchId = branchId,
                Name = "Published",
                StartsOn = new DateOnly(2026, 5, 1),
                EndsOn = new DateOnly(2026, 5, 31),
                Status = PromotionStatus.Published
            },
            new Promotion
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicId,
                BranchId = null,
                Name = "Draft",
                StartsOn = new DateOnly(2026, 6, 1),
                EndsOn = new DateOnly(2026, 6, 30),
                Status = PromotionStatus.Draft
            }
        ]);
        var unitOfWork = new FakeUnitOfWork();
        var service = new PromotionService(repository, unitOfWork);

        var result = await service.ListByClinicAsync(clinicId);

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task ListByClinicAsync_WithBranchFilter_ReturnsSharedAndBranchPromotions()
    {
        var clinicId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var repository = new InMemoryPromotionRepository(
        [
            new Promotion
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicId,
                BranchId = branchId,
                Name = "Branch Promo",
                StartsOn = new DateOnly(2026, 5, 1),
                EndsOn = new DateOnly(2026, 5, 31),
                Status = PromotionStatus.Published
            },
            new Promotion
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicId,
                BranchId = null,
                Name = "Shared Promo",
                StartsOn = new DateOnly(2026, 6, 1),
                EndsOn = new DateOnly(2026, 6, 30),
                Status = PromotionStatus.Draft
            },
            new Promotion
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicId,
                BranchId = Guid.NewGuid(),
                Name = "Other Branch Promo",
                StartsOn = new DateOnly(2026, 6, 1),
                EndsOn = new DateOnly(2026, 6, 30),
                Status = PromotionStatus.Draft
            }
        ]);
        var unitOfWork = new FakeUnitOfWork();
        var service = new PromotionService(repository, unitOfWork);

        var result = await service.ListByClinicAsync(clinicId, branchId);

        result.Select(x => x.Name).Should().BeEquivalentTo(["Branch Promo", "Shared Promo"]);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveCalls { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveCalls++;
            return Task.FromResult(1);
        }
    }

    private sealed class InMemoryPromotionRepository(IEnumerable<Promotion> seed) : IPromotionRepository
    {
        public List<Promotion> Items { get; } = seed.ToList();

        public Task<IReadOnlyList<Promotion>> ListByClinicAsync(Guid clinicId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Promotion> result = Items.Where(x => x.ClinicId == clinicId).ToList();
            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<Promotion>> ListByClinicAsync(Guid clinicId, Guid? branchId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Promotion> result = Items
                .Where(x => x.ClinicId == clinicId && (branchId is null || x.BranchId is null || x.BranchId == branchId))
                .ToList();
            return Task.FromResult(result);
        }

        public Task<Promotion?> GetByIdAsync(Guid clinicId, Guid promotionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Items.FirstOrDefault(x => x.ClinicId == clinicId && x.Id == promotionId));
        }

        public Task<Promotion?> GetByIdAsync(Guid clinicId, Guid promotionId, Guid? branchId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Items.FirstOrDefault(x =>
                x.ClinicId == clinicId
                && x.Id == promotionId
                && (branchId is null || x.BranchId is null || x.BranchId == branchId)));
        }

        public Task AddAsync(Promotion promotion, CancellationToken cancellationToken = default)
        {
            Items.Add(promotion);
            return Task.CompletedTask;
        }

        public void Update(Promotion promotion)
        {
            var index = Items.FindIndex(x => x.Id == promotion.Id);
            if (index >= 0)
            {
                Items[index] = promotion;
            }
        }
    }
}
