using System.Net.Http.Headers;
using System.Text.Json;
using ClinicMateAI.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;

namespace ClinicMateAI.Infrastructure.Messaging;

public sealed class LineChannelConnectionTester(HttpClient httpClient, ILogger<LineChannelConnectionTester> logger)
    : ILineChannelConnectionTester
{
    private const string BotInfoUrl = "https://api.line.me/v2/bot/info";

    public async Task<LineConnectionTestResult> TestAsync(
        string channelSecret,
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        _ = channelSecret; // LINE bot info validates the access token; webhook signature flow validates the secret.

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, BotInfoUrl);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return new LineConnectionTestResult(true, string.Empty);
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning("LINE bot info test failed {StatusCode}: {Body}", (int)response.StatusCode, body);
            return new LineConnectionTestResult(false, TryExtractErrorMessage(body) ?? "LINE connection test failed.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "LINE connection test threw an exception.");
            return new LineConnectionTestResult(false, ex.Message);
        }
    }

    private static string? TryExtractErrorMessage(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(body);
            return document.RootElement.TryGetProperty("message", out var message)
                ? message.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
