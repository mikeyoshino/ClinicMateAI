using System.Text.Json;
using System.Text.Json.Serialization;
using ClinicMateAI.Application.Abstractions.Messaging;

namespace ClinicMateAI.Infrastructure.Messaging;

public sealed class LineWebhookParser : ILineWebhookParser
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public LineWebhookPayload? Parse(byte[] body)
    {
        try
        {
            var raw = JsonSerializer.Deserialize<LineRawPayload>(body, Options);
            if (raw is null) return null;

            var events = (raw.Events ?? [])
                .Select(e => new LineWebhookEvent(
                    Type: e.Type ?? string.Empty,
                    ReplyToken: e.ReplyToken,
                    Source: new LineEventSource(
                        Type: e.Source?.Type ?? string.Empty,
                        UserId: e.Source?.UserId,
                        GroupId: e.Source?.GroupId,
                        RoomId: e.Source?.RoomId),
                    Message: e.Message is null ? null : new LineWebhookMessage(
                        Id: e.Message.Id ?? string.Empty,
                        Type: e.Message.Type ?? string.Empty,
                        Text: e.Message.Text),
                    Timestamp: e.Timestamp))
                .ToList();

            return new LineWebhookPayload(raw.Destination ?? string.Empty, events);
        }
        catch
        {
            return null;
        }
    }

    // Internal raw DTOs — kept private, only used for deserialization
    private sealed class LineRawPayload
    {
        public string? Destination { get; set; }
        public List<LineRawEvent>? Events { get; set; }
    }

    private sealed class LineRawEvent
    {
        public string? Type { get; set; }
        public string? ReplyToken { get; set; }
        public LineRawSource? Source { get; set; }
        public LineRawMessage? Message { get; set; }
        public long Timestamp { get; set; }
    }

    private sealed class LineRawSource
    {
        public string? Type { get; set; }
        public string? UserId { get; set; }
        public string? GroupId { get; set; }
        public string? RoomId { get; set; }
    }

    private sealed class LineRawMessage
    {
        public string? Id { get; set; }
        public string? Type { get; set; }
        public string? Text { get; set; }
    }
}
