using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Setup;
using ClinicMateAI.Domain.Clinics;
using FluentValidation;

namespace ClinicMateAI.Logic.Setup;

public sealed class SaveLineChannelConfigHandler(
    IValidator<SaveLineChannelConfigCommand> validator,
    IClinicChannelConfigRepository repository,
    IUnitOfWork unitOfWork) : ISaveLineChannelConfigHandler
{
    public async Task HandleAsync(SaveLineChannelConfigCommand command, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var config = await repository.GetByClinicBranchAndChannelAsync(command.ClinicId, command.BranchId, "LINE", cancellationToken)
            ?? new ClinicChannelConfig
            {
                ClinicId = command.ClinicId,
                BranchId = command.BranchId,
                Channel = "LINE",
                ExternalPageId = string.Empty
            };

        config.Secret = command.ChannelSecret;
        config.AccessToken = command.AccessToken;
        config.ConnectionStatus = ChannelConnectionStatus.PendingVerification;
        config.LastError = string.Empty;
        config.UpdatedAtUtc = DateTime.UtcNow;

        await repository.UpsertAsync(config, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
