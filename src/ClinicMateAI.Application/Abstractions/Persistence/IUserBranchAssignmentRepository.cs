using ClinicMateAI.Domain.Clinics;

namespace ClinicMateAI.Application.Abstractions.Persistence;

public interface IUserBranchAssignmentRepository
{
    Task<IReadOnlyList<Guid>> GetBranchIdsForUserAsync(string userId, Guid clinicId, CancellationToken cancellationToken = default);
    Task AddAsync(UserBranchAssignment assignment, CancellationToken cancellationToken = default);
    Task<bool> IsAssignedAsync(string userId, Guid branchId, CancellationToken cancellationToken = default);
    Task RemoveAsync(string userId, Guid branchId, CancellationToken cancellationToken = default);
}
