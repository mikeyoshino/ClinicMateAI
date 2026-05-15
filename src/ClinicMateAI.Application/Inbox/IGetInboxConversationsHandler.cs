using ClinicMateAI.Application.Abstractions.Messaging;

namespace ClinicMateAI.Application.Inbox;

public interface IGetInboxConversationsHandler : IQueryHandler<GetInboxConversationsQuery, IReadOnlyList<InboxConversationDto>>
{
}
