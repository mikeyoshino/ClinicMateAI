namespace ClinicMateAI.Application.Messaging;

public sealed record ReceiveMessageCommand(
    Guid ClinicId,
    string Channel,
    string ExternalConversationId,
    string CustomerDisplayName,
    string Text,
    DateTimeOffset ReceivedAt);
