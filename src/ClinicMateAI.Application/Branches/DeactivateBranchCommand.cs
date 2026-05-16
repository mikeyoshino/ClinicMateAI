namespace ClinicMateAI.Application.Branches;

public sealed record DeactivateBranchCommand(Guid ClinicId, Guid BranchId);
