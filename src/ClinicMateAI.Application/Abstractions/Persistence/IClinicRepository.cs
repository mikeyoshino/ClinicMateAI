using ClinicMateAI.Domain.Clinics;

namespace ClinicMateAI.Application.Abstractions.Persistence;

public interface IClinicRepository
{
    Task<Clinic?> GetByIdAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Clinic>> ListAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Clinic clinic, CancellationToken cancellationToken = default);
}
