using ClinicMateAI.Domain.Clinics;

namespace ClinicMateAI.Application.Abstractions.Persistence;

public interface IClinicRepository
{
    Task<Clinic?> GetByIdAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Clinic>> ListAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Clinic clinic, CancellationToken cancellationToken = default);
    Task UpdateAsync(Clinic clinic, CancellationToken cancellationToken = default);
    Task<(IReadOnlyList<Clinic> Items, int TotalCount)> SearchAsync(
        string? name,
        DateTime? createdFromUtc,
        DateTime? createdToExclusiveUtc,
        string? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
