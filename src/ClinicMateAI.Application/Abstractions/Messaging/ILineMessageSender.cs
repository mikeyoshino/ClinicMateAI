namespace ClinicMateAI.Application.Abstractions.Messaging;

public interface ILineMessageSender
{
    Task SendReplyAsync(string replyToken, string text, string channelAccessToken, CancellationToken ct = default);
}
