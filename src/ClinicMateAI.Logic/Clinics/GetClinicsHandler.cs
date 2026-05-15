using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Clinics;

namespace ClinicMateAI.Logic.Clinics;

public sealed class GetClinicsHandler(IClinicRepository clinicRepository) : IGetClinicsHandler
{
    public async Task<ClinicListResultDto> HandleAsync(GetClinicsQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 10 : query.PageSize;
        var createdFromUtc = query.CreatedFrom?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var createdToExclusiveUtc = query.CreatedTo?.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var (items, totalCount) = await clinicRepository.SearchAsync(
            query.Name,
            createdFromUtc,
            createdToExclusiveUtc,
            query.Status,
            page,
            pageSize,
            cancellationToken);

        return new ClinicListResultDto(
            items.Select(x => new ClinicListItemDto(
                x.Id,
                x.Name,
                x.Address,
                x.Phone,
                x.Status.ToString(),
                x.CreatedAtUtc)).ToList(),
            totalCount,
            page,
            pageSize);
    }
}
