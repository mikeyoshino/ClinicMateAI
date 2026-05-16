using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Inbox;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Domain.Messaging;
using ClinicMateAI.Logic.Inbox;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClinicMateAI.Tests.Inbox;

public class InboxQueryHandlersTests
{
    [Fact]
    public async Task GetInboxClinicsHandler_ReturnsOrderedClinicDtos()
    {
        var repo = new FakeClinicRepository(
        [
            new Clinic { Id = Guid.NewGuid(), Name = "B Clinic" },
            new Clinic { Id = Guid.NewGuid(), Name = "A Clinic" }
        ]);
        var handler = new GetInboxClinicsHandler(repo);

        var result = await handler.HandleAsync(new GetInboxClinicsQuery());

        result.Should().HaveCount(2);
        result[0].Name.Should().Be("B Clinic");
        result[1].Name.Should().Be("A Clinic");
    }

    [Fact]
    public async Task GetInboxConversationsHandler_UsesDefaultTakeWhenInvalid()
    {
        var clinicId = Guid.NewGuid();
        var repo = new FakeConversationRepository();
        var handler = new GetInboxConversationsHandler(repo, new FakeMessageRepository([]), NullLogger<GetInboxConversationsHandler>.Instance);

        await handler.HandleAsync(new GetInboxConversationsQuery(clinicId, Take: 0));

        repo.LastTake.Should().Be(50);
    }

    [Fact]
    public async Task GetInboxConversationsHandler_ClampsTakeToMax200()
    {
        var clinicId = Guid.NewGuid();
        var repo = new FakeConversationRepository();
        var handler = new GetInboxConversationsHandler(repo, new FakeMessageRepository([]), NullLogger<GetInboxConversationsHandler>.Instance);

        await handler.HandleAsync(new GetInboxConversationsQuery(clinicId, Take: 999));

        repo.LastTake.Should().Be(200);
    }

    [Fact]
    public async Task GetConversationMessagesHandler_ReturnsEmpty_WhenConversationNotInClinic()
    {
        var clinicId = Guid.NewGuid();
        var conversationRepo = new FakeConversationRepository();
        var messageRepo = new FakeMessageRepository(
        [
            new Message
            {
                ClinicId = clinicId,
                ConversationId = Guid.NewGuid(),
                SenderType = "Customer",
                Text = "hello",
                SentAtUtc = DateTime.UtcNow
            }
        ]);

        var handler = new GetConversationMessagesHandler(conversationRepo, messageRepo);
        var result = await handler.HandleAsync(new GetConversationMessagesQuery(clinicId, Guid.NewGuid()));

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetConversationMessagesHandler_ReturnsMessagesForClinicConversation()
    {
        var clinicId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        var conversationRepo = new FakeConversationRepository(
        [
            new Conversation
            {
                Id = conversationId,
                ClinicId = clinicId,
                Channel = "LINE",
                ExternalConversationId = "line-1",
                CustomerDisplayName = "A"
            }
        ]);
        var messageRepo = new FakeMessageRepository(
        [
            new Message
            {
                Id = Guid.NewGuid(),
                ClinicId = clinicId,
                ConversationId = conversationId,
                SenderType = "Customer",
                Text = "hello",
                SentAtUtc = DateTime.UtcNow
            }
        ]);

        var handler = new GetConversationMessagesHandler(conversationRepo, messageRepo);
        var result = await handler.HandleAsync(new GetConversationMessagesQuery(clinicId, conversationId));

        result.Should().ContainSingle();
        result[0].Text.Should().Be("hello");
    }

    private sealed class FakeClinicRepository(IEnumerable<Clinic> seed) : IClinicRepository
    {
        private readonly List<Clinic> _items = seed.ToList();

        public Task<Clinic?> GetByIdAsync(Guid clinicId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_items.FirstOrDefault(x => x.Id == clinicId));
        }

        public Task<IReadOnlyList<Clinic>> ListAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Clinic> result = _items.ToList();
            return Task.FromResult(result);
        }

        public Task AddAsync(Clinic clinic, CancellationToken cancellationToken = default)
        {
            _items.Add(clinic);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Clinic clinic, CancellationToken cancellationToken = default)
        {
            var index = _items.FindIndex(x => x.Id == clinic.Id);
            if (index >= 0)
            {
                _items[index] = clinic;
            }

            return Task.CompletedTask;
        }

        public Task<(IReadOnlyList<Clinic> Items, int TotalCount)> SearchAsync(
            string? name,
            DateTime? createdFromUtc,
            DateTime? createdToExclusiveUtc,
            string? status,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default)
        {
            var query = _items.AsEnumerable();
            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(x => x.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
            }

            var filtered = query.ToList();
            var paged = filtered.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return Task.FromResult(((IReadOnlyList<Clinic>)paged, filtered.Count));
        }
    }

    private sealed class FakeConversationRepository : IConversationRepository
    {
        private readonly List<Conversation> _items;
        public int LastTake { get; private set; }

        public FakeConversationRepository()
        {
            _items = [];
        }

        public FakeConversationRepository(IEnumerable<Conversation> seed)
        {
            _items = seed.ToList();
        }

        public Task<Conversation?> GetByIdAsync(Guid clinicId, Guid conversationId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_items.FirstOrDefault(x => x.ClinicId == clinicId && x.Id == conversationId));
        }

        public Task<Conversation?> GetByIdAsync(Guid clinicId, Guid branchId, Guid conversationId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_items.FirstOrDefault(x => x.ClinicId == clinicId && x.BranchId == branchId && x.Id == conversationId));
        }

        public Task<Conversation?> GetByExternalIdAsync(Guid clinicId, string channel, string externalConversationId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_items.FirstOrDefault(x =>
                x.ClinicId == clinicId &&
                x.Channel == channel &&
                x.ExternalConversationId == externalConversationId));
        }

        public Task<Conversation?> GetByExternalIdAsync(Guid clinicId, Guid branchId, string channel, string externalConversationId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_items.FirstOrDefault(x =>
                x.ClinicId == clinicId &&
                x.BranchId == branchId &&
                x.Channel == channel &&
                x.ExternalConversationId == externalConversationId));
        }

        public Task<IReadOnlyList<Conversation>> ListRecentAsync(Guid clinicId, int take, CancellationToken cancellationToken = default)
        {
            LastTake = take;
            IReadOnlyList<Conversation> result = _items
                .Where(x => x.ClinicId == clinicId)
                .OrderByDescending(x => x.LastMessageAtUtc)
                .Take(take)
                .ToList();
            return Task.FromResult(result);
        }

        public Task<IReadOnlyList<Conversation>> ListRecentAsync(Guid clinicId, Guid? branchId, int take, CancellationToken cancellationToken = default)
        {
            LastTake = take;
            var query = _items.Where(x => x.ClinicId == clinicId);
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
            _items.Add(conversation);
            return Task.CompletedTask;
        }

        public void Update(Conversation conversation)
        {
            var index = _items.FindIndex(x => x.Id == conversation.Id);
            if (index >= 0)
            {
                _items[index] = conversation;
            }
        }
    }

    private sealed class FakeMessageRepository(IEnumerable<Message> seed) : IMessageRepository
    {
        private readonly List<Message> _items = seed.ToList();

        public Task AddAsync(Message message, CancellationToken cancellationToken = default)
        {
            _items.Add(message);
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<Message>> ListByConversationAsync(Guid clinicId, Guid conversationId, CancellationToken cancellationToken = default)
        {
            IReadOnlyList<Message> result = _items
                .Where(x => x.ClinicId == clinicId && x.ConversationId == conversationId)
                .OrderBy(x => x.SentAtUtc)
                .ToList();
            return Task.FromResult(result);
        }

        public Task<bool> ExistsAsync(Guid clinicId, string externalMessageId, CancellationToken cancellationToken = default)
            => Task.FromResult(_items.Any(x => x.ClinicId == clinicId && x.ExternalMessageId == externalMessageId));

        public Task<Message?> GetLastInboundAsync(Guid clinicId, Guid conversationId, CancellationToken cancellationToken = default)
        {
            Message? result = _items
                .Where(x => x.ClinicId == clinicId && x.ConversationId == conversationId && x.SenderType == "Customer")
                .OrderByDescending(x => x.SentAtUtc)
                .FirstOrDefault();
            return Task.FromResult(result);
        }
    }
}
