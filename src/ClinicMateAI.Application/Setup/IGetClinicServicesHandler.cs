namespace ClinicMateAI.Application.Setup;

public interface IGetClinicServicesHandler
{
    Task<IReadOnlyList<SetupClinicServiceDto>> HandleAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SetupClinicServiceDto>> HandleAsync(Guid clinicId, Guid? branchId, CancellationToken cancellationToken = default);
}
