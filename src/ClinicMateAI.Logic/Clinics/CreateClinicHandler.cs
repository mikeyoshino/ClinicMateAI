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
        if (string.IsNullOrWhiteSpace(command.Status) ||
            !Enum.TryParse<ClinicStatus>(command.Status.Trim(), true, out var status))
        {
            throw new ArgumentException("Status must be a valid clinic status.", nameof(command.Status));
        }

        var clinic = new Clinic
        {
            Name = command.Name.Trim(),
            Address = command.Address.Trim(),
            Phone = command.Phone.Trim(),
            MapUrl = string.IsNullOrWhiteSpace(command.MapUrl) ? string.Empty : command.MapUrl.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            Status = status
        };
        clinic.SetPackageContract(command.PackageTier, command.AdditionalBranchMonthlyPrice);

        await clinicRepository.AddAsync(clinic, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new CreateClinicResultDto(clinic.Id);
    }
}
