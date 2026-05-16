namespace ClinicMateAI.Application.Inbox;

public sealed record MarkConversationReadCommand(
    Guid ConversationId,
    Guid ClinicId);
