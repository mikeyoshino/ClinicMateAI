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
}
