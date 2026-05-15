namespace ClinicMateAI.Application.Promotions;

public sealed record AvailablePromotionDto(
    Guid PromotionId,
    string Name,
    string? RelatedServiceName,
    decimal? PromoPrice,
    DateOnly StartsOn,
    DateOnly EndsOn,
    string Conditions);
