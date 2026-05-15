using ClinicMateAI.Domain.Promotions;

namespace ClinicMateAI.Application.Abstractions.Persistence;

public interface IPromotionRepository
{
    Task<IReadOnlyList<Promotion>> ListByClinicAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default);

    Task<Promotion?> GetByIdAsync(
        Guid clinicId,
        Guid promotionId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Promotion promotion,
        CancellationToken cancellationToken = default);

    void Update(Promotion promotion);
}
