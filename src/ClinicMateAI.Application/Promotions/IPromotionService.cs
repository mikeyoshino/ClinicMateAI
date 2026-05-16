namespace ClinicMateAI.Application.Promotions;

public interface IPromotionService
{
    Task<IReadOnlyList<PromotionManageDto>> ListByClinicAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PromotionManageDto>> ListByClinicAsync(
        Guid clinicId,
        Guid? branchId,
        CancellationToken cancellationToken = default);

    Task PublishAsync(
        Guid clinicId,
        Guid promotionId,
        CancellationToken cancellationToken = default);

    Task DisableAsync(
        Guid clinicId,
        Guid promotionId,
        CancellationToken cancellationToken = default);
}
