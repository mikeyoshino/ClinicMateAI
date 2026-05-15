using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Promotions;

namespace ClinicMateAI.Logic.Promotions;

public sealed class GetAvailablePromotionsHandler(
    IPromotionRepository promotionRepository) : IGetAvailablePromotionsHandler
{
    public async Task<IReadOnlyList<AvailablePromotionDto>> HandleAsync(
        GetAvailablePromotionsQuery query,
        CancellationToken cancellationToken = default)
    {
        var today = query.Today ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var promotions = await promotionRepository.ListByClinicAsync(query.ClinicId, cancellationToken);

        return promotions
            .Where(x => x.IsAvailableToAi(today))
            .OrderBy(x => x.EndsOn)
            .Select(x => new AvailablePromotionDto(
                x.Id,
                x.Name,
                x.RelatedServiceName,
                x.PromoPrice,
                x.StartsOn,
                x.EndsOn,
                x.Conditions))
            .ToList();
    }
}
