using ClinicMateAI.Application.Abstractions.Auth;
using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Domain.Clinics;

namespace ClinicMateAI.Logic.Branches;

public sealed class BranchAccessPolicy(
    IClinicUserProfileRepository clinicUserProfileRepository,
    IUserBranchAssignmentRepository userBranchAssignmentRepository,
    IBranchRepository branchRepository) : IBranchAccessPolicy
{
    public async Task<IReadOnlyList<Guid>> GetAccessibleBranchIdsAsync(string userId, Guid clinicId, CancellationToken cancellationToken = default)
    {
        var profile = await clinicUserProfileRepository.GetByUserAndClinicAsync(userId, clinicId, cancellationToken);
        if (profile is null)
        {
            return [];
        }

        if (profile.Role == ClinicUserRole.Owner)
        {
            var allBranches = await branchRepository.ListByClinicAsync(clinicId, cancellationToken);
            return allBranches.Select(x => x.Id).ToList();
        }

        return await userBranchAssignmentRepository.GetBranchIdsForUserAsync(userId, clinicId, cancellationToken);
    }

    public async Task<bool> CanAccessBranchAsync(string userId, Guid clinicId, Guid branchId, CancellationToken cancellationToken = default)
    {
        var branchIds = await GetAccessibleBranchIdsAsync(userId, clinicId, cancellationToken);
        return branchIds.Contains(branchId);
    }
}
