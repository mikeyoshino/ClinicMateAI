namespace ClinicMateAI.Application.Abstractions.Messaging;

public interface IFacebookConnectionProvider
{
    string BuildAuthorizationUrl(Guid clinicId);

    Task<FacebookConnectionResult> CompleteAsync(
        Guid clinicId,
        string authorizationCode,
        CancellationToken cancellationToken = default);
}

public sealed record FacebookConnectionResult(
    string PageId,
    string PageName,
    string AccessToken,
    string LongLivedToken,
    DateTime TokenExpiresAtUtc);
