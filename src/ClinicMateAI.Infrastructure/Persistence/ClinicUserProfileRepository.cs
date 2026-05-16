using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Infrastructure.Persistence;

public sealed class ClinicUserProfileRepository(AppDbContext dbContext) : IClinicUserProfileRepository
{
    public Task<ClinicUserProfile?> GetByUserAndClinicAsync(string userId, Guid clinicId, CancellationToken cancellationToken = default)
        => dbContext.ClinicUserProfiles.FirstOrDefaultAsync(x => x.UserId == userId && x.ClinicId == clinicId, cancellationToken);

    public Task AddAsync(ClinicUserProfile profile, CancellationToken cancellationToken = default)
        => dbContext.ClinicUserProfiles.AddAsync(profile, cancellationToken).AsTask();

    public void Update(ClinicUserProfile profile)
        => dbContext.ClinicUserProfiles.Update(profile);
}
