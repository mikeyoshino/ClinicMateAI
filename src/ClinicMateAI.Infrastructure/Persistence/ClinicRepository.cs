using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Infrastructure.Persistence;

public sealed class ClinicRepository(AppDbContext dbContext) : IClinicRepository
{
    public Task<Clinic?> GetByIdAsync(Guid clinicId, CancellationToken cancellationToken = default)
    {
        return dbContext.Clinics
            .FirstOrDefaultAsync(x => x.Id == clinicId, cancellationToken);
    }

    public async Task<IReadOnlyList<Clinic>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Clinics
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Clinic clinic, CancellationToken cancellationToken = default)
    {
        return dbContext.Clinics.AddAsync(clinic, cancellationToken).AsTask();
    }

    public Task UpdateAsync(Clinic clinic, CancellationToken cancellationToken = default)
    {
        dbContext.Clinics.Update(clinic);
        return Task.CompletedTask;
    }

    public async Task<(IReadOnlyList<Clinic> Items, int TotalCount)> SearchAsync(
        string? name,
        DateTime? createdFromUtc,
        DateTime? createdToExclusiveUtc,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Clinics.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            var term = name.Trim();
            query = query.Where(x => EF.Functions.ILike(x.Name, $"%{term}%"));
        }

        if (createdFromUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc >= createdFromUtc.Value);
        }

        if (createdToExclusiveUtc.HasValue)
        {
            query = query.Where(x => x.CreatedAtUtc < createdToExclusiveUtc.Value);
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ClinicStatus>(status, true, out var parsedStatus))
        {
            query = query.Where(x => x.Status == parsedStatus);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
