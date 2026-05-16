using System.Net.Http.Headers;
using System.Text.Json;
using ClinicMateAI.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;

namespace ClinicMateAI.Infrastructure.Messaging;

public sealed class LineProfileProvider(HttpClient httpClient, ILogger<LineProfileProvider> logger) : ILineProfileProvider
{
    public async Task<string> GetDisplayNameAsync(string userId, string channelAccessToken, CancellationToken ct = default)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://api.line.me/v2/bot/profile/{userId}");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", channelAccessToken);

            var response = await httpClient.SendAsync(request, ct);
            if (!response.IsSuccessStatusCode)
                return userId; // fallback to userId if profile unavailable

            var body = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(body);
            return doc.RootElement.TryGetProperty("displayName", out var name)
                ? name.GetString() ?? userId
                : userId;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to get LINE profile for userId {UserId}", userId);
            return userId;
        }
    }
}
