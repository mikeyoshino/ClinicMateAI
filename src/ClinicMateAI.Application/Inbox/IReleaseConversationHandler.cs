namespace ClinicMateAI.Application.Inbox;

public interface IReleaseConversationHandler
{
    Task HandleAsync(ReleaseConversationCommand command, CancellationToken cancellationToken = default);
}
