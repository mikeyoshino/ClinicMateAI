using ClinicMateAI.Application.Abstractions.Messaging;

namespace ClinicMateAI.Application.Setup;

public interface IGetIntegrationOverviewHandler : IQueryHandler<GetIntegrationOverviewQuery, IReadOnlyList<ClinicIntegrationChannelDto>>
{
}
