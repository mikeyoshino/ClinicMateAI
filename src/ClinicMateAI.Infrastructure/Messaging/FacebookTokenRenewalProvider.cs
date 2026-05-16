using System.Net;
using System.Text.Json;
using ClinicMateAI.Application.Abstractions.Messaging;
using Microsoft.Extensions.Configuration;

namespace ClinicMateAI.Infrastructure.Messaging;

public sealed class FacebookTokenRenewalProvider(
    HttpClient httpClient,
    IConfiguration configuration) : IFacebookTokenRenewalProvider
{
    public async Task<FacebookTokenRenewalResult> RenewAsync(
        string longLivedToken,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(longLivedToken))
        {
            return new FacebookTokenRenewalResult(
                IsSuccess: false,
                AccessToken: string.Empty,
                LongLivedToken: string.Empty,
                TokenExpiresAtUtc: null,
                ErrorMessage: "Facebook long-lived token is required.");
        }

        var appId = configuration["Facebook:AppId"];
        var appSecret = configuration["Facebook:AppSecret"];

        if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(appSecret))
        {
            return new FacebookTokenRenewalResult(
                IsSuccess: true,
                AccessToken: longLivedToken,
                LongLivedToken: longLivedToken,
                TokenExpiresAtUtc: DateTime.UtcNow.AddDays(60),
                ErrorMessage: string.Empty);
        }

        var url = string.Join("&",
        [
            "https://graph.facebook.com/v19.0/oauth/access_token?grant_type=fb_exchange_token",
            $"client_id={WebUtility.UrlEncode(appId)}",
            $"client_secret={WebUtility.UrlEncode(appSecret)}",
            $"fb_exchange_token={WebUtility.UrlEncode(longLivedToken)}"
        ]);

        try
        {
            using var response = await httpClient.GetAsync(url, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return new FacebookTokenRenewalResult(
                    IsSuccess: false,
                    AccessToken: string.Empty,
                    LongLivedToken: string.Empty,
                    TokenExpiresAtUtc: null,
                    ErrorMessage: ParseFacebookError(body) ?? $"Facebook token renewal failed ({(int)response.StatusCode}).");
            }

            using var document = JsonDocument.Parse(body);
            if (!document.RootElement.TryGetProperty("access_token", out var accessTokenElement))
            {
                return new FacebookTokenRenewalResult(
                    IsSuccess: false,
                    AccessToken: string.Empty,
                    LongLivedToken: string.Empty,
                    TokenExpiresAtUtc: null,
                    ErrorMessage: "Facebook token renewal response was missing an access token.");
            }

            var accessToken = accessTokenElement.GetString() ?? string.Empty;
            var expiresAtUtc = document.RootElement.TryGetProperty("expires_in", out var expiresInElement)
                ? DateTime.UtcNow.AddSeconds(expiresInElement.GetInt32())
                : DateTime.UtcNow.AddDays(60);

            return new FacebookTokenRenewalResult(
                IsSuccess: !string.IsNullOrWhiteSpace(accessToken),
                AccessToken: accessToken,
                LongLivedToken: accessToken,
                TokenExpiresAtUtc: expiresAtUtc,
                ErrorMessage: string.Empty);
        }
        catch (Exception ex) when (ex is HttpRequestException or JsonException)
        {
            return new FacebookTokenRenewalResult(
                IsSuccess: false,
                AccessToken: string.Empty,
                LongLivedToken: string.Empty,
                TokenExpiresAtUtc: null,
                ErrorMessage: $"Facebook token renewal failed: {ex.Message}");
        }
    }

    private static string? ParseFacebookError(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("error", out var errorElement)
                && errorElement.TryGetProperty("message", out var messageElement))
            {
                return messageElement.GetString();
            }
        }
        catch (JsonException)
        {
            return body;
        }

        return body;
    }
}
