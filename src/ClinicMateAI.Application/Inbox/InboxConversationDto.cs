namespace ClinicMateAI.Application.Inbox;

public sealed record InboxConversationDto(
    Guid ConversationId,
    string Channel,
    string ExternalConversationId,
    string CustomerDisplayName,
    string Status,
    DateTime LastMessageAtUtc);
