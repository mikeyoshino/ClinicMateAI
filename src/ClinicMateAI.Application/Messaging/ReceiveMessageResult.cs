namespace ClinicMateAI.Application.Messaging;

public sealed record ReceiveMessageResult(
    Guid ConversationId,
    Guid MessageId,
    bool RequiresHandoff,
    string ReplyText);
