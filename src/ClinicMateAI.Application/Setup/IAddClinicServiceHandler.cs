namespace ClinicMateAI.Application.Setup;

public interface IAddClinicServiceHandler
{
    Task HandleAsync(AddClinicServiceCommand command, CancellationToken cancellationToken = default);
}
