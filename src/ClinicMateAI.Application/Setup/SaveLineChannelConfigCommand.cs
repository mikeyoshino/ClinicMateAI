namespace ClinicMateAI.Application.Setup;

public sealed record SaveLineChannelConfigCommand(Guid ClinicId, Guid BranchId, string ChannelSecret, string AccessToken);
