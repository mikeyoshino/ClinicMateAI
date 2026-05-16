using ClinicMateAI.Application.Abstractions.Messaging;

namespace ClinicMateAI.Application.Setup;

public interface IStartFacebookConnectionHandler : ICommandHandler<StartFacebookConnectionCommand, StartFacebookConnectionResult>;
