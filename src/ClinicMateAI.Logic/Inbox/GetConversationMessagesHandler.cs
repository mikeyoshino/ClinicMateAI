using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Inbox;

namespace ClinicMateAI.Logic.Inbox;

public sealed class GetConversationMessagesHandler(
    IConversationRepository conversationRepository,
    IMessageRepository messageRepository) : IGetConversationMessagesHandler
{
    public async Task<IReadOnlyList<InboxMessageDto>> HandleAsync(
        GetConversationMessagesQuery query,
        CancellationToken cancellationToken = default)
    {
        var conversation = await conversationRepository.GetByIdAsync(
            query.ClinicId,
            query.ConversationId,
            cancellationToken);

        if (conversation is null)
        {
            return [];
        }

        var messages = await messageRepository.ListByConversationAsync(
            query.ClinicId,
            query.ConversationId,
            cancellationToken);

        return messages
            .Select(x => new InboxMessageDto(
                x.Id,
                x.SenderType,
                x.Text,
                x.SentAtUtc))
            .ToList();
    }
}
