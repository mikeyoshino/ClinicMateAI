namespace ClinicMateAI.Application.Inbox;

public interface IMarkConversationReadHandler
{
    Task HandleAsync(MarkConversationReadCommand command, CancellationToken cancellationToken = default);
}
