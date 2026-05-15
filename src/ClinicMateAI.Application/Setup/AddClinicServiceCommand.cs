namespace ClinicMateAI.Application.Setup;

public sealed record AddClinicServiceCommand(
    Guid ClinicId,
    string Name,
    string Category,
    decimal StartingPrice,
    int DurationMinutes,
    bool RequiresDoctorAssessment,
    string ApprovedAiWording);
