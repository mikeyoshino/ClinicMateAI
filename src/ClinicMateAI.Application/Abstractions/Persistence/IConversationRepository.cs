using ClinicMateAI.Domain.Messaging;

namespace ClinicMateAI.Application.Abstractions.Persistence;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid clinicId, Guid conversationId, CancellationToken cancellationToken = default);
    Task<Conversation?> GetByExternalIdAsync(Guid clinicId, string channel, string externalConversationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Conversation>> ListRecentAsync(Guid clinicId, int take, CancellationToken cancellationToken = default);
    Task AddAsync(Conversation conversation, CancellationToken cancellationToken = default);
    void Update(Conversation conversation);
}
