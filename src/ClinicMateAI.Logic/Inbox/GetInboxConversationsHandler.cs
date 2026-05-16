using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Inbox;
using ClinicMateAI.Domain.Messaging;
using Microsoft.Extensions.Logging;

namespace ClinicMateAI.Logic.Inbox;

public sealed class GetInboxConversationsHandler(
    IConversationRepository conversationRepository,
    IMessageRepository messageRepository,
    ILogger<GetInboxConversationsHandler> logger) : IGetInboxConversationsHandler
{
    private static readonly TimeSpan ClaimTimeout = TimeSpan.FromMinutes(30);

    public async Task<IReadOnlyList<InboxConversationDto>> HandleAsync(
        GetInboxConversationsQuery query,
        CancellationToken cancellationToken = default)
    {
        var take = query.Take switch
        {
            < 1 => 50,
            > 200 => 200,
            _ => query.Take
        };

        var conversations = await conversationRepository.ListRecentAsync(
            query.ClinicId,
            take,
            cancellationToken);

        var results = new List<InboxConversationDto>(conversations.Count);
        var now = DateTime.UtcNow;

        foreach (var x in conversations)
        {
            // Auto-release stale claims in the response (no DB write in query handler)
            var isStale = x.ClaimedAt.HasValue && (now - x.ClaimedAt.Value) > ClaimTimeout;
            var effectiveAssignedStaff = isStale ? null : x.AssignedStaff;
            var effectiveStatus = isStale ? "Open" : x.Status;

            string preview = string.Empty;
            try
            {
                var lastMessage = await messageRepository.GetLastInboundAsync(x.ClinicId, x.Id, cancellationToken);
                preview = lastMessage?.Text is { Length: > 0 } t
                    ? (t.Length > 60 ? t[..60] + "…" : t)
                    : string.Empty;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "Failed to load preview for conversation {ConversationId}", x.Id);
            }

            results.Add(new InboxConversationDto(
                x.Id,
                x.Channel,
                x.ExternalConversationId,
                x.CustomerDisplayName,
                effectiveStatus,
                x.AiStatus,
                x.IsRead,
                x.UnreadCount,
                effectiveAssignedStaff,
                isStale ? null : x.ClaimedAt,
                x.LastMessageAtUtc,
                preview));
        }

        return results;
    }
}
