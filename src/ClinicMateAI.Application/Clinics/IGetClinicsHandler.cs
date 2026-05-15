namespace ClinicMateAI.Application.Clinics;

public interface IGetClinicsHandler
{
    Task<ClinicListResultDto> HandleAsync(GetClinicsQuery query, CancellationToken cancellationToken = default);
}
