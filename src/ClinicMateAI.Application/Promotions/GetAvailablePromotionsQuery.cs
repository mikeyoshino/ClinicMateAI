namespace ClinicMateAI.Application.Promotions;

public sealed record GetAvailablePromotionsQuery(
    Guid ClinicId,
    DateOnly? Today = null);
