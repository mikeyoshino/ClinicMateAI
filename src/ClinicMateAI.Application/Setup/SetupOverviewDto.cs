namespace ClinicMateAI.Application.Setup;

public sealed record SetupOverviewDto(
    Guid ClinicId,
    string ClinicName,
    string Address,
    string Phone,
    string MapUrl,
    int CompletedSteps,
    int TotalSteps,
    IReadOnlyList<SetupStepStatusDto> Steps);
