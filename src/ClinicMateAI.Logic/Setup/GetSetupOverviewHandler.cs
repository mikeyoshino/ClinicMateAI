using ClinicMateAI.Application.Abstractions.Persistence;
using ClinicMateAI.Application.Setup;
using ClinicMateAI.Domain.Promotions;

namespace ClinicMateAI.Logic.Setup;

public sealed class GetSetupOverviewHandler(
    IClinicRepository clinicRepository,
    IPromotionRepository promotionRepository,
    IClinicServiceRepository clinicServiceRepository) : IGetSetupOverviewHandler
{
    public async Task<SetupOverviewDto?> HandleAsync(
        GetSetupOverviewQuery query,
        CancellationToken cancellationToken = default)
    {
        var clinic = await clinicRepository.GetByIdAsync(query.ClinicId, cancellationToken);
        if (clinic is null)
        {
            return null;
        }

        var promotions = await promotionRepository.ListByClinicAsync(query.ClinicId, cancellationToken);
        var services = await clinicServiceRepository.ListByClinicAsync(query.ClinicId, cancellationToken);
        var publishedCount = promotions.Count(x => x.Status == PromotionStatus.Published);
        var draftCount = promotions.Count(x => x.Status == PromotionStatus.Draft);
        var disabledCount = promotions.Count(x => x.Status == PromotionStatus.Disabled);

        var clinicProfileReady =
            !string.IsNullOrWhiteSpace(clinic.Name)
            && !string.IsNullOrWhiteSpace(clinic.Address)
            && !string.IsNullOrWhiteSpace(clinic.Phone);

        var steps = new List<SetupStepStatusDto>
        {
            new(
                "clinic-profile",
                "Clinic Profile",
                clinicProfileReady ? "Ready" : "Incomplete",
                clinicProfileReady ? "ข้อมูลคลินิกพร้อมใช้งาน" : "กรุณากรอกชื่อ ที่อยู่ และเบอร์โทรคลินิก"),
            new(
                "services",
                "Services",
                services.Count > 0 ? "Ready" : "Pending",
                services.Count > 0 ? $"Configured {services.Count} service(s)" : "Please add at least one service"),
            new(
                "promotions",
                "Promotions",
                publishedCount > 0 ? "Ready" : "NeedsPublish",
                $"Published {publishedCount}, Draft {draftCount}, Disabled {disabledCount}"),
            new(
                "doctors-availability",
                "Doctors & Availability",
                "Pending",
                "Doctor schedules are not configured yet"),
            new(
                "booking-rules",
                "Booking Rules",
                "Pending",
                "Booking rules are not configured yet"),
            new(
                "faq",
                "FAQ",
                "Pending",
                "Frequently asked questions are not configured yet"),
            new(
                "safety-rules",
                "Safety Rules",
                "Pending",
                "Safety red-flag policy is using default rules"),
            new(
                "ai-test",
                "Test AI",
                publishedCount > 0 ? "Ready" : "PendingData",
                publishedCount > 0 ? "พร้อมทดสอบ AI จากข้อมูลที่ Publish แล้ว" : "ต้องมีโปรโมชั่นที่ Publish อย่างน้อย 1 รายการ")
        };

        var completed = steps.Count(x => x.Status == "Ready");
        return new SetupOverviewDto(query.ClinicId, clinic.Name, clinic.Address, clinic.Phone, clinic.MapUrl, completed, steps.Count, steps);
    }
}
