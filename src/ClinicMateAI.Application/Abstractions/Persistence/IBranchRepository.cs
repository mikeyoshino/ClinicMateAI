using ClinicMateAI.Domain.Clinics;

namespace ClinicMateAI.Application.Abstractions.Persistence;

public interface IBranchRepository
{
    Task<Branch?> GetByIdAsync(Guid clinicId, Guid branchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Branch>> ListByClinicAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<Branch?> GetDefaultAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task AddAsync(Branch branch, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(Guid clinicId, string name, CancellationToken cancellationToken = default);
}
