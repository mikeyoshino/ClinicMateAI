using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Domain.Services;
using ClinicMateAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Infrastructure.Persistence;

public sealed class ClinicServiceRepository(AppDbContext dbContext) : IClinicServiceRepository
{
    public async Task<IReadOnlyList<ClinicService>> ListByClinicAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Services
            .Where(x => x.ClinicId == clinicId)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClinicService>> ListByClinicAsync(Guid clinicId, Guid? branchId, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Services.Where(x => x.ClinicId == clinicId);

        if (branchId is not null)
        {
            query = query.Where(x => x.BranchId == null || x.BranchId == branchId.Value);
        }

        return await query
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(ClinicService service, CancellationToken cancellationToken = default)
    {
        return dbContext.Services.AddAsync(service, cancellationToken).AsTask();
    }

    public async Task<ClinicService?> GetByIdAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Services.FindAsync([serviceId], cancellationToken);
    }

    public Task DeleteAsync(ClinicService service, CancellationToken cancellationToken = default)
    {
        dbContext.Services.Remove(service);
        return Task.CompletedTask;
    }
}
