namespace ClinicMateAI.Application.Clinics;

public sealed record GetClinicsQuery(
    string? Name,
    DateOnly? CreatedFrom,
    DateOnly? CreatedTo,
    string? Status,
    int Page = 1,
    int PageSize = 10);
