# ClinicMateAI MVP Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the first working ClinicMateAI Blazor/.NET MVP for beauty clinics with setup UI, unified test inbox, LINE/Facebook-ready webhooks, AI safety decisions, booking availability, promotions, and admin-only package limits.

**Architecture:** Use a .NET solution with domain/application logic isolated from Blazor UI. The web project hosts Blazor interactive server pages and ASP.NET Core API/webhook endpoints; infrastructure implements EF Core PostgreSQL persistence through Npgsql, seeded demo data, and provider adapters. The MVP runs locally with simulated AI/messaging/calendar providers, while interfaces are shaped for real LINE, Facebook, Google Calendar, and OpenAI credentials.

**Tech Stack:** .NET 8 or newer, C#, Blazor Web App with interactive server rendering, ASP.NET Core Identity, Entity Framework Core with PostgreSQL/Npgsql, Docker Compose for the local PostgreSQL database only, xUnit, bUnit, FluentAssertions.

**UI Reference:** Implement the Blazor UI using `Docs/UIDesignIdea.html` as the required visual and interaction reference. Translate its app-first Thai dashboard, fixed sidebar, slate/teal palette, compact cards, three-column inbox, AI Test simulator, and promotions table into Blazor components and local CSS. Do not rely on Tailwind CDN, Google Fonts CDN, or external script CDNs in the production MVP.

---

## File Structure

Create this structure:

- `ClinicMateAI.sln` or `ClinicMateAI.slnx` - solution file.
- `docker-compose.yml` - local PostgreSQL database only; the app runs on the host with `dotnet run`.
- `src/ClinicMateAI.Domain/` - entities, enums, and pure domain services.
- `src/ClinicMateAI.Application/` - use cases, DTOs, service interfaces, AI reply orchestration, booking logic.
- `src/ClinicMateAI.Infrastructure/` - EF Core DbContext, repositories, seeded demo data, simulated providers, LINE/Facebook/Calendar adapter skeletons.
- `src/ClinicMateAI.Web/` - Blazor UI, Identity, API/webhook endpoints, dependency injection, app configuration.
- `tests/ClinicMateAI.Tests/` - domain and application unit tests.
- `tests/ClinicMateAI.Web.Tests/` - bUnit component tests and endpoint tests where practical.

Key files:

- `src/ClinicMateAI.Domain/Clinics/Clinic.cs` - clinic tenant root.
- `src/ClinicMateAI.Domain/Clinics/ClinicPackage.cs` - assigned package and usage counters.
- `src/ClinicMateAI.Domain/Services/ClinicService.cs` - clinic service catalog item.
- `src/ClinicMateAI.Domain/Promotions/Promotion.cs` - promotion rules and active/published checks.
- `src/ClinicMateAI.Domain/Appointments/DoctorAvailability.cs` - doctor schedule and service mapping.
- `src/ClinicMateAI.Domain/Messaging/Conversation.cs` - conversation and message records.
- `src/ClinicMateAI.Domain/Ai/AiSafetyDecision.cs` - auto-reply/draft/handoff decision result.
- `src/ClinicMateAI.Application/Ai/AiReceptionistOrchestrator.cs` - message-to-reply workflow.
- `src/ClinicMateAI.Application/Ai/RedFlagDetector.cs` - Thai red flag keyword detection.
- `src/ClinicMateAI.Application/Appointments/AvailabilityService.cs` - available slot calculation.
- `src/ClinicMateAI.Application/Packages/PackageLimitService.cs` - plan quotas and warnings.
- `src/ClinicMateAI.Infrastructure/Data/AppDbContext.cs` - EF Core database.
- `src/ClinicMateAI.Infrastructure/Data/DemoDataSeeder.cs` - seeded beauty clinic.
- `src/ClinicMateAI.Web/Components/Pages/Setup/*.razor` - setup wizard pages.
- `src/ClinicMateAI.Web/Components/Pages/Inbox.razor` - unified inbox/test inbox.
- `src/ClinicMateAI.Web/Components/Pages/Appointments.razor` - appointment calendar/list.
- `src/ClinicMateAI.Web/Components/Pages/PlatformAdmin/Clinics.razor` - admin-only package assignment.
- `src/ClinicMateAI.Web/Endpoints/WebhookEndpoints.cs` - LINE/Facebook webhook endpoints.
- `src/ClinicMateAI.Web/wwwroot/css/clinicmate-theme.css` - local CSS implementing the visual system from `Docs/UIDesignIdea.html`.
- `src/ClinicMateAI.Web/Components/Shared/AppIcon.razor` - local icon wrapper or simple lucide-style icon helper for consistent action/status icons.

## Milestone 1: Solution And Test Foundation

### Task 1: Create solution and projects

**Files:**
- Create: `ClinicMateAI.sln` or `ClinicMateAI.slnx`
- Create: `docker-compose.yml`
- Create: `src/ClinicMateAI.Domain/ClinicMateAI.Domain.csproj`
- Create: `src/ClinicMateAI.Application/ClinicMateAI.Application.csproj`
- Create: `src/ClinicMateAI.Infrastructure/ClinicMateAI.Infrastructure.csproj`
- Create: `src/ClinicMateAI.Web/ClinicMateAI.Web.csproj`
- Create: `tests/ClinicMateAI.Tests/ClinicMateAI.Tests.csproj`
- Create: `tests/ClinicMateAI.Web.Tests/ClinicMateAI.Web.Tests.csproj`

- [ ] **Step 1: Scaffold projects**

Run:

```powershell
dotnet new sln -n ClinicMateAI
dotnet new classlib -n ClinicMateAI.Domain -o src/ClinicMateAI.Domain
dotnet new classlib -n ClinicMateAI.Application -o src/ClinicMateAI.Application
dotnet new classlib -n ClinicMateAI.Infrastructure -o src/ClinicMateAI.Infrastructure
dotnet new blazor -n ClinicMateAI.Web -o src/ClinicMateAI.Web --interactivity Server --auth Individual
dotnet new xunit -n ClinicMateAI.Tests -o tests/ClinicMateAI.Tests
dotnet new xunit -n ClinicMateAI.Web.Tests -o tests/ClinicMateAI.Web.Tests
dotnet sln add src/ClinicMateAI.Domain src/ClinicMateAI.Application src/ClinicMateAI.Infrastructure src/ClinicMateAI.Web tests/ClinicMateAI.Tests tests/ClinicMateAI.Web.Tests
```

Expected: solution and project files are created.

- [ ] **Step 2: Add project references**

Run:

```powershell
dotnet add src/ClinicMateAI.Application reference src/ClinicMateAI.Domain
dotnet add src/ClinicMateAI.Infrastructure reference src/ClinicMateAI.Application
dotnet add src/ClinicMateAI.Infrastructure reference src/ClinicMateAI.Domain
dotnet add src/ClinicMateAI.Web reference src/ClinicMateAI.Application
dotnet add src/ClinicMateAI.Web reference src/ClinicMateAI.Infrastructure
dotnet add tests/ClinicMateAI.Tests reference src/ClinicMateAI.Domain
dotnet add tests/ClinicMateAI.Tests reference src/ClinicMateAI.Application
dotnet add tests/ClinicMateAI.Web.Tests reference src/ClinicMateAI.Web
```

Expected: project references are added without errors.

- [ ] **Step 3: Add test and data packages**

Run:

```powershell
dotnet add src/ClinicMateAI.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/ClinicMateAI.Infrastructure package Microsoft.EntityFrameworkCore.Design
dotnet add src/ClinicMateAI.Web package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add tests/ClinicMateAI.Tests package FluentAssertions
dotnet add tests/ClinicMateAI.Web.Tests package FluentAssertions
dotnet add tests/ClinicMateAI.Web.Tests package bunit
```

Expected: restore succeeds.

- [ ] **Step 3a: Add local PostgreSQL Docker Compose**

Create `docker-compose.yml`:

```yaml
services:
  postgres:
    image: postgres:16-alpine
    container_name: clinicmateai-postgres
    environment:
      POSTGRES_DB: clinicmateai_dev
      POSTGRES_USER: clinicmate
      POSTGRES_PASSWORD: clinicmate_dev
    ports:
      - "5432:5432"
    volumes:
      - clinicmateai-postgres-data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U clinicmate -d clinicmateai_dev"]
      interval: 10s
      timeout: 5s
      retries: 5

volumes:
  clinicmateai-postgres-data:
```

Expected: `docker compose up -d postgres` starts the local database. Do not containerize the Blazor app for local development.

- [ ] **Step 4: Verify clean build**

Run:

```powershell
dotnet build
```

Expected: build succeeds.

## Milestone 2: Domain Model And Rules

### Task 2: Implement package limits

**Files:**
- Create: `src/ClinicMateAI.Domain/Packages/PackageTier.cs`
- Create: `src/ClinicMateAI.Domain/Packages/PackageLimit.cs`
- Create: `src/ClinicMateAI.Application/Packages/PackageLimitService.cs`
- Test: `tests/ClinicMateAI.Tests/Packages/PackageLimitServiceTests.cs`

- [ ] **Step 1: Write failing package tests**

Create `tests/ClinicMateAI.Tests/Packages/PackageLimitServiceTests.cs`:

```csharp
using ClinicMateAI.Application.Packages;
using ClinicMateAI.Domain.Packages;
using FluentAssertions;

namespace ClinicMateAI.Tests.Packages;

public class PackageLimitServiceTests
{
    [Theory]
    [InlineData(PackageTier.Starter, 1000, 20, 1)]
    [InlineData(PackageTier.Growth, 3000, 50, 3)]
    [InlineData(PackageTier.ProClinic, 8000, int.MaxValue, 10)]
    public void GetLimits_ReturnsConfiguredQuota(PackageTier tier, int aiReplies, int services, int admins)
    {
        var limits = PackageLimitService.GetLimits(tier);

        limits.MonthlyAiReplies.Should().Be(aiReplies);
        limits.MaxServices.Should().Be(services);
        limits.MaxAdminSeats.Should().Be(admins);
    }

    [Fact]
    public void IsOverAiReplyQuota_ReturnsTrueWhenUsageExceedsLimit()
    {
        var result = PackageLimitService.IsOverAiReplyQuota(PackageTier.Starter, 1001);

        result.Should().BeTrue();
    }
}
```

- [ ] **Step 2: Run tests to verify failure**

Run:

```powershell
dotnet test tests/ClinicMateAI.Tests --filter PackageLimitServiceTests
```

Expected: fails because package types do not exist.

- [ ] **Step 3: Add package domain types**

Create `src/ClinicMateAI.Domain/Packages/PackageTier.cs`:

```csharp
namespace ClinicMateAI.Domain.Packages;

public enum PackageTier
{
    Starter = 1,
    Growth = 2,
    ProClinic = 3,
    Enterprise = 4
}
```

Create `src/ClinicMateAI.Domain/Packages/PackageLimit.cs`:

```csharp
namespace ClinicMateAI.Domain.Packages;

public sealed record PackageLimit(
    PackageTier Tier,
    int MonthlyAiReplies,
    int MaxServices,
    int MaxAdminSeats,
    int MaxChannels,
    int MaxBranches);
```

- [ ] **Step 4: Implement package limit service**

Create `src/ClinicMateAI.Application/Packages/PackageLimitService.cs`:

```csharp
using ClinicMateAI.Domain.Packages;

namespace ClinicMateAI.Application.Packages;

public static class PackageLimitService
{
    public static PackageLimit GetLimits(PackageTier tier) => tier switch
    {
        PackageTier.Starter => new PackageLimit(tier, 1000, 20, 1, 1, 1),
        PackageTier.Growth => new PackageLimit(tier, 3000, 50, 3, 2, 1),
        PackageTier.ProClinic => new PackageLimit(tier, 8000, int.MaxValue, 10, 3, int.MaxValue),
        PackageTier.Enterprise => new PackageLimit(tier, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue),
        _ => throw new ArgumentOutOfRangeException(nameof(tier), tier, "Unsupported package tier.")
    };

    public static bool IsOverAiReplyQuota(PackageTier tier, int monthlyAiRepliesUsed)
    {
        var limit = GetLimits(tier);
        return monthlyAiRepliesUsed > limit.MonthlyAiReplies;
    }
}
```

- [ ] **Step 5: Verify package tests pass**

Run:

```powershell
dotnet test tests/ClinicMateAI.Tests --filter PackageLimitServiceTests
```

Expected: tests pass.

### Task 3: Implement promotion active/published rules

**Files:**
- Create: `src/ClinicMateAI.Domain/Promotions/PromotionStatus.cs`
- Create: `src/ClinicMateAI.Domain/Promotions/Promotion.cs`
- Test: `tests/ClinicMateAI.Tests/Promotions/PromotionTests.cs`

- [ ] **Step 1: Write failing promotion tests**

Create `tests/ClinicMateAI.Tests/Promotions/PromotionTests.cs`:

```csharp
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
```

- [ ] **Step 2: Run test to verify failure**

Run:

```powershell
dotnet test tests/ClinicMateAI.Tests --filter PromotionTests
```

Expected: fails because promotion types do not exist.

- [ ] **Step 3: Implement promotion domain**

Create `src/ClinicMateAI.Domain/Promotions/PromotionStatus.cs`:

```csharp
namespace ClinicMateAI.Domain.Promotions;

public enum PromotionStatus
{
    Draft = 1,
    Published = 2,
    Disabled = 3
}
```

Create `src/ClinicMateAI.Domain/Promotions/Promotion.cs`:

```csharp
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
```

- [ ] **Step 4: Verify promotion tests pass**

Run:

```powershell
dotnet test tests/ClinicMateAI.Tests --filter PromotionTests
```

Expected: tests pass.

### Task 4: Implement AI safety and red flag decision logic

**Files:**
- Create: `src/ClinicMateAI.Domain/Ai/AiReplyMode.cs`
- Create: `src/ClinicMateAI.Domain/Ai/AiSafetyDecision.cs`
- Create: `src/ClinicMateAI.Application/Ai/RedFlagDetector.cs`
- Create: `src/ClinicMateAI.Application/Ai/AiSafetyDecider.cs`
- Test: `tests/ClinicMateAI.Tests/Ai/AiSafetyDeciderTests.cs`

- [ ] **Step 1: Write failing safety tests**

Create `tests/ClinicMateAI.Tests/Ai/AiSafetyDeciderTests.cs`:

```csharp
using ClinicMateAI.Application.Ai;
using ClinicMateAI.Domain.Ai;
using FluentAssertions;

namespace ClinicMateAI.Tests.Ai;

public class AiSafetyDeciderTests
{
    [Fact]
    public void Decide_ReturnsEscalateForThaiRedFlag()
    {
        var decision = AiSafetyDecider.Decide("ฉีดแล้วบวมมากและปวดมากค่ะ", hasApprovedData: true, confidence: 0.95m);

        decision.Mode.Should().Be(AiReplyMode.Escalate);
        decision.Reason.Should().Contain("red flag");
    }

    [Fact]
    public void Decide_ReturnsDraftWhenClinicDataIsMissing()
    {
        var decision = AiSafetyDecider.Decide("โบท็อกกรามเท่าไรคะ", hasApprovedData: false, confidence: 0.95m);

        decision.Mode.Should().Be(AiReplyMode.DraftForStaff);
    }

    [Fact]
    public void Decide_ReturnsDraftWhenConfidenceIsLow()
    {
        var decision = AiSafetyDecider.Decide("ราคาแพ็กเกจพิเศษเท่าไร", hasApprovedData: true, confidence: 0.45m);

        decision.Mode.Should().Be(AiReplyMode.DraftForStaff);
    }

    [Fact]
    public void Decide_ReturnsAutoReplyForSafeApprovedHighConfidenceMessage()
    {
        var decision = AiSafetyDecider.Decide("โบท็อกกรามเท่าไรคะ", hasApprovedData: true, confidence: 0.90m);

        decision.Mode.Should().Be(AiReplyMode.AutoReply);
    }
}
```

- [ ] **Step 2: Run test to verify failure**

Run:

```powershell
dotnet test tests/ClinicMateAI.Tests --filter AiSafetyDeciderTests
```

Expected: fails because AI safety types do not exist.

- [ ] **Step 3: Add AI decision domain**

Create `src/ClinicMateAI.Domain/Ai/AiReplyMode.cs`:

```csharp
namespace ClinicMateAI.Domain.Ai;

public enum AiReplyMode
{
    AutoReply = 1,
    DraftForStaff = 2,
    Escalate = 3
}
```

Create `src/ClinicMateAI.Domain/Ai/AiSafetyDecision.cs`:

```csharp
namespace ClinicMateAI.Domain.Ai;

public sealed record AiSafetyDecision(AiReplyMode Mode, string Reason);
```

- [ ] **Step 4: Implement red flag detector and decider**

Create `src/ClinicMateAI.Application/Ai/RedFlagDetector.cs`:

```csharp
namespace ClinicMateAI.Application.Ai;

public static class RedFlagDetector
{
    private static readonly string[] Keywords =
    [
        "แพ้", "หายใจไม่ออก", "บวมมาก", "ปวดมาก", "มีไข้", "หนอง",
        "ติดเชื้อ", "หน้าชา", "ตามัว", "เลือดออก", "ฟิลเลอร์ไหล",
        "ฉีดแล้วเป็นก้อน", "ขอคืนเงิน", "ร้องเรียน"
    ];

    public static bool ContainsRedFlag(string message)
    {
        return Keywords.Any(keyword => message.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }
}
```

Create `src/ClinicMateAI.Application/Ai/AiSafetyDecider.cs`:

```csharp
using ClinicMateAI.Domain.Ai;

namespace ClinicMateAI.Application.Ai;

public static class AiSafetyDecider
{
    public static AiSafetyDecision Decide(string customerMessage, bool hasApprovedData, decimal confidence)
    {
        if (RedFlagDetector.ContainsRedFlag(customerMessage))
        {
            return new AiSafetyDecision(AiReplyMode.Escalate, "Message contains a medical or service red flag.");
        }

        if (!hasApprovedData)
        {
            return new AiSafetyDecision(AiReplyMode.DraftForStaff, "No approved clinic data found.");
        }

        if (confidence < 0.70m)
        {
            return new AiSafetyDecision(AiReplyMode.DraftForStaff, "AI confidence is below automatic reply threshold.");
        }

        return new AiSafetyDecision(AiReplyMode.AutoReply, "Approved data and high confidence.");
    }
}
```

- [ ] **Step 5: Verify safety tests pass**

Run:

```powershell
dotnet test tests/ClinicMateAI.Tests --filter AiSafetyDeciderTests
```

Expected: tests pass.

### Task 5: Implement appointment availability calculation

**Files:**
- Create: `src/ClinicMateAI.Domain/Appointments/TimeRange.cs`
- Create: `src/ClinicMateAI.Domain/Appointments/DoctorAvailability.cs`
- Create: `src/ClinicMateAI.Application/Appointments/AvailabilityService.cs`
- Test: `tests/ClinicMateAI.Tests/Appointments/AvailabilityServiceTests.cs`

- [ ] **Step 1: Write failing availability tests**

Create `tests/ClinicMateAI.Tests/Appointments/AvailabilityServiceTests.cs`:

```csharp
using ClinicMateAI.Application.Appointments;
using ClinicMateAI.Domain.Appointments;
using FluentAssertions;

namespace ClinicMateAI.Tests.Appointments;

public class AvailabilityServiceTests
{
    [Fact]
    public void GetAvailableSlots_RemovesBusyCalendarSlots()
    {
        var availability = new DoctorAvailability
        {
            DoctorId = Guid.NewGuid(),
            DayOfWeek = DayOfWeek.Saturday,
            StartsAt = new TimeOnly(13, 0),
            EndsAt = new TimeOnly(16, 0),
            SlotMinutes = 60
        };
        var date = new DateOnly(2026, 5, 16);
        var busy = new[]
        {
            new TimeRange(new DateTime(2026, 5, 16, 14, 0, 0), new DateTime(2026, 5, 16, 15, 0, 0))
        };

        var slots = AvailabilityService.GetAvailableSlots(date, availability, busy);

        slots.Should().Equal(
            new DateTime(2026, 5, 16, 13, 0, 0),
            new DateTime(2026, 5, 16, 15, 0, 0));
    }
}
```

- [ ] **Step 2: Run test to verify failure**

Run:

```powershell
dotnet test tests/ClinicMateAI.Tests --filter AvailabilityServiceTests
```

Expected: fails because appointment types do not exist.

- [ ] **Step 3: Add appointment domain**

Create `src/ClinicMateAI.Domain/Appointments/TimeRange.cs`:

```csharp
namespace ClinicMateAI.Domain.Appointments;

public sealed record TimeRange(DateTime StartsAt, DateTime EndsAt)
{
    public bool Overlaps(DateTime startsAt, DateTime endsAt)
    {
        return StartsAt < endsAt && startsAt < EndsAt;
    }
}
```

Create `src/ClinicMateAI.Domain/Appointments/DoctorAvailability.cs`:

```csharp
namespace ClinicMateAI.Domain.Appointments;

public sealed class DoctorAvailability
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DoctorId { get; set; }
    public Guid? BranchId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartsAt { get; set; }
    public TimeOnly EndsAt { get; set; }
    public int SlotMinutes { get; set; } = 30;
}
```

- [ ] **Step 4: Implement availability service**

Create `src/ClinicMateAI.Application/Appointments/AvailabilityService.cs`:

```csharp
using ClinicMateAI.Domain.Appointments;

namespace ClinicMateAI.Application.Appointments;

public static class AvailabilityService
{
    public static IReadOnlyList<DateTime> GetAvailableSlots(
        DateOnly date,
        DoctorAvailability availability,
        IEnumerable<TimeRange> busyRanges)
    {
        if (date.DayOfWeek != availability.DayOfWeek)
        {
            return [];
        }

        var slots = new List<DateTime>();
        var current = date.ToDateTime(availability.StartsAt);
        var end = date.ToDateTime(availability.EndsAt);
        var step = TimeSpan.FromMinutes(availability.SlotMinutes);

        while (current + step <= end)
        {
            var slotEnd = current + step;
            if (!busyRanges.Any(range => range.Overlaps(current, slotEnd)))
            {
                slots.Add(current);
            }

            current += step;
        }

        return slots;
    }
}
```

- [ ] **Step 5: Verify availability tests pass**

Run:

```powershell
dotnet test tests/ClinicMateAI.Tests --filter AvailabilityServiceTests
```

Expected: tests pass.

## Milestone 3: Persistence And Seed Data

### Task 6: Add EF Core entities and demo clinic seed

**Files:**
- Create: `src/ClinicMateAI.Domain/Clinics/Clinic.cs`
- Create: `src/ClinicMateAI.Domain/Services/ClinicService.cs`
- Create: `src/ClinicMateAI.Domain/Messaging/Conversation.cs`
- Create: `src/ClinicMateAI.Domain/Messaging/Message.cs`
- Create: `src/ClinicMateAI.Infrastructure/Data/AppDbContext.cs`
- Create: `src/ClinicMateAI.Infrastructure/Data/DemoDataSeeder.cs`
- Modify: `src/ClinicMateAI.Web/Program.cs`
- Test: `tests/ClinicMateAI.Tests/Data/DemoDataSeederTests.cs`

- [ ] **Step 1: Write failing seed test**

Create `tests/ClinicMateAI.Tests/Data/DemoDataSeederTests.cs`:

```csharp
using ClinicMateAI.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Tests.Data;

public class DemoDataSeederTests
{
    [Fact]
    public async Task SeedAsync_CreatesBeautyClinicWithPublishedPromotion()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        await using var db = new AppDbContext(options);
        await db.Database.EnsureCreatedAsync();

        await DemoDataSeeder.SeedAsync(db);

        db.Clinics.Should().ContainSingle(c => c.Name == "Demo Aesthetic Clinic");
        db.Services.Should().Contain(s => s.Name == "Botox Jaw");
        db.Promotions.Should().Contain(p => p.Name == "Botox Jaw New Customer");
    }
}
```

- [ ] **Step 2: Run test to verify failure**

Run:

```powershell
dotnet test tests/ClinicMateAI.Tests --filter DemoDataSeederTests
```

Expected: fails because infrastructure data types do not exist.

- [ ] **Step 3: Add entity classes**

Create focused entity classes with these required properties:

```csharp
// src/ClinicMateAI.Domain/Clinics/Clinic.cs
namespace ClinicMateAI.Domain.Clinics;

public sealed class Clinic
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string MapUrl { get; set; } = string.Empty;
}
```

```csharp
// src/ClinicMateAI.Domain/Services/ClinicService.cs
namespace ClinicMateAI.Domain.Services;

public sealed class ClinicService
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal StartingPrice { get; set; }
    public int DurationMinutes { get; set; }
    public bool RequiresDoctorAssessment { get; set; }
    public string ApprovedAiWording { get; set; } = string.Empty;
}
```

```csharp
// src/ClinicMateAI.Domain/Messaging/Conversation.cs
namespace ClinicMateAI.Domain.Messaging;

public sealed class Conversation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClinicId { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string CustomerDisplayName { get; set; } = string.Empty;
    public string Status { get; set; } = "Open";
    public DateTime LastMessageAtUtc { get; set; } = DateTime.UtcNow;
}
```

```csharp
// src/ClinicMateAI.Domain/Messaging/Message.cs
namespace ClinicMateAI.Domain.Messaging;

public sealed class Message
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ConversationId { get; set; }
    public string SenderType { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
}
```

- [ ] **Step 4: Add DbContext**

Create `src/ClinicMateAI.Infrastructure/Data/AppDbContext.cs`:

```csharp
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Domain.Messaging;
using ClinicMateAI.Domain.Promotions;
using ClinicMateAI.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace ClinicMateAI.Infrastructure.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<ClinicService> Services => Set<ClinicService>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Clinic>().HasKey(x => x.Id);
        modelBuilder.Entity<ClinicService>().HasKey(x => x.Id);
        modelBuilder.Entity<Promotion>().HasKey(x => x.Id);
        modelBuilder.Entity<Conversation>().HasKey(x => x.Id);
        modelBuilder.Entity<Message>().HasKey(x => x.Id);
        modelBuilder.Entity<ClinicService>().Property(x => x.StartingPrice).HasColumnType("decimal(18,2)");
        modelBuilder.Entity<Promotion>().Property(x => x.PromoPrice).HasColumnType("decimal(18,2)");
    }
}
```

- [ ] **Step 5: Add demo seed**

Create `src/ClinicMateAI.Infrastructure/Data/DemoDataSeeder.cs`:

```csharp
using ClinicMateAI.Domain.Clinics;
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
            Name = "Demo Aesthetic Clinic",
            Address = "Bangkok",
            Phone = "02-000-0000",
            MapUrl = "https://maps.example/demo-clinic"
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

        await db.SaveChangesAsync();
    }
}
```

- [ ] **Step 6: Register DbContext and seeding**

In `src/ClinicMateAI.Web/Program.cs`, register PostgreSQL through Npgsql and seed on startup:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Host=localhost;Port=5432;Database=clinicmateai_dev;Username=clinicmate;Password=clinicmate_dev"));
```

After `var app = builder.Build();`, add:

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DemoDataSeeder.SeedAsync(db);
}
```

Add required usings:

```csharp
using ClinicMateAI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
```

- [ ] **Step 7: Verify seed test passes**

Run:

```powershell
dotnet test tests/ClinicMateAI.Tests --filter DemoDataSeederTests
```

Expected: tests pass.

## Milestone 4: AI Orchestration And Messaging

### Task 7: Build AI receptionist orchestrator with simulated reply provider

**Files:**
- Create: `src/ClinicMateAI.Application/Ai/AiReplyRequest.cs`
- Create: `src/ClinicMateAI.Application/Ai/AiReplyResult.cs`
- Create: `src/ClinicMateAI.Application/Ai/IAiReplyProvider.cs`
- Create: `src/ClinicMateAI.Application/Ai/SimulatedAiReplyProvider.cs`
- Create: `src/ClinicMateAI.Application/Ai/AiReceptionistOrchestrator.cs`
- Test: `tests/ClinicMateAI.Tests/Ai/AiReceptionistOrchestratorTests.cs`

- [ ] **Step 1: Write failing orchestrator tests**

Create tests proving:

```csharp
[Fact]
public async Task GenerateReplyAsync_UsesServiceMindedThaiToneForSafeMessage()
{
    var orchestrator = new AiReceptionistOrchestrator(new SimulatedAiReplyProvider());

    var result = await orchestrator.GenerateReplyAsync(new AiReplyRequest(
        "โบท็อกกรามเท่าไรคะ",
        HasApprovedData: true,
        Confidence: 0.90m,
        ApprovedClinicFacts: "โบท็อกกรามเริ่มต้นที่ 2,999 บาท"));

    result.Mode.Should().Be(AiReplyMode.AutoReply);
    result.ReplyText.Should().Contain("คุณลูกค้า");
    result.ReplyText.Should().Contain("ค่ะ");
}
```

- [ ] **Step 2: Implement request/result/provider/orchestrator**

Use `AiSafetyDecider.Decide(...)` first, then call provider only for auto-reply or draft modes. For red flag mode, return the fixed escalation text from the spec.

- [ ] **Step 3: Verify orchestrator tests pass**

Run:

```powershell
dotnet test tests/ClinicMateAI.Tests --filter AiReceptionistOrchestratorTests
```

Expected: tests pass.

### Task 8: Add webhook endpoint skeletons and test inbox receive flow

**Files:**
- Create: `src/ClinicMateAI.Web/Endpoints/WebhookEndpoints.cs`
- Create: `src/ClinicMateAI.Application/Messaging/ReceiveMessageCommand.cs`
- Create: `src/ClinicMateAI.Application/Messaging/ReceiveMessageHandler.cs`
- Modify: `src/ClinicMateAI.Web/Program.cs`
- Test: `tests/ClinicMateAI.Web.Tests/Webhooks/WebhookEndpointTests.cs`

- [ ] **Step 1: Write endpoint tests**

Test POST `/webhooks/line` and `/webhooks/facebook` with a simple JSON body:

```json
{ "clinicId": "11111111-1111-1111-1111-111111111111", "customerName": "คุณลูกค้า Demo", "text": "โบท็อกกรามเท่าไรคะ" }
```

Expected response:

```json
{ "received": true }
```

- [ ] **Step 2: Implement message receive handler**

Handler responsibilities:

- Find or create conversation.
- Store incoming customer message.
- Call AI orchestrator.
- Store AI message or staff draft record.
- Increment AI usage when an AI reply is generated.

- [ ] **Step 3: Map endpoints**

In `WebhookEndpoints.MapWebhookEndpoints(app)`, map:

```csharp
app.MapPost("/webhooks/line", async (ReceiveMessageCommand command, ReceiveMessageHandler handler) =>
    Results.Ok(await handler.HandleAsync(command with { Channel = "LINE" })));

app.MapPost("/webhooks/facebook", async (ReceiveMessageCommand command, ReceiveMessageHandler handler) =>
    Results.Ok(await handler.HandleAsync(command with { Channel = "Facebook" })));
```

- [ ] **Step 4: Verify endpoint tests pass**

Run:

```powershell
dotnet test tests/ClinicMateAI.Web.Tests --filter WebhookEndpointTests
```

Expected: tests pass.

## Milestone 5: Blazor MVP UI

### Task 9: Build shell navigation and dashboard

**Files:**
- Modify: `src/ClinicMateAI.Web/Components/Layout/NavMenu.razor`
- Create: `src/ClinicMateAI.Web/Components/Pages/Dashboard.razor`
- Create: `src/ClinicMateAI.Web/Components/Shared/MetricTile.razor`
- Create: `src/ClinicMateAI.Web/wwwroot/css/clinicmate-theme.css`
- Test: `tests/ClinicMateAI.Web.Tests/Components/DashboardTests.cs`

- [ ] **Step 1: Add bUnit test for dashboard metrics**

Test that dashboard renders labels:

- New customers today.
- AI replied.
- Handoff.
- Booked appointments.
- Follow-up needed.

- [ ] **Step 2: Implement navigation**

Navigation items:

- Dashboard
- Inbox
- Appointments
- Setup
- Services
- Promotions
- AI Settings
- Integrations
- Platform Admin

Use `Docs/UIDesignIdea.html` as the visual reference:

- fixed white sidebar with clinic logo block,
- Thai labels with English hints where useful,
- teal active state,
- badge count for Inbox,
- top header with current page title and package usage chip.

- [ ] **Step 3: Implement dashboard**

Use demo counts from seeded data or a simple dashboard service. Match the design reference with compact metric tiles for new customers, AI auto replies, escalations, and booked appointments, plus a recent activity list with rose/green/teal status indicators.

- [ ] **Step 4: Add local theme CSS**

Create `src/ClinicMateAI.Web/wwwroot/css/clinicmate-theme.css` with local styles that replace the design prototype's Tailwind CDN usage:

```css
:root {
    --cm-bg: #f8fafc;
    --cm-surface: #ffffff;
    --cm-border: #e2e8f0;
    --cm-text: #1e293b;
    --cm-muted: #64748b;
    --cm-teal: #0d9488;
    --cm-teal-dark: #0f766e;
    --cm-rose: #f43f5e;
    --cm-amber: #d97706;
    --cm-green: #16a34a;
}

body {
    font-family: "Prompt", "Segoe UI", Tahoma, sans-serif;
    background: var(--cm-bg);
    color: var(--cm-text);
}

.cm-shell {
    min-height: 100vh;
    display: flex;
    background: var(--cm-bg);
}

.cm-sidebar {
    width: 16rem;
    background: var(--cm-surface);
    border-right: 1px solid var(--cm-border);
    flex-shrink: 0;
}

.cm-card {
    background: var(--cm-surface);
    border: 1px solid var(--cm-border);
    border-radius: 0.75rem;
    box-shadow: 0 1px 2px rgba(15, 23, 42, 0.04);
}

.cm-badge {
    display: inline-flex;
    align-items: center;
    border-radius: 999px;
    padding: 0.125rem 0.625rem;
    font-size: 0.75rem;
    font-weight: 600;
}
```

- [ ] **Step 5: Verify dashboard component tests pass**

Run:

```powershell
dotnet test tests/ClinicMateAI.Web.Tests --filter DashboardTests
```

Expected: tests pass.

### Task 10: Build clinic setup and promotions screens

**Files:**
- Create: `src/ClinicMateAI.Web/Components/Pages/Setup/SetupWizard.razor`
- Create: `src/ClinicMateAI.Web/Components/Pages/Services.razor`
- Create: `src/ClinicMateAI.Web/Components/Pages/Promotions.razor`
- Create: `src/ClinicMateAI.Application/Promotions/PromotionService.cs`
- Test: `tests/ClinicMateAI.Tests/Promotions/PromotionServiceTests.cs`

- [ ] **Step 1: Test promotion service only returns AI-available promotions**

Write a test with one published active promo, one expired promo, and one draft promo. Assert only the published active promo is returned.

- [ ] **Step 2: Implement promotion service**

Expose:

```csharp
Task<IReadOnlyList<Promotion>> GetAvailablePromotionsForAiAsync(Guid clinicId, DateOnly today);
Task<Promotion> SaveDraftAsync(Promotion promotion);
Task PublishAsync(Guid promotionId);
Task DisableAsync(Guid promotionId);
```

- [ ] **Step 3: Build `Promotions.razor`**

UI fields:

- Name
- Related service
- Promo price
- Start date
- End date
- Conditions
- Approved AI wording
- Status

Buttons:

- Save Draft
- Publish
- Disable

- [ ] **Step 4: Build setup wizard landing page**

Show setup progress cards:

- Clinic Profile
- Services
- Promotions
- Doctors & Availability
- Booking Rules
- FAQ
- Safety Rules
- Test AI

- [ ] **Step 5: Verify promotion tests pass**

Run:

```powershell
dotnet test tests/ClinicMateAI.Tests --filter PromotionServiceTests
```

Expected: tests pass.

### Task 11: Build inbox and AI test screen

**Files:**
- Create: `src/ClinicMateAI.Web/Components/Pages/Inbox.razor`
- Create: `src/ClinicMateAI.Web/Components/Pages/AiTest.razor`
- Create: `src/ClinicMateAI.Web/Components/Shared/ConversationList.razor`
- Create: `src/ClinicMateAI.Web/Components/Shared/ConversationThread.razor`
- Test: `tests/ClinicMateAI.Web.Tests/Components/InboxTests.cs`

- [ ] **Step 1: Add bUnit test for inbox**

Assert inbox renders:

- Channel badge.
- Customer name.
- Latest customer message.
- AI reply or handoff state.
- Approve draft button when mode is `DraftForStaff`.

- [ ] **Step 2: Implement inbox UI**

Use a three-column operational layout:

- Conversation list.
- Thread.
- Customer and safety panel.

Match `Docs/UIDesignIdea.html`:

- left column search and LINE/Facebook conversation cards,
- center chat thread with customer bubbles and AI/staff status labels,
- right safety panel showing escalation reason, customer info, and action buttons,
- rose status for red flag/handoff, teal for AI replied, amber for draft.

- [ ] **Step 3: Implement AI test UI**

The page accepts a test customer message and displays:

- Safety mode.
- AI reply text.
- Data source used.
- Whether it would send automatically.

Match the design reference with:

- phone-style customer simulator,
- AI Safety Decision Engine panel,
- confidence score,
- reason text,
- red flag keyword display.

- [ ] **Step 4: Verify inbox tests pass**

Run:

```powershell
dotnet test tests/ClinicMateAI.Web.Tests --filter InboxTests
```

Expected: tests pass.

### Task 12: Build appointments and doctor availability screens

**Files:**
- Create: `src/ClinicMateAI.Web/Components/Pages/Appointments.razor`
- Create: `src/ClinicMateAI.Web/Components/Pages/Doctors.razor`
- Create: `src/ClinicMateAI.Application/Appointments/AppointmentService.cs`
- Test: `tests/ClinicMateAI.Tests/Appointments/AppointmentServiceTests.cs`

- [ ] **Step 1: Test appointment creation from an available slot**

Assert service creates appointment only when the requested time is in `AvailabilityService.GetAvailableSlots(...)`.

- [ ] **Step 2: Implement appointment service**

Expose:

```csharp
Task<IReadOnlyList<DateTime>> GetAvailableSlotsAsync(Guid doctorId, DateOnly date, Guid serviceId);
Task<Guid> CreateAppointmentAsync(Guid customerId, Guid serviceId, Guid doctorId, DateTime startsAt);
Task RescheduleAppointmentAsync(Guid appointmentId, DateTime newStartsAt);
Task CancelAppointmentAsync(Guid appointmentId);
```

- [ ] **Step 3: Implement appointments UI**

Show:

- Today’s appointments.
- Service.
- Doctor.
- Channel/customer.
- Status.
- Deposit status.
- Reminder status.

- [ ] **Step 4: Implement doctors UI**

Show doctor list and working-hour editor with service assignment.

- [ ] **Step 5: Verify appointment tests pass**

Run:

```powershell
dotnet test tests/ClinicMateAI.Tests --filter AppointmentServiceTests
```

Expected: tests pass.

## Milestone 6: Admin, Integrations, And End-To-End Verification

### Task 13: Build platform admin package assignment

**Files:**
- Create: `src/ClinicMateAI.Web/Components/Pages/PlatformAdmin/Clinics.razor`
- Create: `src/ClinicMateAI.Application/Clinics/ClinicAdminService.cs`
- Test: `tests/ClinicMateAI.Tests/Packages/PackageAssignmentTests.cs`

- [ ] **Step 1: Test admin package assignment**

Assert assigning `Growth` updates clinic package and changes AI reply quota to 3,000.

- [ ] **Step 2: Implement admin service**

Expose:

```csharp
Task AssignPackageAsync(Guid clinicId, PackageTier tier);
Task<PackageLimit> GetCurrentLimitsAsync(Guid clinicId);
```

- [ ] **Step 3: Build admin-only UI**

Show:

- Clinic.
- Current package.
- Monthly AI usage.
- Service count.
- Admin seat count.
- Warning when quota is exceeded.

- [ ] **Step 4: Protect route**

Use role authorization so only Platform Admin can access the page.

### Task 14: Build integration status screens

**Files:**
- Create: `src/ClinicMateAI.Web/Components/Pages/Integrations.razor`
- Create: `src/ClinicMateAI.Application/Integrations/IntegrationSettingsService.cs`
- Create: `src/ClinicMateAI.Domain/Integrations/IntegrationConnection.cs`

- [ ] **Step 1: Add settings model**

Connection fields:

- ClinicId
- Provider: LINE, Facebook, GoogleCalendar
- IsConnected
- DisplayName
- LastCheckedAtUtc
- MaskedCredentialLabel

- [ ] **Step 2: Build integrations UI**

Show connection cards:

- LINE OA: Not connected / connected.
- Facebook Messenger: Not connected / connected.
- Google Calendar: Not connected / connected.

Credential input is Platform Admin only.

- [ ] **Step 3: Verify build**

Run:

```powershell
dotnet build
```

Expected: build succeeds.

### Task 15: End-to-end local verification

**Files:**
- Modify as needed from previous tasks only.

- [ ] **Step 1: Run full tests**

Run:

```powershell
dotnet test
```

Expected: all tests pass.

- [ ] **Step 2: Run app**

Run:

```powershell
dotnet run --project src/ClinicMateAI.Web
```

Expected: app starts and prints localhost URL.

- [ ] **Step 3: Manual smoke test**

Open the app and verify:

- Dashboard loads.
- Setup wizard loads.
- Promotions page shows demo Botox promo.
- AI test with "โบท็อกกรามเท่าไรคะ" returns Thai service-minded reply with "คุณลูกค้า".
- AI test with "ฉีดแล้วบวมมาก" returns escalation mode.
- Inbox can show a simulated LINE/Facebook message.
- Appointments page shows doctor availability behavior.
- Platform Admin page shows package and usage.
- Integrations page shows LINE/Facebook/Google Calendar as not connected.

- [ ] **Step 4: Document current credential status**

Update `README.md` with:

- How to run app.
- Demo login accounts.
- LINE/Facebook credentials are not required for local test mode.
- Where credentials will be configured when available.

## Self-Review Notes

- Spec coverage: plan includes Blazor/.NET stack, multi-tenant seed, setup flow, services/promotions, AI Thai tone and safety, booking availability, LINE/Facebook webhook skeletons, Google Calendar boundary, admin-only packages, and tests.
- Placeholder scan: no task uses open-ended placeholder markers. Some service APIs are intentionally minimal for MVP and are named explicitly.
- Type consistency: package, promotion, AI decision, and availability types are introduced before later tasks use them.
