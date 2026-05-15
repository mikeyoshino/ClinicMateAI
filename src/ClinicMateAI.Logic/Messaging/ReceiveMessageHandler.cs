using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Ai;
using ClinicMateAI.Application.Messaging;
using ClinicMateAI.Domain.Ai;
using ClinicMateAI.Domain.Messaging;
using ClinicMateAI.Logic.Ai;
using FluentValidation;

namespace ClinicMateAI.Logic.Messaging;

public sealed class ReceiveMessageHandler(
    IValidator<ReceiveMessageCommand> validator,
    IConversationRepository conversationRepository,
    IMessageRepository messageRepository,
    IAiReplyProvider aiReplyProvider,
    IUnitOfWork unitOfWork) : IReceiveMessageHandler
{
    public async Task<ReceiveMessageResult> HandleAsync(
        ReceiveMessageCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var conversation = await conversationRepository.GetByExternalIdAsync(
            command.ClinicId,
            command.Channel,
            command.ExternalConversationId,
            cancellationToken);

        if (conversation is null)
        {
            conversation = new Conversation
            {
                ClinicId = command.ClinicId,
                Channel = command.Channel,
                ExternalConversationId = command.ExternalConversationId,
                CustomerDisplayName = command.CustomerDisplayName,
                LastMessageAtUtc = command.ReceivedAt.UtcDateTime
            };
            await conversationRepository.AddAsync(conversation, cancellationToken);
        }
        else
        {
            conversation.CustomerDisplayName = command.CustomerDisplayName;
            conversation.LastMessageAtUtc = command.ReceivedAt.UtcDateTime;
            conversationRepository.Update(conversation);
        }

        var inboundMessage = new Message
        {
            ClinicId = command.ClinicId,
            ConversationId = conversation.Id,
            SenderType = "Customer",
            Text = command.Text,
            SentAtUtc = command.ReceivedAt.UtcDateTime
        };
        await messageRepository.AddAsync(inboundMessage, cancellationToken);

        var orchestrator = new AiReceptionistOrchestrator(aiReplyProvider);
        var aiResult = await orchestrator.GenerateReplyAsync(
            new AiReplyRequest(command.Text, HasApprovedData: true, Confidence: 0.9m, ApprovedClinicFacts: string.Empty),
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new ReceiveMessageResult(
            ConversationId: conversation.Id,
            MessageId: inboundMessage.Id,
            RequiresHandoff: aiResult.Mode == AiReplyMode.Escalate || aiResult.Mode == AiReplyMode.DraftForStaff,
            ReplyText: aiResult.ReplyText);
    }
}
