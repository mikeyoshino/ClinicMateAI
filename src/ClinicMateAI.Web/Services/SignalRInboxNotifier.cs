using ClinicMateAI.Application.Abstractions.Messaging;
using ClinicMateAI.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ClinicMateAI.Web.Services;

public sealed class SignalRInboxNotifier(
    IHubContext<InboxHub> hubContext,
    ILogger<SignalRInboxNotifier> logger) : IInboxNotifier
{
    public async Task NotifyConversationUpdatedAsync(
        Guid clinicId,
        ConversationUpdatedEvent evt,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await hubContext.Clients
                .Group($"clinic-{clinicId}")
                .SendAsync("ConversationUpdated", evt, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "SignalR ConversationUpdated failed for clinic {ClinicId}", clinicId);
        }
    }

    public async Task NotifyNewMessageAsync(
        Guid clinicId,
        Guid conversationId,
        NewMessageEvent evt,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await hubContext.Clients
                .Group($"clinic-{clinicId}")
                .SendAsync($"NewMessage-{conversationId}", evt, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "SignalR NewMessage failed for conversation {ConversationId}", conversationId);
        }
    }

    public async Task NotifyConversationClaimedAsync(
        Guid clinicId,
        ConversationClaimedEvent evt,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await hubContext.Clients
                .Group($"clinic-{clinicId}")
                .SendAsync("ConversationClaimed", evt, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "SignalR ConversationClaimed failed for clinic {ClinicId}", clinicId);
        }
    }
}
