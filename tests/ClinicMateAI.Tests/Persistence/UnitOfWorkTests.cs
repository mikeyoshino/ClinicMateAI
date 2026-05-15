using ClinicMateAI.Domain.Messaging;
using ClinicMateAI.Infrastructure.Data;
using ClinicMateAI.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Tests.Persistence;

public class UnitOfWorkTests
{
    [Fact]
    public async Task SaveChangesAsync_CommitsPendingRepositoryChanges()
    {
        var dbName = Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        await using (var db = new AppDbContext(options))
        {
            var repository = new ConversationRepository(db);
            var unitOfWork = new UnitOfWork(db);

            var conversation = new Conversation
            {
                ClinicId = Guid.NewGuid(),
                Channel = "LINE",
                ExternalConversationId = "line-123",
                CustomerDisplayName = "Customer"
            };
            await repository.AddAsync(conversation);

            db.Conversations.Count().Should().Be(0);

            await unitOfWork.SaveChangesAsync();
        }

        await using (var verifyDb = new AppDbContext(options))
        {
            verifyDb.Conversations.Should().ContainSingle();
        }
    }
}
