using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Inbox;

namespace ClinicMateAI.Logic.Inbox;

public sealed class GetInboxConversationsHandler(
    IConversationRepository conversationRepository) : IGetInboxConversationsHandler
{
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

        return conversations
            .Select(x => new InboxConversationDto(
                x.Id,
                x.Channel,
                x.ExternalConversationId,
                x.CustomerDisplayName,
                x.Status,
                x.LastMessageAtUtc))
            .ToList();
    }
}
