namespace ClinicMateAI.Domain.Promotions;

public sealed class Promotion
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RelatedServiceName { get; set; }
    public decimal? PromoPrice { get; set; }
    public DateOnly StartsOn { get; set; }
    public DateOnly EndsOn { get; set; }
    public string Conditions { get; set; } = string.Empty;
    public string ApprovedAiWording { get; set; } = string.Empty;
    public PromotionStatus Status { get; set; } = PromotionStatus.Draft;

    public bool IsAvailableToAi(DateOnly today)
    {
        return Status == PromotionStatus.Published
            && StartsOn <= today
            && EndsOn >= today;
    }
}
