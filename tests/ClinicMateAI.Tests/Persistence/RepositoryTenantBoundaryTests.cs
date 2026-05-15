using ClinicMateAI.Domain.Messaging;
using ClinicMateAI.Infrastructure.Data;
using ClinicMateAI.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Tests.Persistence;

public class RepositoryTenantBoundaryTests
{
    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenClinicDoesNotMatch()
    {
        await using var db = CreateDb();
        var otherClinicId = Guid.NewGuid();
        var conversation = new Conversation
        {
            ClinicId = Guid.NewGuid(),
            Channel = "LINE",
            ExternalConversationId = "line-1",
            CustomerDisplayName = "A"
        };
        db.Conversations.Add(conversation);
        await db.SaveChangesAsync();

        var repository = new ConversationRepository(db);

        var result = await repository.GetByIdAsync(otherClinicId, conversation.Id);

        result.Should().BeNull();
    }

    [Fact]
    public async Task ListByConversationAsync_ReturnsOnlyMessagesOfRequestedClinic()
    {
        await using var db = CreateDb();
        var clinicA = Guid.NewGuid();
        var clinicB = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        db.Messages.AddRange(
            new Message { ClinicId = clinicA, ConversationId = conversationId, SenderType = "Customer", Text = "A1" },
            new Message { ClinicId = clinicA, ConversationId = conversationId, SenderType = "Customer", Text = "A2" },
            new Message { ClinicId = clinicB, ConversationId = conversationId, SenderType = "Customer", Text = "B1" });
        await db.SaveChangesAsync();

        var repository = new MessageRepository(db);
        var result = await repository.ListByConversationAsync(clinicA, conversationId);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(x => x.ClinicId == clinicA);
    }

    [Fact]
    public async Task ListRecentAsync_ExcludesOtherClinics()
    {
        await using var db = CreateDb();
        var clinicA = Guid.NewGuid();
        var clinicB = Guid.NewGuid();
        db.Conversations.AddRange(
            new Conversation { ClinicId = clinicA, Channel = "LINE", ExternalConversationId = "a-1", CustomerDisplayName = "A1", LastMessageAtUtc = DateTime.UtcNow.AddMinutes(-1) },
            new Conversation { ClinicId = clinicA, Channel = "LINE", ExternalConversationId = "a-2", CustomerDisplayName = "A2", LastMessageAtUtc = DateTime.UtcNow.AddMinutes(-2) },
            new Conversation { ClinicId = clinicB, Channel = "LINE", ExternalConversationId = "b-1", CustomerDisplayName = "B1", LastMessageAtUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var repository = new ConversationRepository(db);
        var result = await repository.ListRecentAsync(clinicA, take: 10);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(x => x.ClinicId == clinicA);
    }

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
