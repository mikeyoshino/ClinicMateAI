namespace ClinicMateAI.Application.Inbox;

public sealed record InboxConversationDto(
    Guid ConversationId,
    string Channel,
    string ExternalConversationId,
    string CustomerDisplayName,
    string Status,
    string AiStatus,
    bool IsRead,
    int UnreadCount,
    string? AssignedStaff,
    DateTime? ClaimedAt,
    DateTime LastMessageAtUtc,
    string LastMessagePreview);
