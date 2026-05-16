namespace ClinicMateAI.Application.Abstractions.Messaging;

public interface IFacebookTokenRenewalProvider
{
    Task<FacebookTokenRenewalResult> RenewAsync(
        string longLivedToken,
        CancellationToken cancellationToken = default);
}

public sealed record FacebookTokenRenewalResult(
    bool IsSuccess,
    string AccessToken,
    string LongLivedToken,
    DateTime? TokenExpiresAtUtc,
    string ErrorMessage);
