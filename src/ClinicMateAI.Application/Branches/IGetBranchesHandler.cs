namespace ClinicMateAI.Application.Branches;

public interface IGetBranchesHandler
{
    Task<IReadOnlyList<BranchListItemDto>> HandleAsync(GetBranchesQuery query, CancellationToken cancellationToken = default);
}
