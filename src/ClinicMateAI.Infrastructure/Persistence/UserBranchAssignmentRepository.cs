using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Infrastructure.Persistence;

public sealed class UserBranchAssignmentRepository(AppDbContext dbContext) : IUserBranchAssignmentRepository
{
    public async Task<IReadOnlyList<Guid>> GetBranchIdsForUserAsync(string userId, Guid clinicId, CancellationToken cancellationToken = default)
        => await dbContext.UserBranchAssignments
            .Where(x => x.UserId == userId && x.ClinicId == clinicId)
            .Select(x => x.BranchId)
            .ToListAsync(cancellationToken);

    public Task AddAsync(UserBranchAssignment assignment, CancellationToken cancellationToken = default)
        => dbContext.UserBranchAssignments.AddAsync(assignment, cancellationToken).AsTask();

    public Task<bool> IsAssignedAsync(string userId, Guid branchId, CancellationToken cancellationToken = default)
        => dbContext.UserBranchAssignments.AnyAsync(x => x.UserId == userId && x.BranchId == branchId, cancellationToken);

    public async Task RemoveAsync(string userId, Guid branchId, CancellationToken cancellationToken = default)
    {
        var assignments = await dbContext.UserBranchAssignments
            .Where(x => x.UserId == userId && x.BranchId == branchId)
            .ToListAsync(cancellationToken);

        if (assignments.Count > 0)
        {
            dbContext.UserBranchAssignments.RemoveRange(assignments);
        }
    }
}
