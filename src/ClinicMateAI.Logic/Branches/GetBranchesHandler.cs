using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Branches;

namespace ClinicMateAI.Logic.Branches;

public sealed class GetBranchesHandler(IBranchRepository branchRepository) : IGetBranchesHandler
{
    public async Task<IReadOnlyList<BranchListItemDto>> HandleAsync(GetBranchesQuery query, CancellationToken cancellationToken = default)
    {
        var branches = await branchRepository.ListByClinicAsync(query.ClinicId, cancellationToken);
        return branches
            .Select(x => new BranchListItemDto(
                x.Id,
                x.Name,
                x.Address,
                x.Phone,
                x.IsDefault,
                x.Status))
            .ToList();
    }
}
