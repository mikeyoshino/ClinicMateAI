namespace ClinicMateAI.Application.Branches;

public sealed record CreateBranchCommand(
    Guid ClinicId,
    string Name,
    string Address,
    string Phone,
    string MapUrl,
    string BusinessHours);
