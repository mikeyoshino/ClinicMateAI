using ClinicMateAI.Application.Abstractions.Messaging;

namespace ClinicMateAI.Application.Inbox;

public interface IGetInboxClinicsHandler : IQueryHandler<GetInboxClinicsQuery, IReadOnlyList<InboxClinicDto>>
{
}
