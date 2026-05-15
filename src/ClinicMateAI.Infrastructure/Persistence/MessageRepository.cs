using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Domain.Messaging;
using ClinicMateAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Infrastructure.Persistence;

public sealed class MessageRepository(AppDbContext dbContext) : IMessageRepository
{
    public Task AddAsync(Message message, CancellationToken cancellationToken = default)
    {
        return dbContext.Messages.AddAsync(message, cancellationToken).AsTask();
    }

    public async Task<IReadOnlyList<Message>> ListByConversationAsync(
        Guid clinicId,
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Messages
            .Where(x => x.ClinicId == clinicId && x.ConversationId == conversationId)
            .OrderBy(x => x.SentAtUtc)
            .ToListAsync(cancellationToken);
    }
}
