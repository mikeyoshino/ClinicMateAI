using ClinicMateAI.Application.Abstractions.Auth;
using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Branches;

namespace ClinicMateAI.Logic.Branches;

public sealed class GetAccessibleBranchesHandler(
    IBranchAccessPolicy branchAccessPolicy,
    IBranchRepository branchRepository) : IGetAccessibleBranchesHandler
{
    public async Task<IReadOnlyList<AccessibleBranchDto>> HandleAsync(GetAccessibleBranchesQuery query, CancellationToken cancellationToken = default)
    {
        var accessibleBranchIds = await branchAccessPolicy.GetAccessibleBranchIdsAsync(query.UserId, query.ClinicId, cancellationToken);
        var branches = await branchRepository.ListByClinicAsync(query.ClinicId, cancellationToken);

        return branches
            .Where(x => accessibleBranchIds.Contains(x.Id))
            .Select(x => new AccessibleBranchDto(x.Id, x.Name, x.IsDefault))
            .ToList();
    }
}
