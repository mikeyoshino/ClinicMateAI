using ClinicMateAI.Application.Abstractions.Messaging;

namespace ClinicMateAI.Application.Promotions;

public interface IGetAvailablePromotionsHandler : IQueryHandler<GetAvailablePromotionsQuery, IReadOnlyList<AvailablePromotionDto>>
{
}
