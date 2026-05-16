using ClinicMateAI.Application.Abstractions.Messaging;
using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Setup;
using ClinicMateAI.Domain.Clinics;
using FluentValidation;

namespace ClinicMateAI.Logic.Setup;

public sealed class RenewFacebookConnectionHandler(
    IValidator<RenewFacebookConnectionCommand> validator,
    IClinicChannelConfigRepository repository,
    IFacebookTokenRenewalProvider provider,
    IUnitOfWork unitOfWork) : IRenewFacebookConnectionHandler
{
    public async Task<FacebookTokenRenewalResult> HandleAsync(
        RenewFacebookConnectionCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var config = await repository.GetByClinicAndChannelIncludingDisabledAsync(command.ClinicId, "Facebook", cancellationToken)
            ?? throw new InvalidOperationException("Facebook channel is not configured.");

        if (string.IsNullOrWhiteSpace(config.RefreshTokenOrLongLivedToken))
        {
            throw new InvalidOperationException("Facebook long-lived token is not configured.");
        }

        var result = await provider.RenewAsync(config.RefreshTokenOrLongLivedToken, cancellationToken);

        if (!result.IsSuccess)
        {
            config.ConnectionStatus = ChannelConnectionStatus.ReconnectRequired;
            config.LastError = result.ErrorMessage;
            config.IsEnabled = false;
        }
        else
        {
            config.ConnectionStatus = ChannelConnectionStatus.Connected;
            config.LastError = string.Empty;
            config.IsEnabled = true;
            config.AccessToken = string.IsNullOrWhiteSpace(result.AccessToken)
                ? config.AccessToken
                : result.AccessToken;
            config.RefreshTokenOrLongLivedToken = string.IsNullOrWhiteSpace(result.LongLivedToken)
                ? config.RefreshTokenOrLongLivedToken
                : result.LongLivedToken;

            if (result.TokenExpiresAtUtc.HasValue)
            {
                config.TokenExpiresAtUtc = result.TokenExpiresAtUtc.Value;
            }

            config.LastVerifiedAtUtc = DateTime.UtcNow;
        }

        config.UpdatedAtUtc = DateTime.UtcNow;

        await repository.UpsertAsync(config, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }
}
