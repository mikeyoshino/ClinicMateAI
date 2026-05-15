namespace ClinicMateAI.Application.Setup;

public sealed record SetupStepStatusDto(
    string Key,
    string Title,
    string Status,
    string Detail);
