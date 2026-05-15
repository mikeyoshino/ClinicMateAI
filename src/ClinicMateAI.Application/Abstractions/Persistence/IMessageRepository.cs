using ClinicMateAI.Domain.Messaging;

namespace ClinicMateAI.Application.Abstractions.Persistence;

public interface IMessageRepository
{
    Task AddAsync(Message message, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Message>> ListByConversationAsync(Guid clinicId, Guid conversationId, CancellationToken cancellationToken = default);
}
