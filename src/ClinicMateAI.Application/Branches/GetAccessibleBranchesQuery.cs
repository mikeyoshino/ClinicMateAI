namespace ClinicMateAI.Application.Branches;

public sealed record GetAccessibleBranchesQuery(Guid ClinicId, string UserId);
