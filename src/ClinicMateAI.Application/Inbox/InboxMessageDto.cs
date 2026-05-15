namespace ClinicMateAI.Application.Inbox;

public sealed record InboxMessageDto(
    Guid MessageId,
    string SenderType,
    string Text,
    DateTime SentAtUtc);
