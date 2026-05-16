using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using ClinicMateAI.Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;

namespace ClinicMateAI.Infrastructure.Messaging;

public sealed class LineMessageSender(HttpClient httpClient, ILogger<LineMessageSender> logger) : ILineMessageSender
{
    private const string ReplyUrl = "https://api.line.me/v2/bot/message/reply";

    public async Task SendReplyAsync(string replyToken, string text, string channelAccessToken, CancellationToken ct = default)
    {
        var payload = new
        {
            replyToken,
            messages = new[] { new { type = "text", text } }
        };

        var json = JsonSerializer.Serialize(payload);
        using var request = new HttpRequestMessage(HttpMethod.Post, ReplyUrl)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", channelAccessToken);

        var response = await httpClient.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(ct);
            logger.LogWarning("LINE Reply API failed {Status}: {Body}", (int)response.StatusCode, body);
        }
    }
}
