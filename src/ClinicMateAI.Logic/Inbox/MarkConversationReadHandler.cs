using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Inbox;
using Microsoft.Extensions.Logging;

namespace ClinicMateAI.Logic.Inbox;

public sealed class MarkConversationReadHandler(
    IConversationRepository conversationRepository,
    IUnitOfWork unitOfWork,
    ILogger<MarkConversationReadHandler> logger) : IMarkConversationReadHandler
{
    public async Task HandleAsync(
        MarkConversationReadCommand command,
        CancellationToken cancellationToken = default)
    {
        var conversation = await conversationRepository.GetByIdAsync(
            command.ClinicId, command.ConversationId, cancellationToken);

        // Idempotent: already read or not found → no-op (safe to ignore)
        if (conversation is null)
        {
            logger.LogDebug(
                "MarkRead no-op — conversation {ConversationId} not found (clinic {ClinicId})",
                command.ConversationId, command.ClinicId);
            return;
        }

        if (conversation.IsRead && conversation.UnreadCount == 0)
            return;

        conversation.IsRead = true;
        conversation.UnreadCount = 0;
        conversationRepository.Update(conversation);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
