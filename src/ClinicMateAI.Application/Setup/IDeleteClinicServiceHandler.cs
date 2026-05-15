namespace ClinicMateAI.Application.Setup;

public interface IDeleteClinicServiceHandler
{
    /// <summary>
    /// Deletes the service identified by <paramref name="command"/>.ServiceId,
    /// enforcing that it belongs to the given ClinicId.
    /// Returns false if the service was not found or does not belong to the clinic.
    /// </summary>
    Task<bool> HandleAsync(DeleteClinicServiceCommand command, CancellationToken cancellationToken = default);
}
