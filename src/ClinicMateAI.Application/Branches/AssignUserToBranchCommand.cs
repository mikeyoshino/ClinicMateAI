namespace ClinicMateAI.Application.Branches;

public sealed record AssignUserToBranchCommand(Guid ClinicId, string UserId, Guid BranchId);
