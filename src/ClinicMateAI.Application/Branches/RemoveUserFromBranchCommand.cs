namespace ClinicMateAI.Application.Branches;

public sealed record RemoveUserFromBranchCommand(Guid ClinicId, string UserId, Guid BranchId);
