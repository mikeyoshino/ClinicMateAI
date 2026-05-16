namespace ClinicMateAI.Application.Abstractions.Messaging;

public interface ILineProfileProvider
{
    Task<string> GetDisplayNameAsync(string userId, string channelAccessToken, CancellationToken ct = default);
}
