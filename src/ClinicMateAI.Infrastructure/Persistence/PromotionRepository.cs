using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Domain.Promotions;
using ClinicMateAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Infrastructure.Persistence;

public sealed class PromotionRepository(AppDbContext dbContext) : IPromotionRepository
{
    public async Task<IReadOnlyList<Promotion>> ListByClinicAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Promotions
            .Where(x => x.ClinicId == clinicId)
            .OrderBy(x => x.StartsOn)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Promotion?> GetByIdAsync(
        Guid clinicId,
        Guid promotionId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Promotions
            .FirstOrDefaultAsync(x => x.ClinicId == clinicId && x.Id == promotionId, cancellationToken);
    }

    public Task AddAsync(
        Promotion promotion,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Promotions.AddAsync(promotion, cancellationToken).AsTask();
    }

    public void Update(Promotion promotion)
    {
        dbContext.Promotions.Update(promotion);
    }
}
