using ClinicMateAI.Application.Abstractions.Messaging;

namespace ClinicMateAI.Application.Setup;

public interface IRenewFacebookConnectionHandler : ICommandHandler<RenewFacebookConnectionCommand, FacebookTokenRenewalResult>
{
}
