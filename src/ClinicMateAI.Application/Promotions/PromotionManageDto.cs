using ClinicMateAI.Domain.Promotions;

namespace ClinicMateAI.Application.Promotions;

public sealed record PromotionManageDto(
    Guid PromotionId,
    string Name,
    string? RelatedServiceName,
    decimal? PromoPrice,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string Conditions,
    PromotionStatus Status);
