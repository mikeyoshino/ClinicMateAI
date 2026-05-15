using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Setup;

namespace ClinicMateAI.Logic.Setup;

public sealed class GetClinicServicesHandler(
    IClinicServiceRepository clinicServiceRepository) : IGetClinicServicesHandler
{
    public async Task<IReadOnlyList<SetupClinicServiceDto>> HandleAsync(
        Guid clinicId,
        CancellationToken cancellationToken = default)
    {
        var services = await clinicServiceRepository.ListByClinicAsync(clinicId, cancellationToken);
        return services
            .Select(x => new SetupClinicServiceDto(
                x.Id,
                x.Name,
                x.Category,
                x.StartingPrice,
                x.DurationMinutes,
                x.RequiresDoctorAssessment))
            .ToList();
    }
}
