using ClinicMateAI.Application.Abstractions.Messaging;
using ClinicMateAI.Application.Setup;

namespace ClinicMateAI.Logic.Setup;

public sealed class StartFacebookConnectionHandler(
    IFacebookConnectionProvider provider) : IStartFacebookConnectionHandler
{
    public Task<StartFacebookConnectionResult> HandleAsync(
        StartFacebookConnectionCommand command,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new StartFacebookConnectionResult(provider.BuildAuthorizationUrl(command.ClinicId)));
    }
}
