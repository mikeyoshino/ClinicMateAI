using ClinicMateAI.Application.Abstractions.Messaging;
using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Setup;
using ClinicMateAI.Domain.Clinics;
using FluentValidation;

namespace ClinicMateAI.Logic.Setup;

public sealed class CompleteFacebookConnectionHandler(
    IValidator<CompleteFacebookConnectionCommand> validator,
    IClinicChannelConfigRepository repository,
    IFacebookConnectionProvider provider,
    IUnitOfWork unitOfWork) : ICompleteFacebookConnectionHandler
{
    public async Task HandleAsync(
        CompleteFacebookConnectionCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var result = await provider.CompleteAsync(command.ClinicId, command.AuthorizationCode, cancellationToken);

        var config = await repository.GetByClinicAndChannelIncludingDisabledAsync(command.ClinicId, "Facebook", cancellationToken)
            ?? new ClinicChannelConfig
            {
                ClinicId = command.ClinicId,
                Channel = "Facebook",
                Secret = string.Empty
            };

        config.AccessToken = result.AccessToken;
        config.RefreshTokenOrLongLivedToken = result.LongLivedToken;
        config.TokenExpiresAtUtc = result.TokenExpiresAtUtc;
        config.ExternalPageId = result.PageId;
        config.ConnectionStatus = ChannelConnectionStatus.Connected;
        config.LastVerifiedAtUtc = DateTime.UtcNow;
        config.LastError = string.Empty;
        config.IsEnabled = true;
        config.UpdatedAtUtc = DateTime.UtcNow;

        await repository.UpsertAsync(config, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
