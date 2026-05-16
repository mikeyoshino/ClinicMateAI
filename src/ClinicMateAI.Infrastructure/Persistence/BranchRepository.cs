using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Infrastructure.Persistence;

public sealed class BranchRepository(AppDbContext dbContext) : IBranchRepository
{
    public Task<Branch?> GetByIdAsync(Guid clinicId, Guid branchId, CancellationToken cancellationToken = default)
        => dbContext.Branches.FirstOrDefaultAsync(x => x.ClinicId == clinicId && x.Id == branchId, cancellationToken);

    public async Task<IReadOnlyList<Branch>> ListByClinicAsync(Guid clinicId, CancellationToken cancellationToken = default)
        => await dbContext.Branches
            .Where(x => x.ClinicId == clinicId)
            .OrderByDescending(x => x.IsDefault)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

    public Task<Branch?> GetDefaultAsync(Guid clinicId, CancellationToken cancellationToken = default)
        => dbContext.Branches.FirstOrDefaultAsync(x => x.ClinicId == clinicId && x.IsDefault, cancellationToken);

    public Task AddAsync(Branch branch, CancellationToken cancellationToken = default)
        => dbContext.Branches.AddAsync(branch, cancellationToken).AsTask();

    public Task<bool> ExistsByNameAsync(Guid clinicId, string name, CancellationToken cancellationToken = default)
        => dbContext.Branches.AnyAsync(x => x.ClinicId == clinicId && x.Name == name, cancellationToken);
}
