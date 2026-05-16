namespace ClinicMateAI.Application.Inbox;

public sealed record ReleaseConversationCommand(
    Guid ConversationId,
    Guid ClinicId);
