using ClinicMateAI.Application.Abstractions.Messaging;
using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Inbox;
using ClinicMateAI.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ClinicMateAI.Logic.Inbox;

public sealed class ReleaseConversationHandler(
    IConversationRepository conversationRepository,
    IInboxNotifier inboxNotifier,
    IUnitOfWork unitOfWork,
    ILogger<ReleaseConversationHandler> logger) : IReleaseConversationHandler
{
    public async Task HandleAsync(
        ReleaseConversationCommand command,
        CancellationToken cancellationToken = default)
    {
        var conversation = await conversationRepository.GetByIdAsync(
            command.ClinicId, command.ConversationId, cancellationToken);

        if (conversation is null)
        {
            logger.LogWarning(
                "Release failed — conversation not found: {ConversationId} (clinic {ClinicId})",
                command.ConversationId, command.ClinicId);
            throw new BusinessException(BusinessErrorCode.ConversationNotFound);
        }

        conversation.AssignedStaff = null;
        conversation.ClaimedAt = null;
        conversation.Status = "Open";
        conversationRepository.Update(conversation);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await inboxNotifier.NotifyConversationClaimedAsync(
                command.ClinicId,
                new ConversationClaimedEvent(conversation.Id, null, "Open"),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "SignalR notify failed for release on conversation {ConversationId} — release is persisted",
                command.ConversationId);
        }
    }
}
