namespace ClinicMateAI.Application.Abstractions.Messaging;

public interface IInboxNotifier
{
    Task NotifyConversationUpdatedAsync(Guid clinicId, ConversationUpdatedEvent evt, CancellationToken cancellationToken = default);
    Task NotifyNewMessageAsync(Guid clinicId, Guid conversationId, NewMessageEvent evt, CancellationToken cancellationToken = default);
    Task NotifyConversationClaimedAsync(Guid clinicId, ConversationClaimedEvent evt, CancellationToken cancellationToken = default);
}

public sealed record ConversationUpdatedEvent(
    Guid ConversationId,
    string CustomerDisplayName,
    string Channel,
    string Status,
    string AiStatus,
    bool IsRead,
    int UnreadCount,
    string? AssignedStaff,
    DateTime LastMessageAtUtc,
    string LastMessagePreview);

public sealed record NewMessageEvent(
    Guid MessageId,
    string SenderType,
    string Text,
    DateTime SentAtUtc);

public sealed record ConversationClaimedEvent(
    Guid ConversationId,
    string? AssignedStaff,
    string Status);
