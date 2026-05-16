using ClinicMateAI.Application.Abstractions.Messaging;
using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Inbox;
using ClinicMateAI.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace ClinicMateAI.Logic.Inbox;

public sealed class ClaimConversationHandler(
    IConversationRepository conversationRepository,
    IInboxNotifier inboxNotifier,
    IUnitOfWork unitOfWork,
    ILogger<ClaimConversationHandler> logger) : IClaimConversationHandler
{
    public async Task<ClaimConversationResult> HandleAsync(
        ClaimConversationCommand command,
        CancellationToken cancellationToken = default)
    {
        var conversation = await conversationRepository.GetByIdAsync(
            command.ClinicId, command.ConversationId, cancellationToken);

        if (conversation is null)
        {
            logger.LogWarning(
                "Claim failed — conversation not found: {ConversationId} (clinic {ClinicId})",
                command.ConversationId, command.ClinicId);
            throw new BusinessException(BusinessErrorCode.ConversationNotFound);
        }

        // If already claimed by someone else and not stale, return conflict
        var now = DateTime.UtcNow;
        if (conversation.AssignedStaff is not null
            && conversation.AssignedStaff != command.StaffName
            && conversation.ClaimedAt.HasValue
            && (now - conversation.ClaimedAt.Value).TotalMinutes < 30)
        {
            logger.LogInformation(
                "Claim conflict: conversation {ConversationId} already claimed by {Staff}",
                command.ConversationId, conversation.AssignedStaff);
            return new ClaimConversationResult(false, conversation.AssignedStaff);
        }

        conversation.AssignedStaff = command.StaffName;
        conversation.ClaimedAt = now;
        conversation.Status = "InProgress";
        conversationRepository.Update(conversation);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            await inboxNotifier.NotifyConversationClaimedAsync(
                command.ClinicId,
                new ConversationClaimedEvent(conversation.Id, conversation.AssignedStaff, conversation.Status),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "SignalR notify failed for claim on conversation {ConversationId} — claim is persisted",
                command.ConversationId);
        }

        return new ClaimConversationResult(true, null);
    }
}
