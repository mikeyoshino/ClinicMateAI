using ClinicMateAI.Domain.Messaging;
using ClinicMateAI.Domain.Promotions;
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
            BranchId = Guid.NewGuid(),
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
            new Conversation { ClinicId = clinicA, BranchId = Guid.NewGuid(), Channel = "LINE", ExternalConversationId = "a-1", CustomerDisplayName = "A1", LastMessageAtUtc = DateTime.UtcNow.AddMinutes(-1) },
            new Conversation { ClinicId = clinicA, BranchId = Guid.NewGuid(), Channel = "LINE", ExternalConversationId = "a-2", CustomerDisplayName = "A2", LastMessageAtUtc = DateTime.UtcNow.AddMinutes(-2) },
            new Conversation { ClinicId = clinicB, BranchId = Guid.NewGuid(), Channel = "LINE", ExternalConversationId = "b-1", CustomerDisplayName = "B1", LastMessageAtUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var repository = new ConversationRepository(db);
        var result = await repository.ListRecentAsync(clinicA, take: 10);

        result.Should().HaveCount(2);
        result.Should().OnlyContain(x => x.ClinicId == clinicA);
    }

    [Fact]
    public async Task ListRecentAsync_WithBranchFilter_ReturnsOnlyRequestedBranch()
    {
        await using var db = CreateDb();
        var clinicId = Guid.NewGuid();
        var branchA = Guid.NewGuid();
        var branchB = Guid.NewGuid();
        db.Conversations.AddRange(
            new Conversation { ClinicId = clinicId, BranchId = branchA, Channel = "LINE", ExternalConversationId = "a-1", CustomerDisplayName = "A1", LastMessageAtUtc = DateTime.UtcNow.AddMinutes(-1) },
            new Conversation { ClinicId = clinicId, BranchId = branchB, Channel = "LINE", ExternalConversationId = "b-1", CustomerDisplayName = "B1", LastMessageAtUtc = DateTime.UtcNow });
        await db.SaveChangesAsync();

        var repository = new ConversationRepository(db);
        var result = await repository.ListRecentAsync(clinicId, branchA, take: 10);

        result.Should().ContainSingle();
        result[0].BranchId.Should().Be(branchA);
    }

    [Fact]
    public async Task ListByClinicAsync_ReturnsMatchingBranchAndAllBranchPromotions()
    {
        await using var db = CreateDb();
        var clinicId = Guid.NewGuid();
        var branchId = Guid.NewGuid();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        db.Promotions.AddRange(
            new Promotion { ClinicId = clinicId, BranchId = branchId, Name = "Branch only", StartsOn = today, EndsOn = today, Conditions = "x", ApprovedAiWording = "x" },
            new Promotion { ClinicId = clinicId, BranchId = null, Name = "All branch", StartsOn = today, EndsOn = today, Conditions = "x", ApprovedAiWording = "x" },
            new Promotion { ClinicId = clinicId, BranchId = Guid.NewGuid(), Name = "Other branch", StartsOn = today, EndsOn = today, Conditions = "x", ApprovedAiWording = "x" });
        await db.SaveChangesAsync();

        var repository = new PromotionRepository(db);
        var result = await repository.ListByClinicAsync(clinicId, branchId);

        result.Select(x => x.Name).Should().BeEquivalentTo(["Branch only", "All branch"]);
    }

    private static AppDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
