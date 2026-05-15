using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Promotions;
using ClinicMateAI.Domain.Promotions;
using ClinicMateAI.Logic.Promotions;
using FluentAssertions;

namespace ClinicMateAI.Tests.Promotions;

public class GetAvailablePromotionsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsOnlyPublishedAndActivePromotionsForClinic()
    {
        var clinicId = Guid.NewGuid();
        var otherClinicId = Guid.NewGuid();
        var today = new DateOnly(2026, 5, 15);
        var repository = new FakePromotionRepository(
        [
            new Promotion
            {
                ClinicId = clinicId,
                Name = "Published Active",
                RelatedServiceName = "Botox Jaw",
                PromoPrice = 2999,
                StartsOn = new DateOnly(2026, 5, 1),
                EndsOn = new DateOnly(2026, 5, 31),
                Status = PromotionStatus.Published
            },
            new Promotion
            {
                ClinicId = clinicId,
                Name = "Draft",
                StartsOn = new DateOnly(2026, 5, 1),
                EndsOn = new DateOnly(2026, 5, 31),
                Status = PromotionStatus.Draft
            },
            new Promotion
            {
                ClinicId = clinicId,
                Name = "Expired",
                StartsOn = new DateOnly(2026, 4, 1),
                EndsOn = new DateOnly(2026, 4, 30),
                Status = PromotionStatus.Published
            },
            new Promotion
            {
                ClinicId = otherClinicId,
                Name = "Other Clinic",
                StartsOn = new DateOnly(2026, 5, 1),
                EndsOn = new DateOnly(2026, 5, 31),
                Status = PromotionStatus.Published
            }
        ]);
        var handler = new GetAvailablePromotionsHandler(repository);

        var result = await handler.HandleAsync(new GetAvailablePromotionsQuery(clinicId, today));

        result.Should().ContainSingle();
        result[0].Name.Should().Be("Published Active");
        result[0].PromoPrice.Should().Be(2999);
    }

    private sealed class FakePromotionRepository(IEnumerable<Promotion> seed) : IPromotionRepository
    {
        private readonly List<Promotion> _items = seed.ToList();

        public Task<IReadOnlyList<Promotion>> ListByClinicAsync(Guid clinicId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Promotion> result = _items.Where(x => x.ClinicId == clinicId).ToList();
            return Task.FromResult(result);
        }

        public Task<Promotion?> GetByIdAsync(Guid clinicId, Guid promotionId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_items.FirstOrDefault(x => x.ClinicId == clinicId && x.Id == promotionId));
        }

        public Task AddAsync(Promotion promotion, CancellationToken cancellationToken = default)
        {
            _items.Add(promotion);
            return Task.CompletedTask;
        }

        public void Update(Promotion promotion)
        {
            var index = _items.FindIndex(x => x.Id == promotion.Id);
            if (index >= 0)
            {
                _items[index] = promotion;
            }
        }
    }
}
