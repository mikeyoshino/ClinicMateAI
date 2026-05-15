namespace ClinicMateAI.Application.Inbox;

public sealed record GetConversationMessagesQuery(Guid ClinicId, Guid ConversationId);
