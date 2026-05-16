namespace ClinicMateAI.Application.Abstractions.Auth;

public interface IBranchAccessPolicy
{
    Task<IReadOnlyList<Guid>> GetAccessibleBranchIdsAsync(string userId, Guid clinicId, CancellationToken cancellationToken = default);
    Task<bool> CanAccessBranchAsync(string userId, Guid clinicId, Guid branchId, CancellationToken cancellationToken = default);
}
