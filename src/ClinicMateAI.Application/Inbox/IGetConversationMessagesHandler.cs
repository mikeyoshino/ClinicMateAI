using ClinicMateAI.Application.Abstractions.Messaging;

namespace ClinicMateAI.Application.Inbox;

public interface IGetConversationMessagesHandler : IQueryHandler<GetConversationMessagesQuery, IReadOnlyList<InboxMessageDto>>
{
}
