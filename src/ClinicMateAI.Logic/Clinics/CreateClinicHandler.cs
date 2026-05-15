using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Clinics;
using ClinicMateAI.Domain.Clinics;

namespace ClinicMateAI.Logic.Clinics;

public sealed class CreateClinicHandler(
    IClinicRepository clinicRepository,
    IUnitOfWork unitOfWork) : ICreateClinicHandler
{
    public async Task<CreateClinicResultDto> HandleAsync(CreateClinicCommand command, CancellationToken cancellationToken = default)
    {
        var status = Enum.TryParse<ClinicStatus>(command.Status, true, out var parsed) ? parsed : ClinicStatus.Active;
        var clinic = new Clinic
        {
            Id = Guid.NewGuid(),
            Name = command.Name.Trim(),
            Address = command.Address.Trim(),
            Phone = command.Phone.Trim(),
            MapUrl = string.IsNullOrWhiteSpace(command.MapUrl) ? string.Empty : command.MapUrl.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            Status = status
        };

        await clinicRepository.AddAsync(clinic, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreateClinicResultDto(clinic.Id);
    }
}
