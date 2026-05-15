using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Setup;
using ClinicMateAI.Domain.Services;

namespace ClinicMateAI.Logic.Setup;

public sealed class AddClinicServiceHandler(
    IClinicServiceRepository clinicServiceRepository,
    IUnitOfWork unitOfWork) : IAddClinicServiceHandler
{
    public async Task HandleAsync(AddClinicServiceCommand command, CancellationToken cancellationToken = default)
    {
        var service = new ClinicService
        {
            ClinicId = command.ClinicId,
            Name = command.Name.Trim(),
            Category = command.Category.Trim(),
            StartingPrice = command.StartingPrice,
            DurationMinutes = command.DurationMinutes,
            RequiresDoctorAssessment = command.RequiresDoctorAssessment,
            ApprovedAiWording = command.ApprovedAiWording.Trim()
        };

        await clinicServiceRepository.AddAsync(service, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
