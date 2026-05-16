namespace ClinicMateAI.Domain.Messaging;

public sealed class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClinicId { get; set; }
    public Guid BranchId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string ExternalConversationId { get; set; } = string.Empty;
    public string CustomerDisplayName { get; set; } = string.Empty;

    // Open = AI handling, InProgress = claimed by staff, Resolved = closed
    public string Status { get; set; } = "Open";

    public DateTime LastMessageAtUtc { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    public int UnreadCount { get; set; } = 0;

    // AI reply state: None | AutoReplied | DraftReady | Escalated
    public string AiStatus { get; set; } = "None";

    // Claim ownership — prevents duplicate staff replies
    public string? AssignedStaff { get; set; }
    public DateTime? ClaimedAt { get; set; }
}
