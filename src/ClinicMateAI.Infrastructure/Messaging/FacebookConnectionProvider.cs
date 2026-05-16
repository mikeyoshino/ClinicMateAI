using System.Net;
using System.Security.Cryptography;
using System.Text;
using ClinicMateAI.Application.Abstractions.Messaging;
using Microsoft.Extensions.Configuration;

namespace ClinicMateAI.Infrastructure.Messaging;

public sealed class FacebookConnectionProvider(IConfiguration configuration) : IFacebookConnectionProvider
{
    public string BuildAuthorizationUrl(Guid clinicId)
    {
        var appId = configuration["Facebook:AppId"] ?? "facebook-app-id-not-configured";
        var redirectUri = configuration["Facebook:RedirectUri"] ?? "https://localhost/facebook/oauth/callback";
        var scope = configuration["Facebook:Scope"] ?? "pages_show_list,pages_messaging";

        return string.Join("&",
        [
            "https://www.facebook.com/v19.0/dialog/oauth?response_type=code",
            $"client_id={WebUtility.UrlEncode(appId)}",
            $"redirect_uri={WebUtility.UrlEncode(redirectUri)}",
            $"scope={WebUtility.UrlEncode(scope)}",
            $"state={WebUtility.UrlEncode(clinicId.ToString())}"
        ]);
    }

    public Task<FacebookConnectionResult> CompleteAsync(
        Guid clinicId,
        string authorizationCode,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(authorizationCode))
        {
            throw new InvalidOperationException("Facebook authorization code is required.");
        }

        // Demo-mode completion is intentional for the MVP until real Facebook app credentials are wired.
        return Task.FromResult(CreateDemoResult(clinicId, authorizationCode));
    }

    private static FacebookConnectionResult CreateDemoResult(Guid clinicId, string authorizationCode)
    {
        var normalizedCode = authorizationCode.Trim();
        var source = $"{clinicId:N}:{normalizedCode}";
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source))).ToLowerInvariant();

        return new FacebookConnectionResult(
            PageId: $"demo-page-{hash[..16]}",
            PageName: "Demo Facebook Page",
            AccessToken: "demo-facebook-access-token",
            LongLivedToken: "demo-facebook-long-lived-token",
            TokenExpiresAtUtc: DateTime.UtcNow.AddDays(60));
    }
}
