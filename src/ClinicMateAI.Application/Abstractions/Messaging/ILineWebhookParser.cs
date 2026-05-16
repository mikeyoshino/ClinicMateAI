namespace ClinicMateAI.Application.Abstractions.Messaging;

public sealed record LineWebhookPayload(string Destination, IReadOnlyList<LineWebhookEvent> Events);

public sealed record LineWebhookEvent(
    string Type,
    string? ReplyToken,
    LineEventSource Source,
    LineWebhookMessage? Message,
    long Timestamp);

public sealed record LineEventSource(string Type, string? UserId, string? GroupId, string? RoomId);

public sealed record LineWebhookMessage(string Id, string Type, string? Text);

public interface ILineWebhookParser
{
    LineWebhookPayload? Parse(byte[] body);
}
