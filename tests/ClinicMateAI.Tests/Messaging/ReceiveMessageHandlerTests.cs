using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Ai;
using ClinicMateAI.Application.Messaging;
using ClinicMateAI.Domain.Messaging;
using ClinicMateAI.Logic.Ai;
using ClinicMateAI.Logic.Messaging;
using FluentAssertions;
using FluentValidation;

namespace ClinicMateAI.Tests.Messaging;

public class ReceiveMessageHandlerTests
{
    [Fact]
    public async Task HandleAsync_ThrowsValidationException_WhenCommandIsInvalid()
    {
        var conversationRepository = new InMemoryConversationRepository();
        var messageRepository = new InMemoryMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new ReceiveMessageHandler(
            new ReceiveMessageCommandValidator(),
            conversationRepository,
            messageRepository,
            new SimulatedAiReplyProvider(),
            unitOfWork);

        var command = new ReceiveMessageCommand(
            ClinicId: Guid.Empty,
            Channel: "LINE",
            ExternalConversationId: "line-1",
            CustomerDisplayName: "A",
            Text: "hello",
            ReceivedAt: DateTimeOffset.UtcNow);

        await Assert.ThrowsAsync<ValidationException>(() => handler.HandleAsync(command));
        unitOfWork.SaveChangesCallCount.Should().Be(0);
        conversationRepository.Items.Should().BeEmpty();
        messageRepository.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_PersistsMessage_AndCommits_ForValidCommand()
    {
        var clinicId = Guid.NewGuid();
        var conversationRepository = new InMemoryConversationRepository();
        var messageRepository = new InMemoryMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new ReceiveMessageHandler(
            new ReceiveMessageCommandValidator(),
            conversationRepository,
            messageRepository,
            new SimulatedAiReplyProvider(),
            unitOfWork);

        var result = await handler.HandleAsync(new ReceiveMessageCommand(
            ClinicId: clinicId,
            Channel: "LINE",
            ExternalConversationId: "line-9",
            CustomerDisplayName: "Customer A",
            Text: "โบท็อกกรามเท่าไรคะ",
            ReceivedAt: DateTimeOffset.UtcNow));

        unitOfWork.SaveChangesCallCount.Should().Be(1);
        conversationRepository.Items.Should().ContainSingle();
        messageRepository.Items.Should().ContainSingle();
        messageRepository.Items[0].ClinicId.Should().Be(clinicId);
        result.ReplyText.Should().Contain("คุณลูกค้า");
        result.RequiresHandoff.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_UpdatesExistingConversation_WhenExternalConversationExists()
    {
        var clinicId = Guid.NewGuid();
        var conversation = new Conversation
        {
            ClinicId = clinicId,
            Channel = "LINE",
            ExternalConversationId = "line-9",
            CustomerDisplayName = "Old Name",
            LastMessageAtUtc = DateTime.UtcNow.AddDays(-1)
        };
        var conversationRepository = new InMemoryConversationRepository([conversation]);
        var messageRepository = new InMemoryMessageRepository();
        var unitOfWork = new FakeUnitOfWork();
        var handler = new ReceiveMessageHandler(
            new ReceiveMessageCommandValidator(),
            conversationRepository,
            messageRepository,
            new SimulatedAiReplyProvider(),
            unitOfWork);

        var now = DateTimeOffset.UtcNow;
        var result = await handler.HandleAsync(new ReceiveMessageCommand(
            ClinicId: clinicId,
            Channel: "LINE",
            ExternalConversationId: "line-9",
            CustomerDisplayName: "New Name",
            Text: "ฉีดแล้วเป็นก้อนค่ะ",
            ReceivedAt: now));

        conversationRepository.Items.Should().ContainSingle();
        conversationRepository.Items[0].CustomerDisplayName.Should().Be("New Name");
        conversationRepository.Items[0].LastMessageAtUtc.Should().Be(now.UtcDateTime);
        result.RequiresHandoff.Should().BeTrue();
        unitOfWork.SaveChangesCallCount.Should().Be(1);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class InMemoryConversationRepository : IConversationRepository
    {
        public List<Conversation> Items { get; } = [];

        public InMemoryConversationRepository()
        {
        }

        public InMemoryConversationRepository(IEnumerable<Conversation> seed)
        {
            Items.AddRange(seed);
        }

        public Task<Conversation?> GetByIdAsync(Guid clinicId, Guid conversationId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Items.FirstOrDefault(x => x.ClinicId == clinicId && x.Id == conversationId));
        }

        public Task<Conversation?> GetByExternalIdAsync(
            Guid clinicId,
            string channel,
            string externalConversationId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Items.FirstOrDefault(x =>
                x.ClinicId == clinicId && x.Channel == channel && x.ExternalConversationId == externalConversationId));
        }

        public Task<IReadOnlyList<Conversation>> ListRecentAsync(Guid clinicId, int take, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Conversation> result = Items
                .Where(x => x.ClinicId == clinicId)
                .OrderByDescending(x => x.LastMessageAtUtc)
                .Take(take)
                .ToList();
            return Task.FromResult(result);
        }

        public Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
        {
            Items.Add(conversation);
            return Task.CompletedTask;
        }

        public void Update(Conversation conversation)
        {
            var index = Items.FindIndex(x => x.Id == conversation.Id);
            if (index >= 0)
            {
                Items[index] = conversation;
            }
        }
    }

    private sealed class InMemoryMessageRepository : IMessageRepository
    {
        public List<Message> Items { get; } = [];

        public Task AddAsync(Message message, CancellationToken cancellationToken = default)
        {
            Items.Add(message);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Message>> ListByConversationAsync(
            Guid clinicId,
            Guid conversationId,
            CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Message> result = Items
                .Where(x => x.ClinicId == clinicId && x.ConversationId == conversationId)
                .OrderBy(x => x.SentAtUtc)
                .ToList();
            return Task.FromResult(result);
        }
    }
}
