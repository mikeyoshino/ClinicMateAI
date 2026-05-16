namespace ClinicMateAI.Application.Branches;

public interface IGetAccessibleBranchesHandler
{
    Task<IReadOnlyList<AccessibleBranchDto>> HandleAsync(GetAccessibleBranchesQuery query, CancellationToken cancellationToken = default);
}
