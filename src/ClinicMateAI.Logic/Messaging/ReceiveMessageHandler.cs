using ClinicMateAI.Application.Abstractions.Messaging;
using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Ai;
using ClinicMateAI.Application.Messaging;
using ClinicMateAI.Domain.Ai;
using ClinicMateAI.Domain.Messaging;
using ClinicMateAI.Logic.Ai;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace ClinicMateAI.Logic.Messaging;

public sealed class ReceiveMessageHandler(
    IValidator<ReceiveMessageCommand> validator,
    IConversationRepository conversationRepository,
    IMessageRepository messageRepository,
    IAiReplyProvider aiReplyProvider,
    IInboxNotifier inboxNotifier,
    IUnitOfWork unitOfWork,
    ILogger<ReceiveMessageHandler> logger) : IReceiveMessageHandler
{
    public async Task<ReceiveMessageResult> HandleAsync(
        ReceiveMessageCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        // Idempotency: skip if we've already processed this external message
        if (command.ExternalMessageId is not null
            && await messageRepository.ExistsAsync(command.ClinicId, command.ExternalMessageId, cancellationToken))
        {
            var existing = await conversationRepository.GetByExternalIdAsync(
                command.ClinicId, command.Channel, command.ExternalConversationId, cancellationToken);
            return new ReceiveMessageResult(existing?.Id ?? Guid.Empty, Guid.Empty, false, null);
        }

        var conversation = await conversationRepository.GetByExternalIdAsync(
            command.ClinicId, command.Channel, command.ExternalConversationId, cancellationToken);

        bool isNewConversation = conversation is null;

        if (isNewConversation)
        {
            conversation = new Conversation
            {
                ClinicId = command.ClinicId,
                Channel = command.Channel,
                ExternalConversationId = command.ExternalConversationId,
                CustomerDisplayName = command.CustomerDisplayName,
                LastMessageAtUtc = command.ReceivedAt.UtcDateTime,
                IsRead = false,
                UnreadCount = 1
            };
            await conversationRepository.AddAsync(conversation, cancellationToken);
        }
        else
        {
            conversation.CustomerDisplayName = command.CustomerDisplayName;
            conversation.LastMessageAtUtc = command.ReceivedAt.UtcDateTime;
            conversation.IsRead = false;
            conversation.UnreadCount++;
            conversationRepository.Update(conversation);
        }

        var inboundMessage = new Message
        {
            ClinicId = command.ClinicId,
            ConversationId = conversation.Id,
            SenderType = "Customer",
            Text = command.Text,
            SentAtUtc = command.ReceivedAt.UtcDateTime,
            ExternalMessageId = command.ExternalMessageId
        };
        await messageRepository.AddAsync(inboundMessage, cancellationToken);

        // AI reply — degrade gracefully on timeout or failure
        string? aiStatus = null;
        string? replyText = null;
        bool requiresHandoff = false;

        try
        {
            using var aiCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            aiCts.CancelAfter(TimeSpan.FromSeconds(15));

            var orchestrator = new AiReceptionistOrchestrator(aiReplyProvider);
            var aiResult = await orchestrator.GenerateReplyAsync(
                new AiReplyRequest(command.Text, HasApprovedData: true, Confidence: 0.9m, ApprovedClinicFacts: string.Empty),
                aiCts.Token);

            requiresHandoff = aiResult.Mode == AiReplyMode.Escalate || aiResult.Mode == AiReplyMode.DraftForStaff;
            replyText = aiResult.ReplyText;
            aiStatus = requiresHandoff ? "DraftReady" : "AutoReplied";
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "AI reply failed for conversation {ConversationId} — setting DraftReady", conversation.Id);
            aiStatus = "DraftReady";
            requiresHandoff = true;
        }

        conversation.AiStatus = aiStatus ?? "None";
        if (!isNewConversation)
            conversationRepository.Update(conversation);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // SignalR notify — best-effort, never throw
        try
        {
            var preview = inboundMessage.Text.Length > 60 ? inboundMessage.Text[..60] + "…" : inboundMessage.Text;
            await inboxNotifier.NotifyConversationUpdatedAsync(
                command.ClinicId,
                new ConversationUpdatedEvent(
                    conversation.Id,
                    conversation.CustomerDisplayName,
                    conversation.Channel,
                    conversation.Status,
                    conversation.AiStatus,
                    conversation.IsRead,
                    conversation.UnreadCount,
                    conversation.AssignedStaff,
                    conversation.LastMessageAtUtc,
                    preview),
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "SignalR notify failed for conversation {ConversationId} — message is persisted.", conversation.Id);
        }

        return new ReceiveMessageResult(conversation.Id, inboundMessage.Id, requiresHandoff, replyText);
    }
}
