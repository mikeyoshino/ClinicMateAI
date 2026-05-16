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

    public async Task<IReadOnlyList<Promotion>> ListByClinicAsync(
        Guid clinicId,
        Guid? branchId,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Promotions.Where(x => x.ClinicId == clinicId);

        if (branchId is not null)
        {
            query = query.Where(x => x.BranchId == null || x.BranchId == branchId.Value);
        }

        return await query
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

    public Task<Promotion?> GetByIdAsync(
        Guid clinicId,
        Guid promotionId,
        Guid? branchId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.Promotions
            .FirstOrDefaultAsync(
                x => x.ClinicId == clinicId
                    && x.Id == promotionId
                    && (branchId == null || x.BranchId == null || x.BranchId == branchId.Value),
                cancellationToken);
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
