namespace ClinicMateAI.Application.Setup;

public sealed record SetupClinicServiceDto(
    Guid ServiceId,
    string Name,
    string Category,
    decimal StartingPrice,
    int DurationMinutes,
    bool RequiresDoctorAssessment);
