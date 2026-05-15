using ClinicMateAI.Domain.Promotions;
using FluentAssertions;

namespace ClinicMateAI.Tests.Promotions;

public class PromotionTests
{
    [Fact]
    public void IsAvailableToAi_ReturnsTrueForPublishedActivePromotion()
    {
        var promotion = new Promotion
        {
            Name = "Botox Jaw New Customer",
            Status = PromotionStatus.Published,
            StartsOn = new DateOnly(2026, 5, 1),
            EndsOn = new DateOnly(2026, 5, 31)
        };

        promotion.IsAvailableToAi(new DateOnly(2026, 5, 15)).Should().BeTrue();
    }

    [Theory]
    [InlineData(PromotionStatus.Draft)]
    [InlineData(PromotionStatus.Disabled)]
    public void IsAvailableToAi_ReturnsFalseForNonPublishedPromotions(PromotionStatus status)
    {
        var promotion = new Promotion
        {
            Name = "Botox Jaw New Customer",
            Status = status,
            StartsOn = new DateOnly(2026, 5, 1),
            EndsOn = new DateOnly(2026, 5, 31)
        };

        promotion.IsAvailableToAi(new DateOnly(2026, 5, 15)).Should().BeFalse();
    }

    [Fact]
    public void IsAvailableToAi_ReturnsFalseAfterEndDate()
    {
        var promotion = new Promotion
        {
            Name = "Botox Jaw New Customer",
            Status = PromotionStatus.Published,
            StartsOn = new DateOnly(2026, 5, 1),
            EndsOn = new DateOnly(2026, 5, 31)
        };

        promotion.IsAvailableToAi(new DateOnly(2026, 6, 1)).Should().BeFalse();
    }
}
