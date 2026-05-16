using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Infrastructure.Data;
using ClinicMateAI.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Tests.Persistence;

public class BranchRepositoryTests
{
    [Fact]
    public async Task GetDefaultAsync_ReturnsDefaultBranch_ForClinic()
    {
        await using var db = CreateDb();
        var clinicId = Guid.NewGuid();
        var defaultBranch = new Branch
        {
            ClinicId = clinicId,
            Name = "Main",
            Address = "A",
            Phone = "1",
            MapUrl = "map",
            BusinessHours = "Mon",
            IsDefault = true
        };

        db.Branches.AddRange(
            defaultBranch,
            new Branch
            {
                ClinicId = clinicId,
                Name = "Branch B",
                Address = "B",
                Phone = "2",
                MapUrl = "map-b",
                BusinessHours = "Tue"
            });
        await db.SaveChangesAsync();

        var repository = new BranchRepository(db);
        var result = await repository.GetDefaultAsync(clinicId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(defaultBranch.Id);
    }

    [Fact]
    public async Task GetBranchIdsForUserAsync_ReturnsOnlyAssignmentsForClinic()
    {
        await using var db = CreateDb();
        var clinicA = Guid.NewGuid();
        var clinicB = Guid.NewGuid();
        var branchA = Guid.NewGuid();
        var branchB = Guid.NewGuid();

        db.UserBranchAssignments.AddRange(
            new UserBranchAssignment { UserId = "user-1", ClinicId = clinicA, BranchId = branchA },
            new UserBranchAssignment { UserId = "user-1", ClinicId = clinicB, BranchId = branchB });
        await db.SaveChangesAsync();

        var repository = new UserBranchAssignmentRepository(db);
        var result = await repository.GetBranchIdsForUserAsync("user-1", clinicA);

        result.Should().Equal(branchA);
    }

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
