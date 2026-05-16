namespace ClinicMateAI.Application.Setup;

public sealed record AddClinicServiceCommand(
    Guid ClinicId,
    Guid? BranchId,
    string Name,
    string Category,
    decimal StartingPrice,
    int DurationMinutes,
    bool RequiresDoctorAssessment,
    string ApprovedAiWording);
