using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Domain.Promotions;
using ClinicMateAI.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Tests.Data;

public class DemoDataSeederTests
{
    [Fact]
    public async Task SeedAsync_CreatesDefaultBranch_AndAttachesSeedDataToIt()
    {
        await using var db = CreateDb();

        await DemoDataSeeder.SeedAsync(db);

        var clinic = await db.Clinics.SingleAsync();
        var branch = await db.Branches.SingleAsync();
        var service = await db.Services.SingleAsync();
        var promotion = await db.Promotions.SingleAsync(x => x.Status == PromotionStatus.Published);
        var conversation = await db.Conversations.SingleAsync();

        branch.ClinicId.Should().Be(clinic.Id);
        branch.IsDefault.Should().BeTrue();
        service.BranchId.Should().Be(branch.Id);
        promotion.BranchId.Should().Be(branch.Id);
        conversation.BranchId.Should().Be(branch.Id);
    }

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
