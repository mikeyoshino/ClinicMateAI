using ClinicMateAI.Domain.Clinics;

namespace ClinicMateAI.Application.Abstractions.Persistence;

public interface IClinicUserProfileRepository
{
    Task<ClinicUserProfile?> GetByUserAndClinicAsync(string userId, Guid clinicId, CancellationToken cancellationToken = default);
    Task AddAsync(ClinicUserProfile profile, CancellationToken cancellationToken = default);
    void Update(ClinicUserProfile profile);
}
