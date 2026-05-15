using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Domain.Messaging;
using ClinicMateAI.Domain.Promotions;
using ClinicMateAI.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Infrastructure.Data;

public static class DemoDataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Clinics.AnyAsync())
        {
            return;
        }

        var clinic = new Clinic
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            CreatedAtUtc = new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc),
            Status = ClinicStatus.Active,
            Name = "Demo Aesthetic Clinic",
            Address = "Bangkok",
            Phone = "02-000-0000",
            MapUrl = "https://maps.example/demo-clinic"
        };

        var conversation = new Conversation
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            ClinicId = clinic.Id,
            Channel = "LINE",
            CustomerDisplayName = "คุณมิน",
            Status = "Open",
            LastMessageAtUtc = new DateTime(2026, 5, 15, 3, 0, 0, DateTimeKind.Utc)
        };

        db.Clinics.Add(clinic);
        db.Services.Add(new ClinicService
        {
            ClinicId = clinic.Id,
            Name = "Botox Jaw",
            Category = "Injectables",
            StartingPrice = 2999,
            DurationMinutes = 30,
            RequiresDoctorAssessment = true,
            ApprovedAiWording = "โบท็อกกรามเริ่มต้นที่ 2,999 บาทค่ะคุณลูกค้า ราคาขึ้นอยู่กับยี่ห้อและจำนวนยูนิต แนะนำให้คุณหมอประเมินก่อนนะคะ"
        });
        db.Promotions.Add(new Promotion
        {
            ClinicId = clinic.Id,
            Name = "Botox Jaw New Customer",
            RelatedServiceName = "Botox Jaw",
            PromoPrice = 2999,
            StartsOn = new DateOnly(2026, 5, 1),
            EndsOn = new DateOnly(2026, 5, 31),
            Conditions = "เฉพาะลูกค้าใหม่ ต้องจองล่วงหน้า",
            ApprovedAiWording = "ตอนนี้มีโปรโบท็อกกรามสำหรับคุณลูกค้าใหม่ เริ่มต้น 2,999 บาทค่ะ",
            Status = PromotionStatus.Published
        });
        db.Promotions.Add(new Promotion
        {
            ClinicId = clinic.Id,
            Name = "Laser Bright Draft",
            RelatedServiceName = "Laser",
            PromoPrice = 1990,
            StartsOn = new DateOnly(2026, 6, 1),
            EndsOn = new DateOnly(2026, 6, 30),
            Conditions = "Draft promotion for review",
            ApprovedAiWording = "Draft wording",
            Status = PromotionStatus.Draft
        });
        db.Conversations.Add(conversation);
        db.Messages.Add(new Message
        {
            ClinicId = clinic.Id,
            ConversationId = conversation.Id,
            SenderType = "Customer",
            Text = "โบท็อกกรามราคาเท่าไรคะ",
            SentAtUtc = conversation.LastMessageAtUtc
        });

        await db.SaveChangesAsync();
    }
}
