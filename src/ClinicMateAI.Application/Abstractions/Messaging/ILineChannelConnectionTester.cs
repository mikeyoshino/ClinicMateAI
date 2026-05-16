namespace ClinicMateAI.Application.Abstractions.Messaging;

public interface ILineChannelConnectionTester
{
    Task<LineConnectionTestResult> TestAsync(
        string channelSecret,
        string accessToken,
        CancellationToken cancellationToken = default);
}

public sealed record LineConnectionTestResult(bool IsSuccess, string ErrorMessage);
