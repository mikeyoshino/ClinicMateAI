namespace ClinicMateAI.Application.Promotions;

public sealed record GetAvailablePromotionsQuery(
    Guid ClinicId,
    Guid? BranchId = null,
    DateOnly? Today = null);
