namespace ClinicMateAI.Application.Setup;

public sealed record UpsertClinicProfileCommand(
    Guid ClinicId,
    string Name,
    string Address,
    string Phone,
    string MapUrl);
