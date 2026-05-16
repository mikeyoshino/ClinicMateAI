namespace ClinicMateAI.Application.Setup;

public sealed record CompleteFacebookConnectionCommand(Guid ClinicId, string AuthorizationCode);
