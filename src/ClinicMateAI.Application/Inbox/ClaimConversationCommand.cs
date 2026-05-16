namespace ClinicMateAI.Application.Inbox;

public sealed record ClaimConversationCommand(
    Guid ConversationId,
    Guid ClinicId,
    string StaffName);

public sealed record ClaimConversationResult(
    bool Success,
    string? ConflictingStaff);
