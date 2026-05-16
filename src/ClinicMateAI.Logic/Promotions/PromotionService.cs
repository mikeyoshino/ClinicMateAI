using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Promotions;
using ClinicMateAI.Domain.Promotions;

namespace ClinicMateAI.Logic.Promotions;

public sealed class PromotionService(
    IPromotionRepository promotionRepository,
    IUnitOfWork unitOfWork) : IPromotionService
{
    public async Task<IReadOnlyList<PromotionManageDto>> ListByClinicAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        var promotions = await promotionRepository.ListByClinicAsync(clinicId, cancellationToken);
        return MapList(promotions);
    }

    public async Task<IReadOnlyList<PromotionManageDto>> ListByClinicAsync(
        Guid clinicId,
        Guid? branchId,
        CancellationToken cancellationToken = default)
    {
        var promotions = await promotionRepository.ListByClinicAsync(clinicId, branchId, cancellationToken);
        return MapList(promotions);
    }

    private static IReadOnlyList<PromotionManageDto> MapList(IReadOnlyList<Promotion> promotions)
    {
        return promotions
            .OrderByDescending(x => x.StartsOn)
            .ThenBy(x => x.Name)
            .Select(Map)
            .ToList();
    }

    public async Task PublishAsync(
        Guid clinicId,
        Guid promotionId,
        CancellationToken cancellationToken = default)
    {
        var promotion = await promotionRepository.GetByIdAsync(clinicId, promotionId, cancellationToken)
            ?? throw new InvalidOperationException("Promotion not found.");

        promotion.Status = PromotionStatus.Published;
        promotionRepository.Update(promotion);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DisableAsync(
        Guid clinicId,
        Guid promotionId,
        CancellationToken cancellationToken = default)
    {
        var promotion = await promotionRepository.GetByIdAsync(clinicId, promotionId, cancellationToken)
            ?? throw new InvalidOperationException("Promotion not found.");

        promotion.Status = PromotionStatus.Disabled;
        promotionRepository.Update(promotion);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static PromotionManageDto Map(Promotion promotion)
    {
        return new PromotionManageDto(
            promotion.Id,
            promotion.Name,
            promotion.RelatedServiceName,
            promotion.PromoPrice,
            promotion.StartsOn,
            promotion.EndsOn,
            promotion.Conditions,
            promotion.Status);
    }
}
