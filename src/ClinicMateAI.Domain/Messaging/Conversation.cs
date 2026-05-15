namespace ClinicMateAI.Domain.Messaging;

public sealed class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClinicId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string ExternalConversationId { get; set; } = string.Empty;
    public string CustomerDisplayName { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public DateTime LastMessageAtUtc { get; set; } = DateTime.UtcNow;
}
