namespace ClinicMateAI.Application.Clinics;

public sealed record ClinicListResultDto(
    IReadOnlyList<ClinicListItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize);
