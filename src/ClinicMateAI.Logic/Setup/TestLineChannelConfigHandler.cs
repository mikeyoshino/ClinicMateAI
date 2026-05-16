using ClinicMateAI.Application.Abstractions.Messaging;
using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Setup;
using ClinicMateAI.Domain.Clinics;
using FluentValidation;

namespace ClinicMateAI.Logic.Setup;

public sealed class TestLineChannelConfigHandler(
    IValidator<TestLineChannelConfigCommand> validator,
    IClinicChannelConfigRepository repository,
    ILineChannelConnectionTester tester,
    IUnitOfWork unitOfWork) : ITestLineChannelConfigHandler
{
    public async Task<LineConnectionTestResult> HandleAsync(
        TestLineChannelConfigCommand command,
        CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(command, cancellationToken);

        var config = await repository.GetByClinicAndChannelAsync(command.ClinicId, "LINE", cancellationToken)
            ?? throw new InvalidOperationException("LINE channel is not configured.");

        var result = await tester.TestAsync(config.Secret, config.AccessToken, cancellationToken);
        config.ConnectionStatus = result.IsSuccess ? ChannelConnectionStatus.Connected : ChannelConnectionStatus.Error;
        config.LastVerifiedAtUtc = result.IsSuccess ? DateTime.UtcNow : null;
        config.LastError = result.ErrorMessage;
        config.UpdatedAtUtc = DateTime.UtcNow;

        await repository.UpsertAsync(config, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result;
    }
}
