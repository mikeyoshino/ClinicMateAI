using ClinicMateAI.Application.Abstractions.Messaging;
using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Inbox;
using ClinicMateAI.Domain.Errors;
using ClinicMateAI.Domain.Messaging;
using ClinicMateAI.Logic.Inbox;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicMateAI.Tests.Inbox;

public class InboxCommandHandlersTests
{
    // ── ClaimConversationHandler ──────────────────────────────────────────

    [Fact]
    public async Task ClaimConversation_ReturnsSuccess_WhenConversationIsUnclaimed()
    {
        var clinicId = Guid.NewGuid();
        var conv = MakeConversation(clinicId);
        var convRepo = new FakeConversationRepository([conv]);
        var notifier = new SpyInboxNotifier();
        var uow = new FakeUnitOfWork();

        var handler = new ClaimConversationHandler(convRepo, notifier, uow,
            NullLogger<ClaimConversationHandler>.Instance);
        var result = await handler.HandleAsync(new ClaimConversationCommand(conv.Id, clinicId, "Alice"));
        conv.Status.Should().Be("InProgress");
        conv.ClaimedAt.Should().NotBeNull();
        uow.SaveChangesCallCount.Should().Be(1);
        notifier.ClaimedEvents.Should().ContainSingle();
    }

    [Fact]
    public async Task ClaimConversation_ReturnsConflict_WhenAlreadyClaimedByOther()
    {
        var clinicId = Guid.NewGuid();
        var conv = MakeConversation(clinicId, assignedStaff: "Bob", claimedAt: DateTime.UtcNow.AddMinutes(-5));
        var convRepo = new FakeConversationRepository([conv]);
        var notifier = new SpyInboxNotifier();
        var uow = new FakeUnitOfWork();

        var handler = new ClaimConversationHandler(convRepo, notifier, uow,
            NullLogger<ClaimConversationHandler>.Instance);
        var result = await handler.HandleAsync(new ClaimConversationCommand(conv.Id, clinicId, "Alice"));

        result.Success.Should().BeFalse();
        result.ConflictingStaff.Should().Be("Bob");
        uow.SaveChangesCallCount.Should().Be(0);
        notifier.ClaimedEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task ClaimConversation_AllowsOverride_WhenClaimIsStale()
    {
        var clinicId = Guid.NewGuid();
        var conv = MakeConversation(clinicId, assignedStaff: "Bob", claimedAt: DateTime.UtcNow.AddMinutes(-35));
        var convRepo = new FakeConversationRepository([conv]);
        var notifier = new SpyInboxNotifier();
        var uow = new FakeUnitOfWork();

        var handler = new ClaimConversationHandler(convRepo, notifier, uow,
            NullLogger<ClaimConversationHandler>.Instance);
        var result = await handler.HandleAsync(new ClaimConversationCommand(conv.Id, clinicId, "Alice"));

        result.Success.Should().BeTrue();
        conv.AssignedStaff.Should().Be("Alice");
    }

    [Fact]
    public async Task ClaimConversation_AllowsSamePerson_ToReclaimOwnConversation()
    {
        var clinicId = Guid.NewGuid();
        var conv = MakeConversation(clinicId, assignedStaff: "Alice", claimedAt: DateTime.UtcNow.AddMinutes(-5));
        var convRepo = new FakeConversationRepository([conv]);
        var handler = new ClaimConversationHandler(convRepo, new SpyInboxNotifier(), new FakeUnitOfWork(),
            NullLogger<ClaimConversationHandler>.Instance);

        var result = await handler.HandleAsync(new ClaimConversationCommand(conv.Id, clinicId, "Alice"));

        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ClaimConversation_Throws_WhenConversationNotFound()
    {
        var clinicId = Guid.NewGuid();
        var handler = new ClaimConversationHandler(new FakeConversationRepository(), new SpyInboxNotifier(),
            new FakeUnitOfWork(), NullLogger<ClaimConversationHandler>.Instance);

        var act = () => handler.HandleAsync(new ClaimConversationCommand(Guid.NewGuid(), clinicId, "Alice"));

        await act.Should().ThrowAsync<BusinessException>()
            .Where(ex => ex.Code == BusinessErrorCode.ConversationNotFound);
    }

    // ── ReleaseConversationHandler ────────────────────────────────────────

    [Fact]
    public async Task ReleaseConversation_ClearsClaimAndSetsOpen()
    {
        var clinicId = Guid.NewGuid();
        var conv = MakeConversation(clinicId, assignedStaff: "Alice", claimedAt: DateTime.UtcNow.AddMinutes(-5));
        conv.Status = "InProgress";
        var convRepo = new FakeConversationRepository([conv]);
        var notifier = new SpyInboxNotifier();
        var uow = new FakeUnitOfWork();

        var handler = new ReleaseConversationHandler(convRepo, notifier, uow,
            NullLogger<ReleaseConversationHandler>.Instance);
        await handler.HandleAsync(new ReleaseConversationCommand(conv.Id, clinicId));

        conv.AssignedStaff.Should().BeNull();
        conv.ClaimedAt.Should().BeNull();
        conv.Status.Should().Be("Open");
        uow.SaveChangesCallCount.Should().Be(1);
        notifier.ClaimedEvents.Should().ContainSingle()
            .Which.AssignedStaff.Should().BeNull();
    }

    [Fact]
    public async Task ReleaseConversation_Throws_WhenConversationNotFound()
    {
        var clinicId = Guid.NewGuid();
        var handler = new ReleaseConversationHandler(new FakeConversationRepository(), new SpyInboxNotifier(),
            new FakeUnitOfWork(), NullLogger<ReleaseConversationHandler>.Instance);

        var act = () => handler.HandleAsync(new ReleaseConversationCommand(Guid.NewGuid(), clinicId));

        await act.Should().ThrowAsync<BusinessException>()
            .Where(ex => ex.Code == BusinessErrorCode.ConversationNotFound);
    }

    // ── MarkConversationReadHandler ───────────────────────────────────────

    [Fact]
    public async Task MarkRead_SetsIsReadAndResetsUnreadCount()
    {
        var clinicId = Guid.NewGuid();
        var conv = MakeConversation(clinicId);
        conv.IsRead = false;
        conv.UnreadCount = 3;
        var convRepo = new FakeConversationRepository([conv]);
        var uow = new FakeUnitOfWork();

        var handler = new MarkConversationReadHandler(convRepo, uow,
            NullLogger<MarkConversationReadHandler>.Instance);
        await handler.HandleAsync(new MarkConversationReadCommand(conv.Id, clinicId));

        conv.IsRead.Should().BeTrue();
        conv.UnreadCount.Should().Be(0);
        uow.SaveChangesCallCount.Should().Be(1);
    }

    [Fact]
    public async Task MarkRead_SkipsSave_WhenAlreadyRead()
    {
        var clinicId = Guid.NewGuid();
        var conv = MakeConversation(clinicId);
        conv.IsRead = true;
        conv.UnreadCount = 0;
        var convRepo = new FakeConversationRepository([conv]);
        var uow = new FakeUnitOfWork();

        var handler = new MarkConversationReadHandler(convRepo, uow,
            NullLogger<MarkConversationReadHandler>.Instance);
        await handler.HandleAsync(new MarkConversationReadCommand(conv.Id, clinicId));

        uow.SaveChangesCallCount.Should().Be(0);
    }

    // ── GetInboxConversationsHandler — new fields ─────────────────────────

    [Fact]
    public async Task GetConversations_IncludesLastMessagePreview()
    {
        var clinicId = Guid.NewGuid();
        var convId = Guid.NewGuid();
        var conv = new Conversation
        {
            Id = convId, ClinicId = clinicId, Channel = "LINE",
            ExternalConversationId = "x", CustomerDisplayName = "A"
        };
        var message = new Message
        {
            Id = Guid.NewGuid(), ClinicId = clinicId, ConversationId = convId,
            SenderType = "Customer", Text = "สวัสดีครับ", SentAtUtc = DateTime.UtcNow
        };
        var convRepo = new FakeConversationRepository([conv]);
        var msgRepo = new FakeMessageRepository([message]);

        var handler = new GetInboxConversationsHandler(convRepo, msgRepo,
            NullLogger<GetInboxConversationsHandler>.Instance);
        var result = await handler.HandleAsync(new GetInboxConversationsQuery(clinicId, 10));

        result.Should().ContainSingle();
        result[0].LastMessagePreview.Should().Be("สวัสดีครับ");
    }

    [Fact]
    public async Task GetConversations_TruncatesPreviewAt60Chars()
    {
        var clinicId = Guid.NewGuid();
        var convId = Guid.NewGuid();
        var conv = new Conversation
        {
            Id = convId, ClinicId = clinicId, Channel = "LINE",
            ExternalConversationId = "x", CustomerDisplayName = "A"
        };
        var longText = new string('x', 70);
        var message = new Message
        {
            Id = Guid.NewGuid(), ClinicId = clinicId, ConversationId = convId,
            SenderType = "Customer", Text = longText, SentAtUtc = DateTime.UtcNow
        };
        var handler = new GetInboxConversationsHandler(
            new FakeConversationRepository([conv]),
            new FakeMessageRepository([message]),
            NullLogger<GetInboxConversationsHandler>.Instance);

        var result = await handler.HandleAsync(new GetInboxConversationsQuery(clinicId, 10));

        result[0].LastMessagePreview.Should().HaveLength(61); // 60 chars + "…"
        result[0].LastMessagePreview.Should().EndWith("…");
    }

    [Fact]
    public async Task GetConversations_MasksStaleClaim_WithoutThrowingOrWritingDb()
    {
        var clinicId = Guid.NewGuid();
        var conv = MakeConversation(clinicId, assignedStaff: "Bob", claimedAt: DateTime.UtcNow.AddMinutes(-35));
        conv.Status = "InProgress";
        var handler = new GetInboxConversationsHandler(
            new FakeConversationRepository([conv]),
            new FakeMessageRepository([]),
            NullLogger<GetInboxConversationsHandler>.Instance);

        var result = await handler.HandleAsync(new GetInboxConversationsQuery(clinicId, 10));

        result[0].AssignedStaff.Should().BeNull();
        result[0].Status.Should().Be("Open");
        result[0].ClaimedAt.Should().BeNull();
    }

    [Fact]
    public async Task GetConversations_PreservesActiveClaim()
    {
        var clinicId = Guid.NewGuid();
        var conv = MakeConversation(clinicId, assignedStaff: "Bob", claimedAt: DateTime.UtcNow.AddMinutes(-5));
        conv.Status = "InProgress";
        var handler = new GetInboxConversationsHandler(
            new FakeConversationRepository([conv]),
            new FakeMessageRepository([]),
            NullLogger<GetInboxConversationsHandler>.Instance);

        var result = await handler.HandleAsync(new GetInboxConversationsQuery(clinicId, 10));

        result[0].AssignedStaff.Should().Be("Bob");
        result[0].Status.Should().Be("InProgress");
    }

    // ── ReceiveMessageHandler — idempotency & UnreadCount ─────────────────

    [Fact]
    public async Task ReceiveMessage_IncrementsUnreadCount_OnNewInboundMessage()
    {
        var clinicId = Guid.NewGuid();
        var convId = Guid.NewGuid();
        var conv = new Conversation
        {
            Id = convId, ClinicId = clinicId, Channel = "LINE",
            ExternalConversationId = "line-1", CustomerDisplayName = "A",
            IsRead = false, UnreadCount = 1
        };
        var convRepo = new FakeConversationRepository([conv]);
        var msgRepo = new FakeMessageRepository([]);
        var uow = new FakeUnitOfWork();
        var handler = MakeReceiveHandler(convRepo, msgRepo, uow);

        await handler.HandleAsync(new ClinicMateAI.Application.Messaging.ReceiveMessageCommand(
            clinicId, "LINE", "line-1", "A", "สวัสดีครับ", DateTimeOffset.UtcNow));

        conv.UnreadCount.Should().Be(2);
        conv.IsRead.Should().BeFalse();
    }

    [Fact]
    public async Task ReceiveMessage_SkipsInsert_WhenExternalMessageIdAlreadyExists()
    {
        var clinicId = Guid.NewGuid();
        var conv = new Conversation
        {
            ClinicId = clinicId, Channel = "LINE",
            ExternalConversationId = "line-1", CustomerDisplayName = "A"
        };
        var existing = new Message
        {
            ClinicId = clinicId, ConversationId = conv.Id, SenderType = "Customer",
            Text = "hello", SentAtUtc = DateTime.UtcNow, ExternalMessageId = "msg-123"
        };
        var convRepo = new FakeConversationRepository([conv]);
        var msgRepo = new FakeMessageRepository([existing]);
        var uow = new FakeUnitOfWork();
        var handler = MakeReceiveHandler(convRepo, msgRepo, uow);

        await handler.HandleAsync(new ClinicMateAI.Application.Messaging.ReceiveMessageCommand(
            clinicId, "LINE", "line-1", "A", "hello", DateTimeOffset.UtcNow, "msg-123"));

        msgRepo.Items.Should().ContainSingle(); // no duplicate added
        uow.SaveChangesCallCount.Should().Be(0);
    }

    [Fact]
    public async Task ReceiveMessage_SetsAiStatus_AfterProcessing()
    {
        var clinicId = Guid.NewGuid();
        var convRepo = new FakeConversationRepository();
        var msgRepo = new FakeMessageRepository([]);
        var uow = new FakeUnitOfWork();
        var handler = MakeReceiveHandler(convRepo, msgRepo, uow);

        await handler.HandleAsync(new ClinicMateAI.Application.Messaging.ReceiveMessageCommand(
            clinicId, "LINE", "line-new", "A", "โบท็อกราคาเท่าไร", DateTimeOffset.UtcNow));

        var saved = convRepo.Items.Should().ContainSingle().Subject;
        saved.AiStatus.Should().NotBeNullOrEmpty();
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private static Conversation MakeConversation(Guid clinicId, string? assignedStaff = null, DateTime? claimedAt = null)
        => new()
        {
            Id = Guid.NewGuid(),
            ClinicId = clinicId,
            Channel = "LINE",
            ExternalConversationId = Guid.NewGuid().ToString(),
            CustomerDisplayName = "Test Customer",
            AssignedStaff = assignedStaff,
            ClaimedAt = claimedAt
        };

    private static ClinicMateAI.Logic.Messaging.ReceiveMessageHandler MakeReceiveHandler(
        FakeConversationRepository convRepo,
        FakeMessageRepository msgRepo,
        FakeUnitOfWork uow)
        => new(
            new ClinicMateAI.Logic.Messaging.ReceiveMessageCommandValidator(),
            convRepo,
            msgRepo,
            new ClinicMateAI.Logic.Ai.SimulatedAiReplyProvider(),
            new NullInboxNotifier(),
            uow,
            Microsoft.Extensions.Logging.Abstractions.NullLogger<ClinicMateAI.Logic.Messaging.ReceiveMessageHandler>.Instance);

    // ── Test doubles ──────────────────────────────────────────────────────

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class SpyInboxNotifier : IInboxNotifier
    {
        public List<ConversationClaimedEvent> ClaimedEvents { get; } = [];

        public Task NotifyConversationUpdatedAsync(Guid clinicId, ConversationUpdatedEvent evt, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task NotifyNewMessageAsync(Guid clinicId, Guid conversationId, NewMessageEvent evt, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task NotifyConversationClaimedAsync(Guid clinicId, ConversationClaimedEvent evt, CancellationToken cancellationToken = default)
        {
            ClaimedEvents.Add(evt);
            return Task.CompletedTask;
        }
    }

    private sealed class NullInboxNotifier : IInboxNotifier
    {
        public Task NotifyConversationUpdatedAsync(Guid clinicId, ConversationUpdatedEvent evt, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyNewMessageAsync(Guid clinicId, Guid conversationId, NewMessageEvent evt, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task NotifyConversationClaimedAsync(Guid clinicId, ConversationClaimedEvent evt, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeConversationRepository : IConversationRepository
    {
        public List<Conversation> Items { get; }

        public FakeConversationRepository() { Items = []; }
        public FakeConversationRepository(IEnumerable<Conversation> seed) { Items = seed.ToList(); }

        public Task<Conversation?> GetByIdAsync(Guid clinicId, Guid conversationId, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(x => x.ClinicId == clinicId && x.Id == conversationId));

        public Task<Conversation?> GetByIdAsync(Guid clinicId, Guid branchId, Guid conversationId, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(x => x.ClinicId == clinicId && x.BranchId == branchId && x.Id == conversationId));

        public Task<Conversation?> GetByExternalIdAsync(Guid clinicId, string channel, string externalConversationId, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(x => x.ClinicId == clinicId && x.Channel == channel && x.ExternalConversationId == externalConversationId));

        public Task<Conversation?> GetByExternalIdAsync(Guid clinicId, Guid branchId, string channel, string externalConversationId, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.FirstOrDefault(x => x.ClinicId == clinicId && x.BranchId == branchId && x.Channel == channel && x.ExternalConversationId == externalConversationId));

        public Task<IReadOnlyList<Conversation>> ListRecentAsync(Guid clinicId, int take, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Conversation> result = Items
                .Where(x => x.ClinicId == clinicId)
                .OrderByDescending(x => x.LastMessageAtUtc)
                .Take(take)
                .ToList();
            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<Conversation>> ListRecentAsync(Guid clinicId, Guid? branchId, int take, CancellationToken cancellationToken = default)
        {
            var query = Items.Where(x => x.ClinicId == clinicId);
            if (branchId is not null)
            {
                query = query.Where(x => x.BranchId == branchId.Value);
            }

            IReadOnlyList<Conversation> result = query
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
            var i = Items.FindIndex(x => x.Id == conversation.Id);
            if (i >= 0) Items[i] = conversation;
        }
    }

    private sealed class FakeMessageRepository : IMessageRepository
    {
        public List<Message> Items { get; }

        public FakeMessageRepository(IEnumerable<Message> seed) { Items = seed.ToList(); }

        public Task AddAsync(Message message, CancellationToken cancellationToken = default)
        {
            Items.Add(message);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Message>> ListByConversationAsync(Guid clinicId, Guid conversationId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Message> result = Items
                .Where(x => x.ClinicId == clinicId && x.ConversationId == conversationId)
                .OrderBy(x => x.SentAtUtc)
                .ToList();
            return Task.FromResult(result);
        }

        public Task<bool> ExistsAsync(Guid clinicId, string externalMessageId, CancellationToken cancellationToken = default)
            => Task.FromResult(Items.Any(x => x.ClinicId == clinicId && x.ExternalMessageId == externalMessageId));

        public Task<Message?> GetLastInboundAsync(Guid clinicId, Guid conversationId, CancellationToken cancellationToken = default)
        {
            Message? result = Items
                .Where(x => x.ClinicId == clinicId && x.ConversationId == conversationId && x.SenderType == "Customer")
                .OrderByDescending(x => x.SentAtUtc)
                .FirstOrDefault();
            return Task.FromResult(result);
        }
    }
}
