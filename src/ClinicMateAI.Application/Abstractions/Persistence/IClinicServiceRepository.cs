using ClinicMateAI.Domain.Services;

namespace ClinicMateAI.Application.Abstractions.Persistence;

public interface IClinicServiceRepository
{
    Task<IReadOnlyList<ClinicService>> ListByClinicAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ClinicService>> ListByClinicAsync(Guid clinicId, Guid? branchId, CancellationToken cancellationToken = default);
    Task AddAsync(ClinicService service, CancellationToken cancellationToken = default);
    Task<ClinicService?> GetByIdAsync(Guid serviceId, CancellationToken cancellationToken = default);
    Task DeleteAsync(ClinicService service, CancellationToken cancellationToken = default);
}
