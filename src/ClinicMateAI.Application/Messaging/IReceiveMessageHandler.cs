using ClinicMateAI.Application.Abstractions.Messaging;

namespace ClinicMateAI.Application.Messaging;

public interface IReceiveMessageHandler : ICommandHandler<ReceiveMessageCommand, ReceiveMessageResult>
{
}
