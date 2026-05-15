namespace ClinicMateAI.Application.Setup;

public interface IUpsertClinicProfileHandler
{
    Task HandleAsync(UpsertClinicProfileCommand command, CancellationToken cancellationToken = default);
}
