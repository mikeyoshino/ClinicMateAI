namespace ClinicMateAI.Application.Inbox;

public interface IClaimConversationHandler
{
    Task<ClaimConversationResult> HandleAsync(ClaimConversationCommand command, CancellationToken cancellationToken = default);
}
