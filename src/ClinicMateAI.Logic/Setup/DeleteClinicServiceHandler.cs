using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Setup;

namespace ClinicMateAI.Logic.Setup;

public sealed class DeleteClinicServiceHandler(
    IClinicServiceRepository serviceRepository,
    IUnitOfWork unitOfWork) : IDeleteClinicServiceHandler
{
    public async Task<bool> HandleAsync(DeleteClinicServiceCommand command, CancellationToken cancellationToken = default)
    {
        var service = await serviceRepository.GetByIdAsync(command.ServiceId, cancellationToken);
        if (service is null || service.ClinicId != command.ClinicId)
        {
            return false;
        }

        await serviceRepository.DeleteAsync(service, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }
}
