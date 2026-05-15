namespace ClinicMateAI.Application.Clinics;

public sealed record ClinicListItemDto(
    Guid ClinicId,
    string Name,
    string Address,
    string Phone,
    string Status,
    DateTime CreatedAtUtc);
