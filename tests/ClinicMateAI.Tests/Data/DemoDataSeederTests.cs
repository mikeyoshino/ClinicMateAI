using ClinicMateAI.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Tests.Data;

public class DemoDataSeederTests
{
    [Fact]
    public async Task SeedAsync_CreatesBeautyClinicWithPublishedPromotion()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();

        await DemoDataSeeder.SeedAsync(db);

        db.Clinics.Should().ContainSingle(c => c.Name == "Demo Aesthetic Clinic");
        db.Services.Should().Contain(s => s.Name == "Botox Jaw");
        db.Promotions.Should().Contain(p => p.Name == "Botox Jaw New Customer");
    }

    [Fact]
    public async Task SeedAsync_LinksClinicOwnedDemoDataToClinicId()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();

        await DemoDataSeeder.SeedAsync(db);

        var clinic = db.Clinics.Should().ContainSingle().Subject;
        db.Services.Should().OnlyContain(service => service.ClinicId == clinic.Id);
        db.Promotions.Should().OnlyContain(promotion => promotion.ClinicId == clinic.Id);
        db.Conversations.Should().OnlyContain(conversation => conversation.ClinicId == clinic.Id);
        db.Messages.Should().OnlyContain(message => message.ClinicId == clinic.Id);
    }
}
