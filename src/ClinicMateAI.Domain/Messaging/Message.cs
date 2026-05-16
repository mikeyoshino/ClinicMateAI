namespace ClinicMateAI.Domain.Messaging;

public sealed class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClinicId { get; set; }
    public Guid ConversationId { get; set; }
    public string SenderType { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
    public string? ExternalMessageId { get; set; }
}
