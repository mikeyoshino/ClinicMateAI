namespace ClinicMateAI.Application.Setup;

public sealed record DeleteClinicServiceCommand(Guid ClinicId, Guid ServiceId);
