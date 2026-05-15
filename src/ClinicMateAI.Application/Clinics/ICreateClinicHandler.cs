namespace ClinicMateAI.Application.Clinics;

public interface ICreateClinicHandler
{
    Task<CreateClinicResultDto> HandleAsync(CreateClinicCommand command, CancellationToken cancellationToken = default);
}
