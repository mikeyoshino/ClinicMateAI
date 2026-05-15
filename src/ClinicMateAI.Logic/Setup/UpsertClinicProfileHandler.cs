using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Setup;
using ClinicMateAI.Domain.Clinics;

namespace ClinicMateAI.Logic.Setup;

public sealed class UpsertClinicProfileHandler(
    IClinicRepository clinicRepository,
    IUnitOfWork unitOfWork) : IUpsertClinicProfileHandler
{
    public async Task HandleAsync(UpsertClinicProfileCommand command, CancellationToken cancellationToken = default)
    {
        var clinic = await clinicRepository.GetByIdAsync(command.ClinicId, cancellationToken);
        if (clinic is null)
        {
            clinic = new Clinic
            {
                Id = command.ClinicId
            };
            await clinicRepository.AddAsync(clinic, cancellationToken);
        }

        clinic.Name = command.Name.Trim();
        clinic.Address = command.Address.Trim();
        clinic.Phone = command.Phone.Trim();
        clinic.MapUrl = command.MapUrl.Trim();

        await clinicRepository.UpdateAsync(clinic, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
