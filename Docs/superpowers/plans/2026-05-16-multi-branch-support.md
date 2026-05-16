# Multi-Branch Support Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add Starter/Enterprise package support and full multi-branch clinic operations so each branch has isolated operational data and access, while the clinic owner can see and manage all branches.

**Architecture:** Keep `Clinic` as the tenant root and add `Branch` as a child entity inside the existing clinic boundary. Store package contract data on the clinic, use branch-aware repository filters for all location-specific data, and resolve the active branch from either webhook channel config or the clinic UI branch selector. Keep clinic-wide AI tone/safety rules shared, while services, promotions, schedules, channels, and staff access are branch-scoped.

**Tech Stack:** .NET 10, ASP.NET Core, Blazor Server, EF Core + Npgsql, ASP.NET Core Identity, FluentValidation, xUnit, FluentAssertions, bUnit

---

## File structure map

### Domain and shared contracts

- Modify: `src\ClinicMateAI.Domain\Clinics\Clinic.cs` — add package tier and enterprise branch billing fields.
- Create: `src\ClinicMateAI.Domain\Clinics\Branch.cs` — branch aggregate root under clinic.
- Create: `src\ClinicMateAI.Domain\Clinics\BranchStatus.cs` — active/inactive branch lifecycle.
- Create: `src\ClinicMateAI.Domain\Clinics\ClinicUserProfile.cs` — user-to-clinic role profile.
- Create: `src\ClinicMateAI.Domain\Clinics\ClinicUserRole.cs` — Owner / BranchAdmin / Doctor / Staff.
- Create: `src\ClinicMateAI.Domain\Clinics\UserBranchAssignment.cs` — user-to-branch access mapping.
- Modify: `src\ClinicMateAI.Domain\Clinics\ClinicChannelConfig.cs` — add `BranchId`.
- Modify: `src\ClinicMateAI.Domain\Promotions\Promotion.cs` — add `BranchId` + helper for all-branch promotions.
- Modify: `src\ClinicMateAI.Domain\Services\ClinicService.cs` — add `BranchId` + helper for all-branch services.
- Modify: `src\ClinicMateAI.Domain\Messaging\Conversation.cs` — add required `BranchId`.
- Modify: `src\ClinicMateAI.Domain\Appointments\DoctorAvailability.cs` — require `BranchId`.
- Modify: `src\ClinicMateAI.Domain\Packages\PackageTier.cs` — reduce to `Starter` and `Enterprise`.
- Modify: `src\ClinicMateAI.Logic\Packages\PackageLimitService.cs` — enforce new two-tier limits.

### Application layer

- Create: `src\ClinicMateAI.Application\Abstractions\Auth\IBranchAccessPolicy.cs`
- Create: `src\ClinicMateAI.Application\Abstractions\Persistence\IBranchRepository.cs`
- Create: `src\ClinicMateAI.Application\Abstractions\Persistence\IClinicUserProfileRepository.cs`
- Create: `src\ClinicMateAI.Application\Abstractions\Persistence\IUserBranchAssignmentRepository.cs`
- Create: `src\ClinicMateAI.Application\Ai\IBranchAiContextBuilder.cs`
- Create: `src\ClinicMateAI.Application\Branches\CreateBranchCommand.cs`
- Create: `src\ClinicMateAI.Application\Branches\UpdateBranchCommand.cs`
- Create: `src\ClinicMateAI.Application\Branches\DeactivateBranchCommand.cs`
- Create: `src\ClinicMateAI.Application\Branches\AssignUserToBranchCommand.cs`
- Create: `src\ClinicMateAI.Application\Branches\RemoveUserFromBranchCommand.cs`
- Create: `src\ClinicMateAI.Application\Branches\GetBranchesQuery.cs`
- Create: `src\ClinicMateAI.Application\Branches\GetAccessibleBranchesQuery.cs`
- Create: `src\ClinicMateAI.Application\Branches\BranchListItemDto.cs`
- Create: `src\ClinicMateAI.Application\Branches\AccessibleBranchDto.cs`
- Create: `src\ClinicMateAI.Application\Branches\ICreateBranchHandler.cs`
- Create: `src\ClinicMateAI.Application\Branches\IUpdateBranchHandler.cs`
- Create: `src\ClinicMateAI.Application\Branches\IDeactivateBranchHandler.cs`
- Create: `src\ClinicMateAI.Application\Branches\IAssignUserToBranchHandler.cs`
- Create: `src\ClinicMateAI.Application\Branches\IRemoveUserFromBranchHandler.cs`
- Create: `src\ClinicMateAI.Application\Branches\IGetBranchesHandler.cs`
- Create: `src\ClinicMateAI.Application\Branches\IGetAccessibleBranchesHandler.cs`
- Create: `src\ClinicMateAI.Application\Dashboard\GetOwnerDashboardQuery.cs`
- Create: `src\ClinicMateAI.Application\Dashboard\OwnerDashboardDto.cs`
- Create: `src\ClinicMateAI.Application\Dashboard\OwnerBranchSummaryDto.cs`
- Create: `src\ClinicMateAI.Application\Dashboard\IGetOwnerDashboardHandler.cs`
- Modify: `src\ClinicMateAI.Application\Clinics\CreateClinicCommand.cs`
- Modify: `src\ClinicMateAI.Application\Messaging\ReceiveMessageCommand.cs`
- Modify: `src\ClinicMateAI.Application\Promotions\GetAvailablePromotionsQuery.cs`
- Modify: `src\ClinicMateAI.Application\Setup\AddClinicServiceCommand.cs`
- Modify: `src\ClinicMateAI.Application\Setup\SaveLineChannelConfigCommand.cs`
- Modify: `src\ClinicMateAI.Application\Inbox\GetInboxConversationsQuery.cs`

### Logic layer

- Create: `src\ClinicMateAI.Logic\Branches\CreateBranchHandler.cs`
- Create: `src\ClinicMateAI.Logic\Branches\UpdateBranchHandler.cs`
- Create: `src\ClinicMateAI.Logic\Branches\DeactivateBranchHandler.cs`
- Create: `src\ClinicMateAI.Logic\Branches\AssignUserToBranchHandler.cs`
- Create: `src\ClinicMateAI.Logic\Branches\RemoveUserFromBranchHandler.cs`
- Create: `src\ClinicMateAI.Logic\Branches\GetBranchesHandler.cs`
- Create: `src\ClinicMateAI.Logic\Branches\GetAccessibleBranchesHandler.cs`
- Create: `src\ClinicMateAI.Logic\Branches\BranchAccessPolicy.cs`
- Create: `src\ClinicMateAI.Logic\Branches\CreateBranchCommandValidator.cs`
- Create: `src\ClinicMateAI.Logic\Branches\AssignUserToBranchCommandValidator.cs`
- Create: `src\ClinicMateAI.Logic\Ai\BranchAiContextBuilder.cs`
- Create: `src\ClinicMateAI.Logic\Dashboard\GetOwnerDashboardHandler.cs`
- Modify: `src\ClinicMateAI.Logic\Clinics\CreateClinicHandler.cs`
- Modify: `src\ClinicMateAI.Logic\Messaging\ReceiveMessageHandler.cs`
- Modify: `src\ClinicMateAI.Logic\Promotions\PromotionService.cs`
- Modify: `src\ClinicMateAI.Logic\Promotions\GetAvailablePromotionsHandler.cs`
- Modify: `src\ClinicMateAI.Logic\Setup\AddClinicServiceHandler.cs`
- Modify: `src\ClinicMateAI.Logic\Setup\GetClinicServicesHandler.cs`
- Modify: `src\ClinicMateAI.Logic\Setup\SaveLineChannelConfigHandler.cs`
- Modify: `src\ClinicMateAI.Logic\Inbox\GetInboxConversationsHandler.cs`
- Modify: `src\ClinicMateAI.Logic\Inbox\GetConversationMessagesHandler.cs`

### Infrastructure and data

- Modify: `src\ClinicMateAI.Infrastructure\Data\AppDbContext.cs`
- Modify: `src\ClinicMateAI.Infrastructure\Data\DemoDataSeeder.cs`
- Create: `src\ClinicMateAI.Infrastructure\Persistence\BranchRepository.cs`
- Create: `src\ClinicMateAI.Infrastructure\Persistence\ClinicUserProfileRepository.cs`
- Create: `src\ClinicMateAI.Infrastructure\Persistence\UserBranchAssignmentRepository.cs`
- Modify: `src\ClinicMateAI.Infrastructure\Persistence\ClinicRepository.cs`
- Modify: `src\ClinicMateAI.Infrastructure\Persistence\ClinicServiceRepository.cs`
- Modify: `src\ClinicMateAI.Infrastructure\Persistence\PromotionRepository.cs`
- Modify: `src\ClinicMateAI.Infrastructure\Persistence\ConversationRepository.cs`
- Modify: `src\ClinicMateAI.Infrastructure\Persistence\ClinicChannelConfigRepository.cs`
- Generate EF migrations in:
  - `src\ClinicMateAI.Infrastructure\Data\Migrations\`
  - `src\ClinicMateAI.Web\Data\Migrations\Identity\` only if the identity schema needs new fields

### Web layer

- Create: `src\ClinicMateAI.Web\Endpoints\BranchesEndpoints.cs`
- Create: `src\ClinicMateAI.Web\Endpoints\DashboardEndpoints.cs`
- Create: `src\ClinicMateAI.Web\Services\BranchContextState.cs`
- Create: `src\ClinicMateAI.Web\Components\Shared\BranchSelector.razor`
- Create: `src\ClinicMateAI.Web\Components\Pages\Branches.razor`
- Create: `src\ClinicMateAI.Web\Components\Pages\BranchCreate.razor`
- Create: `src\ClinicMateAI.Web\Components\Pages\BranchSettings.razor`
- Modify: `src\ClinicMateAI.Web\Program.cs`
- Modify: `src\ClinicMateAI.Web\Endpoints\ClinicsEndpoints.cs`
- Modify: `src\ClinicMateAI.Web\Endpoints\WebhookEndpoints.cs`
- Modify: `src\ClinicMateAI.Web\Endpoints\PromotionsEndpoints.cs`
- Modify: `src\ClinicMateAI.Web\Endpoints\SetupEndpoints.cs`
- Modify: `src\ClinicMateAI.Web\Endpoints\IntegrationEndpoints.cs`
- Modify: `src\ClinicMateAI.Web\Endpoints\InboxEndpoints.cs`
- Modify: `src\ClinicMateAI.Web\Components\Layout\ClinicLayout.razor`
- Modify: `src\ClinicMateAI.Web\Components\Layout\ClinicNavMenu.razor`
- Modify: `src\ClinicMateAI.Web\Components\Pages\Home.razor`
- Modify: `src\ClinicMateAI.Web\Components\Pages\Appointments.razor`

### Tests

- Modify: `tests\ClinicMateAI.Tests\Packages\PackageLimitServiceTests.cs`
- Create: `tests\ClinicMateAI.Tests\Clinics\CreateClinicHandlerTests.cs`
- Create: `tests\ClinicMateAI.Tests\Clinics\BranchScopeTests.cs`
- Create: `tests\ClinicMateAI.Tests\Data\DemoDataSeederTests.cs`
- Create: `tests\ClinicMateAI.Tests\Persistence\BranchRepositoryTests.cs`
- Modify: `tests\ClinicMateAI.Tests\Persistence\RepositoryTenantBoundaryTests.cs`
- Create: `tests\ClinicMateAI.Tests\Branches\CreateBranchHandlerTests.cs`
- Create: `tests\ClinicMateAI.Tests\Branches\AssignUserToBranchHandlerTests.cs`
- Create: `tests\ClinicMateAI.Tests\Branches\BranchAccessPolicyTests.cs`
- Modify: `tests\ClinicMateAI.Tests\Promotions\GetAvailablePromotionsHandlerTests.cs`
- Modify: `tests\ClinicMateAI.Tests\Promotions\PromotionServiceTests.cs`
- Modify: `tests\ClinicMateAI.Tests\Promotions\PromotionTests.cs`
- Modify: `tests\ClinicMateAI.Tests\Setup\SaveLineChannelConfigHandlerTests.cs`
- Modify: `tests\ClinicMateAI.Tests\Setup\GetIntegrationOverviewHandlerTests.cs`
- Modify: `tests\ClinicMateAI.Tests\Inbox\InboxQueryHandlersTests.cs`
- Modify: `tests\ClinicMateAI.Tests\Messaging\ReceiveMessageHandlerTests.cs`
- Modify: `tests\ClinicMateAI.Tests\Messaging\LineWebhookTests.cs`
- Create: `tests\ClinicMateAI.Web.Tests\Branches\BranchesEndpointsTests.cs`
- Create: `tests\ClinicMateAI.Web.Tests\Dashboard\DashboardEndpointsTests.cs`
- Create: `tests\ClinicMateAI.Web.Tests\Layout\BranchSelectorTests.cs`
- Modify: `tests\ClinicMateAI.Web.Tests\Promotions\PromotionsEndpointsTests.cs`

## Task 1: Collapse packages to Starter and Enterprise, and persist the clinic contract

**Files:**
- Modify: `src\ClinicMateAI.Domain\Clinics\Clinic.cs`
- Modify: `src\ClinicMateAI.Domain\Packages\PackageTier.cs`
- Modify: `src\ClinicMateAI.Logic\Packages\PackageLimitService.cs`
- Modify: `src\ClinicMateAI.Application\Clinics\CreateClinicCommand.cs`
- Modify: `src\ClinicMateAI.Logic\Clinics\CreateClinicHandler.cs`
- Modify: `src\ClinicMateAI.Web\Endpoints\ClinicsEndpoints.cs`
- Test: `tests\ClinicMateAI.Tests\Packages\PackageLimitServiceTests.cs`
- Test: `tests\ClinicMateAI.Tests\Clinics\CreateClinicHandlerTests.cs`

- [ ] **Step 1: Write the failing package and clinic-creation tests**

```csharp
[Theory]
[InlineData(PackageTier.Starter, 1000, 20, 1, 1, 1)]
[InlineData(PackageTier.Enterprise, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue)]
public void GetLimits_ReturnsConfiguredQuota(
    PackageTier tier,
    int aiReplies,
    int services,
    int admins,
    int channels,
    int branches)
{
    var limits = PackageLimitService.GetLimits(tier);

    limits.MonthlyAiReplies.Should().Be(aiReplies);
    limits.MaxServices.Should().Be(services);
    limits.MaxAdminSeats.Should().Be(admins);
    limits.MaxChannels.Should().Be(channels);
    limits.MaxBranches.Should().Be(branches);
}

[Fact]
public async Task HandleAsync_SetsEnterpriseBranchPricing_WhenEnterprisePackageIsSelected()
{
    var handler = CreateHandler();
    var result = await handler.HandleAsync(new CreateClinicCommand(
        Name: "Chain Clinic",
        Address: "Bangkok",
        Phone: "02-000-0000",
        MapUrl: "https://maps.example/chain",
        Status: "Active",
        PackageTier: PackageTier.Enterprise,
        AdditionalBranchMonthlyPrice: 3500m));

    SavedClinic!.PackageTier.Should().Be(PackageTier.Enterprise);
    SavedClinic.AdditionalBranchMonthlyPrice.Should().Be(3500m);
}
```

- [ ] **Step 2: Run the targeted tests to verify they fail**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter "FullyQualifiedName~PackageLimitServiceTests|FullyQualifiedName~CreateClinicHandlerTests"
```

Expected: FAIL because `CreateClinicCommand` and `Clinic` do not yet contain package contract fields, and the old package tiers still exist.

- [ ] **Step 3: Implement the two-tier package model and clinic contract fields**

```csharp
public enum PackageTier
{
    Starter = 1,
    Enterprise = 2
}

public sealed class Clinic
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public PackageTier PackageTier { get; set; } = PackageTier.Starter;
    public decimal? AdditionalBranchMonthlyPrice { get; set; }
}

public sealed record CreateClinicCommand(
    string Name,
    string Address,
    string Phone,
    string? MapUrl,
    string Status,
    PackageTier PackageTier,
    decimal? AdditionalBranchMonthlyPrice = null);
```

```csharp
PackageTier.Starter => new PackageLimit(tier, 1000, 20, 1, 1, 1),
PackageTier.Enterprise => new PackageLimit(tier, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue),
```

- [ ] **Step 4: Run the targeted tests again**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter "FullyQualifiedName~PackageLimitServiceTests|FullyQualifiedName~CreateClinicHandlerTests"
```

Expected: PASS.

- [ ] **Step 5: Commit**

```powershell
git add .\src\ClinicMateAI.Domain\Packages\PackageTier.cs .\src\ClinicMateAI.Logic\Packages\PackageLimitService.cs .\src\ClinicMateAI.Domain\Clinics\Clinic.cs .\src\ClinicMateAI.Application\Clinics\CreateClinicCommand.cs .\src\ClinicMateAI.Logic\Clinics\CreateClinicHandler.cs .\src\ClinicMateAI.Web\Endpoints\ClinicsEndpoints.cs .\tests\ClinicMateAI.Tests\Packages\PackageLimitServiceTests.cs .\tests\ClinicMateAI.Tests\Clinics\CreateClinicHandlerTests.cs
git commit -m "feat: simplify clinic packages"
```

## Task 2: Add the branch and user-scope domain model

**Files:**
- Create: `src\ClinicMateAI.Domain\Clinics\BranchStatus.cs`
- Create: `src\ClinicMateAI.Domain\Clinics\Branch.cs`
- Create: `src\ClinicMateAI.Domain\Clinics\ClinicUserRole.cs`
- Create: `src\ClinicMateAI.Domain\Clinics\ClinicUserProfile.cs`
- Create: `src\ClinicMateAI.Domain\Clinics\UserBranchAssignment.cs`
- Modify: `src\ClinicMateAI.Domain\Clinics\ClinicChannelConfig.cs`
- Modify: `src\ClinicMateAI.Domain\Promotions\Promotion.cs`
- Modify: `src\ClinicMateAI.Domain\Services\ClinicService.cs`
- Modify: `src\ClinicMateAI.Domain\Messaging\Conversation.cs`
- Modify: `src\ClinicMateAI.Domain\Appointments\DoctorAvailability.cs`
- Test: `tests\ClinicMateAI.Tests\Clinics\BranchScopeTests.cs`

- [ ] **Step 1: Write the failing branch-scope tests first**

```csharp
[Fact]
public void Promotion_AppliesToBranch_ReturnsTrue_ForAllBranchPromotion()
{
    var promotion = new Promotion { ClinicId = Guid.NewGuid(), BranchId = null, Name = "All branches" };

    promotion.AppliesToBranch(Guid.NewGuid()).Should().BeTrue();
}

[Fact]
public void ClinicService_AppliesToBranch_ReturnsTrue_OnlyForMatchingBranch_WhenScoped()
{
    var branchId = Guid.NewGuid();
    var service = new ClinicService { ClinicId = Guid.NewGuid(), BranchId = branchId, Name = "Botox" };

    service.AppliesToBranch(branchId).Should().BeTrue();
    service.AppliesToBranch(Guid.NewGuid()).Should().BeFalse();
}

[Fact]
public void Branch_DefaultsToActiveStatus()
{
    var branch = new Branch();

    branch.Status.Should().Be(BranchStatus.Active);
    branch.IsDefault.Should().BeFalse();
}
```

- [ ] **Step 2: Run the branch-scope tests and confirm they fail**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter "FullyQualifiedName~BranchScopeTests"
```

Expected: FAIL because the branch entities and helper methods do not exist yet.

- [ ] **Step 3: Add the new clinic branch and user-scope entities**

```csharp
public enum ClinicUserRole
{
    Owner = 1,
    BranchAdmin = 2,
    Doctor = 3,
    Staff = 4
}

public sealed class Branch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClinicId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string MapUrl { get; set; } = string.Empty;
    public string BusinessHours { get; set; } = string.Empty;
    public BranchStatus Status { get; set; } = BranchStatus.Active;
    public bool IsDefault { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
```

```csharp
public sealed class ClinicUserProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public Guid ClinicId { get; set; }
    public ClinicUserRole Role { get; set; }
    public Guid? DefaultBranchId { get; set; }
}
```

- [ ] **Step 4: Add `BranchId` and helper methods to existing branch-scoped entities**

```csharp
public sealed class Promotion
{
    public Guid? BranchId { get; set; }

    public bool AppliesToBranch(Guid branchId)
        => BranchId is null || BranchId == branchId;
}

public sealed class ClinicService
{
    public Guid? BranchId { get; set; }

    public bool AppliesToBranch(Guid branchId)
        => BranchId is null || BranchId == branchId;
}

public sealed class Conversation
{
    public Guid BranchId { get; set; }
}
```

- [ ] **Step 5: Run the tests again**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter "FullyQualifiedName~BranchScopeTests"
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add .\src\ClinicMateAI.Domain\Clinics\BranchStatus.cs .\src\ClinicMateAI.Domain\Clinics\Branch.cs .\src\ClinicMateAI.Domain\Clinics\ClinicUserRole.cs .\src\ClinicMateAI.Domain\Clinics\ClinicUserProfile.cs .\src\ClinicMateAI.Domain\Clinics\UserBranchAssignment.cs .\src\ClinicMateAI.Domain\Clinics\ClinicChannelConfig.cs .\src\ClinicMateAI.Domain\Promotions\Promotion.cs .\src\ClinicMateAI.Domain\Services\ClinicService.cs .\src\ClinicMateAI.Domain\Messaging\Conversation.cs .\src\ClinicMateAI.Domain\Appointments\DoctorAvailability.cs .\tests\ClinicMateAI.Tests\Clinics\BranchScopeTests.cs
git commit -m "feat: add branch domain model"
```

## Task 3: Wire the EF Core model, create default branches, and update seed data

**Files:**
- Modify: `src\ClinicMateAI.Infrastructure\Data\AppDbContext.cs`
- Modify: `src\ClinicMateAI.Infrastructure\Data\DemoDataSeeder.cs`
- Test: `tests\ClinicMateAI.Tests\Data\DemoDataSeederTests.cs`

- [ ] **Step 1: Write the failing seeder test**

```csharp
[Fact]
public async Task SeedAsync_CreatesDefaultBranch_AndAttachesSeedDataToIt()
{
    await using var db = CreateDb();

    await DemoDataSeeder.SeedAsync(db);

    var clinic = await db.Clinics.SingleAsync();
    var branch = await db.Set<Branch>().SingleAsync();
    var service = await db.Services.SingleAsync();
    var promotion = await db.Promotions.SingleAsync(x => x.Status == PromotionStatus.Published);
    var conversation = await db.Conversations.SingleAsync();

    branch.ClinicId.Should().Be(clinic.Id);
    branch.IsDefault.Should().BeTrue();
    service.BranchId.Should().Be(branch.Id);
    promotion.BranchId.Should().Be(branch.Id);
    conversation.BranchId.Should().Be(branch.Id);
}
```

- [ ] **Step 2: Run the seeder test and confirm it fails**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter "FullyQualifiedName~DemoDataSeederTests"
```

Expected: FAIL because `Branch` is not in `AppDbContext` and the seed data does not populate branch IDs.

- [ ] **Step 3: Update `AppDbContext` and `DemoDataSeeder`**

```csharp
public DbSet<Branch> Branches => Set<Branch>();
public DbSet<ClinicUserProfile> ClinicUserProfiles => Set<ClinicUserProfile>();
public DbSet<UserBranchAssignment> UserBranchAssignments => Set<UserBranchAssignment>();

modelBuilder.Entity<Branch>()
    .HasIndex(x => new { x.ClinicId, x.Name })
    .IsUnique();

modelBuilder.Entity<ClinicChannelConfig>()
    .HasIndex(x => new { x.ClinicId, x.BranchId, x.Channel })
    .IsUnique();
```

```csharp
var defaultBranch = new Branch
{
    Id = Guid.Parse("aaaaaaaa-1111-1111-1111-111111111111"),
    ClinicId = clinic.Id,
    Name = "สาขาหลัก",
    Address = clinic.Address,
    Phone = clinic.Phone,
    MapUrl = clinic.MapUrl,
    BusinessHours = "Mon-Sun 10:00-19:00",
    IsDefault = true
};
```

- [ ] **Step 4: Generate the database migration after the model compiles**

Run:

```powershell
dotnet ef migrations add AddBranchSupportAndSimplifyPackages --project .\src\ClinicMateAI.Infrastructure\ClinicMateAI.Infrastructure.csproj --startup-project .\src\ClinicMateAI.Web\ClinicMateAI.Web.csproj --context AppDbContext
```

Expected: A new migration pair appears under `src\ClinicMateAI.Infrastructure\Data\Migrations\`.

- [ ] **Step 5: Re-run the seeder test**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter "FullyQualifiedName~DemoDataSeederTests"
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add .\src\ClinicMateAI.Infrastructure\Data\AppDbContext.cs .\src\ClinicMateAI.Infrastructure\Data\DemoDataSeeder.cs .\src\ClinicMateAI.Infrastructure\Data\Migrations .\tests\ClinicMateAI.Tests\Data\DemoDataSeederTests.cs
git commit -m "feat: add branch data model to persistence"
```

## Task 4: Add repositories and branch-aware persistence filters

**Files:**
- Create: `src\ClinicMateAI.Application\Abstractions\Persistence\IBranchRepository.cs`
- Create: `src\ClinicMateAI.Application\Abstractions\Persistence\IClinicUserProfileRepository.cs`
- Create: `src\ClinicMateAI.Application\Abstractions\Persistence\IUserBranchAssignmentRepository.cs`
- Create: `src\ClinicMateAI.Infrastructure\Persistence\BranchRepository.cs`
- Create: `src\ClinicMateAI.Infrastructure\Persistence\ClinicUserProfileRepository.cs`
- Create: `src\ClinicMateAI.Infrastructure\Persistence\UserBranchAssignmentRepository.cs`
- Modify: `src\ClinicMateAI.Application\Abstractions\Persistence\IConversationRepository.cs`
- Modify: `src\ClinicMateAI.Application\Abstractions\Persistence\IPromotionRepository.cs`
- Modify: `src\ClinicMateAI.Application\Abstractions\Persistence\IClinicServiceRepository.cs`
- Modify: `src\ClinicMateAI.Application\Abstractions\Persistence\IClinicChannelConfigRepository.cs`
- Modify: `src\ClinicMateAI.Infrastructure\Persistence\ConversationRepository.cs`
- Modify: `src\ClinicMateAI.Infrastructure\Persistence\PromotionRepository.cs`
- Modify: `src\ClinicMateAI.Infrastructure\Persistence\ClinicServiceRepository.cs`
- Modify: `src\ClinicMateAI.Infrastructure\Persistence\ClinicChannelConfigRepository.cs`
- Modify: `src\ClinicMateAI.Web\Program.cs`
- Test: `tests\ClinicMateAI.Tests\Persistence\BranchRepositoryTests.cs`
- Test: `tests\ClinicMateAI.Tests\Persistence\RepositoryTenantBoundaryTests.cs`

- [ ] **Step 1: Write the failing repository tests**

```csharp
[Fact]
public async Task ListByClinicAsync_ReturnsMatchingBranchAndAllBranchPromotions()
{
    await using var db = CreateDb();
    var clinicId = Guid.NewGuid();
    var branchId = Guid.NewGuid();

    db.Promotions.AddRange(
        new Promotion { ClinicId = clinicId, BranchId = branchId, Name = "Branch only", StartsOn = DateOnly.FromDateTime(DateTime.UtcNow), EndsOn = DateOnly.FromDateTime(DateTime.UtcNow), Conditions = "x", ApprovedAiWording = "x" },
        new Promotion { ClinicId = clinicId, BranchId = null, Name = "All branch", StartsOn = DateOnly.FromDateTime(DateTime.UtcNow), EndsOn = DateOnly.FromDateTime(DateTime.UtcNow), Conditions = "x", ApprovedAiWording = "x" });
    await db.SaveChangesAsync();

    var repository = new PromotionRepository(db);
    var result = await repository.ListByClinicAsync(clinicId, branchId);

    result.Select(x => x.Name).Should().BeEquivalentTo(["Branch only", "All branch"]);
}
```

- [ ] **Step 2: Run the repository tests and confirm they fail**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter "FullyQualifiedName~BranchRepositoryTests|FullyQualifiedName~RepositoryTenantBoundaryTests"
```

Expected: FAIL because repository interfaces and implementations do not accept branch filters yet.

- [ ] **Step 3: Add the new repository contracts and branch-aware signatures**

```csharp
public interface IBranchRepository
{
    Task<Branch?> GetByIdAsync(Guid clinicId, Guid branchId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Branch>> ListByClinicAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task<Branch?> GetDefaultAsync(Guid clinicId, CancellationToken cancellationToken = default);
    Task AddAsync(Branch branch, CancellationToken cancellationToken = default);
}

public interface IPromotionRepository
{
    Task<IReadOnlyList<Promotion>> ListByClinicAsync(
        Guid clinicId,
        Guid? branchId,
        CancellationToken cancellationToken = default);
}
```

- [ ] **Step 4: Implement branch-aware repository queries**

```csharp
public async Task<IReadOnlyList<Promotion>> ListByClinicAsync(Guid clinicId, Guid? branchId, CancellationToken cancellationToken = default)
{
    var query = db.Promotions.Where(x => x.ClinicId == clinicId);

    if (branchId is not null)
    {
        query = query.Where(x => x.BranchId == null || x.BranchId == branchId);
    }

    return await query.OrderByDescending(x => x.StartsOn).ToListAsync(cancellationToken);
}
```

```csharp
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<IClinicUserProfileRepository, ClinicUserProfileRepository>();
builder.Services.AddScoped<IUserBranchAssignmentRepository, UserBranchAssignmentRepository>();
```

- [ ] **Step 5: Re-run the repository tests**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter "FullyQualifiedName~BranchRepositoryTests|FullyQualifiedName~RepositoryTenantBoundaryTests"
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add .\src\ClinicMateAI.Application\Abstractions\Persistence .\src\ClinicMateAI.Infrastructure\Persistence .\src\ClinicMateAI.Web\Program.cs .\tests\ClinicMateAI.Tests\Persistence\BranchRepositoryTests.cs .\tests\ClinicMateAI.Tests\Persistence\RepositoryTenantBoundaryTests.cs
git commit -m "feat: add branch-aware repositories"
```

## Task 5: Build branch CRUD, staff assignment, and package enforcement handlers

**Files:**
- Create: `src\ClinicMateAI.Application\Branches\CreateBranchCommand.cs`
- Create: `src\ClinicMateAI.Application\Branches\UpdateBranchCommand.cs`
- Create: `src\ClinicMateAI.Application\Branches\DeactivateBranchCommand.cs`
- Create: `src\ClinicMateAI.Application\Branches\AssignUserToBranchCommand.cs`
- Create: `src\ClinicMateAI.Application\Branches\RemoveUserFromBranchCommand.cs`
- Create: `src\ClinicMateAI.Application\Branches\GetBranchesQuery.cs`
- Create: `src\ClinicMateAI.Application\Branches\BranchListItemDto.cs`
- Create: `src\ClinicMateAI.Application\Branches\ICreateBranchHandler.cs`
- Create: `src\ClinicMateAI.Application\Branches\IUpdateBranchHandler.cs`
- Create: `src\ClinicMateAI.Application\Branches\IDeactivateBranchHandler.cs`
- Create: `src\ClinicMateAI.Application\Branches\IAssignUserToBranchHandler.cs`
- Create: `src\ClinicMateAI.Application\Branches\IRemoveUserFromBranchHandler.cs`
- Create: `src\ClinicMateAI.Application\Branches\IGetBranchesHandler.cs`
- Create: `src\ClinicMateAI.Logic\Branches\CreateBranchHandler.cs`
- Create: `src\ClinicMateAI.Logic\Branches\UpdateBranchHandler.cs`
- Create: `src\ClinicMateAI.Logic\Branches\DeactivateBranchHandler.cs`
- Create: `src\ClinicMateAI.Logic\Branches\AssignUserToBranchHandler.cs`
- Create: `src\ClinicMateAI.Logic\Branches\RemoveUserFromBranchHandler.cs`
- Create: `src\ClinicMateAI.Logic\Branches\GetBranchesHandler.cs`
- Create: `src\ClinicMateAI.Logic\Branches\CreateBranchCommandValidator.cs`
- Create: `src\ClinicMateAI.Logic\Branches\AssignUserToBranchCommandValidator.cs`
- Modify: `src\ClinicMateAI.Web\Program.cs`
- Test: `tests\ClinicMateAI.Tests\Branches\CreateBranchHandlerTests.cs`
- Test: `tests\ClinicMateAI.Tests\Branches\AssignUserToBranchHandlerTests.cs`

- [ ] **Step 1: Write the failing handler tests**

```csharp
[Fact]
public async Task HandleAsync_Throws_WhenStarterClinicAlreadyHasOneBranch()
{
    var clinic = new Clinic { Id = Guid.NewGuid(), PackageTier = PackageTier.Starter };
    var existingBranch = new Branch { ClinicId = clinic.Id, Name = "Main", IsDefault = true };
    var handler = CreateCreateBranchHandler(clinic, [existingBranch]);

    var act = () => handler.HandleAsync(new CreateBranchCommand(clinic.Id, "Second", "Addr", "02-1", "map", "Mon-Sun 10:00-19:00"));

    await act.Should().ThrowAsync<BusinessException>();
}

[Fact]
public async Task HandleAsync_AssignsBranchAdmin_ToRequestedBranch()
{
    var clinicId = Guid.NewGuid();
    var branchId = Guid.NewGuid();
    var profile = new ClinicUserProfile { UserId = "user-1", ClinicId = clinicId, Role = ClinicUserRole.BranchAdmin };
    var handler = CreateAssignUserHandler(profile, clinicId, branchId);

    await handler.HandleAsync(new AssignUserToBranchCommand(clinicId, "user-1", branchId));

    SavedAssignments.Should().ContainSingle(x => x.UserId == "user-1" && x.BranchId == branchId);
}
```

- [ ] **Step 2: Run the handler tests and confirm they fail**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter "FullyQualifiedName~CreateBranchHandlerTests|FullyQualifiedName~AssignUserToBranchHandlerTests"
```

Expected: FAIL because the branch command types and handlers do not exist yet.

- [ ] **Step 3: Add the application command/query contracts**

```csharp
public sealed record CreateBranchCommand(
    Guid ClinicId,
    string Name,
    string Address,
    string Phone,
    string MapUrl,
    string BusinessHours);

public sealed record AssignUserToBranchCommand(
    Guid ClinicId,
    string UserId,
    Guid BranchId);
```

- [ ] **Step 4: Implement the handlers with package and clinic-scope checks**

```csharp
if (clinic.PackageTier == PackageTier.Starter && existingBranches.Count >= 1)
{
    throw new BusinessException(BusinessErrorCode.BranchLimitExceeded, "Starter package supports exactly one branch.");
}

var branch = new Branch
{
    ClinicId = clinic.Id,
    Name = command.Name.Trim(),
    Address = command.Address.Trim(),
    Phone = command.Phone.Trim(),
    MapUrl = command.MapUrl.Trim(),
    BusinessHours = command.BusinessHours.Trim(),
    IsDefault = false
};
```

```csharp
if (profile.ClinicId != command.ClinicId)
{
    throw new BusinessException(BusinessErrorCode.AccessDenied, "User does not belong to this clinic.");
}
```

- [ ] **Step 5: Run the handler tests again**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter "FullyQualifiedName~CreateBranchHandlerTests|FullyQualifiedName~AssignUserToBranchHandlerTests"
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add .\src\ClinicMateAI.Application\Branches .\src\ClinicMateAI.Logic\Branches .\src\ClinicMateAI.Web\Program.cs .\tests\ClinicMateAI.Tests\Branches\CreateBranchHandlerTests.cs .\tests\ClinicMateAI.Tests\Branches\AssignUserToBranchHandlerTests.cs
git commit -m "feat: add branch management handlers"
```

## Task 6: Add branch access policy and accessible-branch queries

**Files:**
- Create: `src\ClinicMateAI.Application\Abstractions\Auth\IBranchAccessPolicy.cs`
- Create: `src\ClinicMateAI.Application\Branches\GetAccessibleBranchesQuery.cs`
- Create: `src\ClinicMateAI.Application\Branches\AccessibleBranchDto.cs`
- Create: `src\ClinicMateAI.Logic\Branches\BranchAccessPolicy.cs`
- Create: `src\ClinicMateAI.Logic\Branches\GetAccessibleBranchesHandler.cs`
- Modify: `src\ClinicMateAI.Web\Program.cs`
- Test: `tests\ClinicMateAI.Tests\Branches\BranchAccessPolicyTests.cs`

- [ ] **Step 1: Write the failing access policy tests**

```csharp
[Fact]
public async Task GetAccessibleBranchIdsAsync_ReturnsAllBranches_ForOwner()
{
    var clinicId = Guid.NewGuid();
    var profile = new ClinicUserProfile { UserId = "owner", ClinicId = clinicId, Role = ClinicUserRole.Owner };
    var policy = CreatePolicy(profile, assignedBranchIds: []);

    var branchIds = await policy.GetAccessibleBranchIdsAsync("owner", clinicId);

    branchIds.Should().HaveCount(3);
}

[Fact]
public async Task CanAccessBranchAsync_ReturnsFalse_ForUnassignedStaffBranch()
{
    var clinicId = Guid.NewGuid();
    var branchId = Guid.NewGuid();
    var profile = new ClinicUserProfile { UserId = "staff-1", ClinicId = clinicId, Role = ClinicUserRole.Staff };
    var policy = CreatePolicy(profile, assignedBranchIds: []);

    var allowed = await policy.CanAccessBranchAsync("staff-1", clinicId, branchId);

    allowed.Should().BeFalse();
}
```

- [ ] **Step 2: Run the tests and confirm they fail**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter "FullyQualifiedName~BranchAccessPolicyTests"
```

Expected: FAIL because `IBranchAccessPolicy` and its implementation do not exist yet.

- [ ] **Step 3: Add the branch access abstraction**

```csharp
public interface IBranchAccessPolicy
{
    Task<IReadOnlyList<Guid>> GetAccessibleBranchIdsAsync(string userId, Guid clinicId, CancellationToken cancellationToken = default);
    Task<bool> CanAccessBranchAsync(string userId, Guid clinicId, Guid branchId, CancellationToken cancellationToken = default);
}
```

- [ ] **Step 4: Implement owner vs assigned-branch logic**

```csharp
if (profile.Role == ClinicUserRole.Owner)
{
    var allBranches = await branchRepository.ListByClinicAsync(clinicId, cancellationToken);
    return allBranches.Select(x => x.Id).ToList();
}

return await userBranchAssignmentRepository.GetBranchIdsForUserAsync(userId, clinicId, cancellationToken);
```

- [ ] **Step 5: Re-run the tests**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter "FullyQualifiedName~BranchAccessPolicyTests"
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add .\src\ClinicMateAI.Application\Abstractions\Auth\IBranchAccessPolicy.cs .\src\ClinicMateAI.Application\Branches\GetAccessibleBranchesQuery.cs .\src\ClinicMateAI.Application\Branches\AccessibleBranchDto.cs .\src\ClinicMateAI.Logic\Branches\BranchAccessPolicy.cs .\src\ClinicMateAI.Logic\Branches\GetAccessibleBranchesHandler.cs .\src\ClinicMateAI.Web\Program.cs .\tests\ClinicMateAI.Tests\Branches\BranchAccessPolicyTests.cs
git commit -m "feat: add branch access policy"
```

## Task 7: Make setup, channel config, services, and promotions branch-aware

**Files:**
- Modify: `src\ClinicMateAI.Application\Setup\AddClinicServiceCommand.cs`
- Modify: `src\ClinicMateAI.Application\Setup\SaveLineChannelConfigCommand.cs`
- Modify: `src\ClinicMateAI.Application\Promotions\GetAvailablePromotionsQuery.cs`
- Modify: `src\ClinicMateAI.Logic\Setup\AddClinicServiceHandler.cs`
- Modify: `src\ClinicMateAI.Logic\Setup\GetClinicServicesHandler.cs`
- Modify: `src\ClinicMateAI.Logic\Setup\SaveLineChannelConfigHandler.cs`
- Modify: `src\ClinicMateAI.Logic\Promotions\PromotionService.cs`
- Modify: `src\ClinicMateAI.Logic\Promotions\GetAvailablePromotionsHandler.cs`
- Modify: `src\ClinicMateAI.Web\Endpoints\SetupEndpoints.cs`
- Modify: `src\ClinicMateAI.Web\Endpoints\IntegrationEndpoints.cs`
- Modify: `src\ClinicMateAI.Web\Endpoints\PromotionsEndpoints.cs`
- Modify: `tests\ClinicMateAI.Tests\Setup\SaveLineChannelConfigHandlerTests.cs`
- Modify: `tests\ClinicMateAI.Tests\Setup\GetIntegrationOverviewHandlerTests.cs`
- Modify: `tests\ClinicMateAI.Tests\Promotions\GetAvailablePromotionsHandlerTests.cs`
- Modify: `tests\ClinicMateAI.Tests\Promotions\PromotionServiceTests.cs`
- Modify: `tests\ClinicMateAI.Tests\Promotions\PromotionTests.cs`
- Modify: `tests\ClinicMateAI.Web.Tests\Promotions\PromotionsEndpointsTests.cs`

- [ ] **Step 1: Write failing promotion and setup tests**

```csharp
[Fact]
public async Task ListByClinicAsync_ReturnsSharedAndMatchingBranchServices()
{
    var branchId = Guid.NewGuid();
    var result = await handler.HandleAsync(new GetClinicServicesQuery(clinicId, branchId));

    result.Select(x => x.Name).Should().BeEquivalentTo(["Shared Botox", "Branch Laser"]);
}

[Fact]
public async Task GetAvailablePromotions_ReturnsOnlyCurrentBranchAndAllBranchPromotions()
{
    var response = await client.GetAsync($"/api/promotions/available?clinicId={clinicId}&branchId={branchId}");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
}
```

- [ ] **Step 2: Run the targeted tests and confirm they fail**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter "FullyQualifiedName~Promotions|FullyQualifiedName~Setup"
dotnet test .\tests\ClinicMateAI.Web.Tests\ClinicMateAI.Web.Tests.csproj --filter "FullyQualifiedName~PromotionsEndpointsTests"
```

Expected: FAIL because commands, handlers, and endpoints do not accept `branchId`.

- [ ] **Step 3: Add `BranchId` to setup and promotion contracts**

```csharp
public sealed record AddClinicServiceCommand(
    Guid ClinicId,
    Guid? BranchId,
    string Name,
    string Category,
    decimal StartingPrice,
    int DurationMinutes,
    bool RequiresDoctorAssessment,
    string ApprovedAiWording);

public sealed record SaveLineChannelConfigCommand(
    Guid ClinicId,
    Guid BranchId,
    string ChannelSecret,
    string AccessToken);
```

- [ ] **Step 4: Update the handlers and endpoints to include shared + branch-scoped data**

```csharp
var services = await clinicServiceRepository.ListByClinicAsync(command.ClinicId, command.BranchId, cancellationToken);

var promotions = await promotionRepository.ListByClinicAsync(query.ClinicId, query.BranchId, cancellationToken);
return promotions.Where(x => x.IsAvailableToAi(today)).Select(Map).ToList();
```

```csharp
endpoints.MapGet("/api/promotions/available", async (
    Guid clinicId,
    Guid? branchId,
    IGetAvailablePromotionsHandler handler,
    CancellationToken ct) =>
{
    var result = await handler.HandleAsync(new GetAvailablePromotionsQuery(clinicId, branchId), ct);
    return Results.Ok(result);
});
```

- [ ] **Step 5: Re-run the targeted tests**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter "FullyQualifiedName~Promotions|FullyQualifiedName~Setup"
dotnet test .\tests\ClinicMateAI.Web.Tests\ClinicMateAI.Web.Tests.csproj --filter "FullyQualifiedName~PromotionsEndpointsTests"
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add .\src\ClinicMateAI.Application\Setup .\src\ClinicMateAI.Application\Promotions .\src\ClinicMateAI.Logic\Setup .\src\ClinicMateAI.Logic\Promotions .\src\ClinicMateAI.Web\Endpoints\SetupEndpoints.cs .\src\ClinicMateAI.Web\Endpoints\IntegrationEndpoints.cs .\src\ClinicMateAI.Web\Endpoints\PromotionsEndpoints.cs .\tests\ClinicMateAI.Tests\Setup .\tests\ClinicMateAI.Tests\Promotions .\tests\ClinicMateAI.Web.Tests\Promotions\PromotionsEndpointsTests.cs
git commit -m "feat: scope setup and promotions by branch"
```

## Task 8: Make webhook intake, inbox, and AI context branch-aware

**Files:**
- Create: `src\ClinicMateAI.Application\Ai\IBranchAiContextBuilder.cs`
- Create: `src\ClinicMateAI.Logic\Ai\BranchAiContextBuilder.cs`
- Modify: `src\ClinicMateAI.Application\Messaging\ReceiveMessageCommand.cs`
- Modify: `src\ClinicMateAI.Application\Inbox\GetInboxConversationsQuery.cs`
- Modify: `src\ClinicMateAI.Logic\Messaging\ReceiveMessageHandler.cs`
- Modify: `src\ClinicMateAI.Logic\Inbox\GetInboxConversationsHandler.cs`
- Modify: `src\ClinicMateAI.Logic\Inbox\GetConversationMessagesHandler.cs`
- Modify: `src\ClinicMateAI.Web\Endpoints\WebhookEndpoints.cs`
- Modify: `src\ClinicMateAI.Web\Endpoints\InboxEndpoints.cs`
- Modify: `tests\ClinicMateAI.Tests\Messaging\ReceiveMessageHandlerTests.cs`
- Modify: `tests\ClinicMateAI.Tests\Messaging\LineWebhookTests.cs`
- Modify: `tests\ClinicMateAI.Tests\Inbox\InboxQueryHandlersTests.cs`

- [ ] **Step 1: Write the failing messaging and inbox tests**

```csharp
[Fact]
public async Task HandleAsync_PersistsConversationBranchId_FromCommand()
{
    var branchId = Guid.NewGuid();
    var result = await handler.HandleAsync(new ReceiveMessageCommand(
        ClinicId: clinicId,
        BranchId: branchId,
        Channel: "LINE",
        ExternalConversationId: "line-9",
        CustomerDisplayName: "Customer A",
        Text: "โบท็อกกรามเท่าไรคะ",
        ReceivedAt: DateTimeOffset.UtcNow));

    conversationRepository.Items[0].BranchId.Should().Be(branchId);
}

[Fact]
public async Task GetInboxConversationsHandler_ReturnsOnlySelectedBranch()
{
    var result = await handler.HandleAsync(new GetInboxConversationsQuery(clinicId, branchId, take: 20));

    result.Should().OnlyContain(x => x.BranchId == branchId);
}
```

- [ ] **Step 2: Run the tests and confirm they fail**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter "FullyQualifiedName~ReceiveMessageHandlerTests|FullyQualifiedName~LineWebhookTests|FullyQualifiedName~InboxQueryHandlersTests"
```

Expected: FAIL because `ReceiveMessageCommand`, conversations, and inbox queries do not carry branch context yet.

- [ ] **Step 3: Add `BranchId` to the message flow and inbox queries**

```csharp
public sealed record ReceiveMessageCommand(
    Guid ClinicId,
    Guid BranchId,
    string Channel,
    string ExternalConversationId,
    string CustomerDisplayName,
    string Text,
    DateTimeOffset ReceivedAt,
    string? ExternalMessageId = null);
```

```csharp
public sealed record GetInboxConversationsQuery(Guid ClinicId, Guid? BranchId, int Take = 50);
```

- [ ] **Step 4: Resolve the branch from channel config and build branch-specific AI facts**

```csharp
var configs = await channelConfigRepo.GetAllByClinicAsync(clinicId, ct);
var config = configs
    .Where(x => x.Channel == "LINE")
    .FirstOrDefault(x => signatureVerifier.Verify(body, signature, x.Secret));

if (config is null)
    return Results.BadRequest("Invalid LINE signature.");
```

```csharp
var approvedFacts = await branchAiContextBuilder.BuildAsync(command.ClinicId, command.BranchId, cancellationToken);
var aiResult = await orchestrator.GenerateReplyAsync(
    new AiReplyRequest(command.Text, HasApprovedData: true, Confidence: 0.9m, ApprovedClinicFacts: approvedFacts),
    aiCts.Token);
```

- [ ] **Step 5: Re-run the targeted tests**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter "FullyQualifiedName~ReceiveMessageHandlerTests|FullyQualifiedName~LineWebhookTests|FullyQualifiedName~InboxQueryHandlersTests"
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add .\src\ClinicMateAI.Application\Ai\IBranchAiContextBuilder.cs .\src\ClinicMateAI.Logic\Ai\BranchAiContextBuilder.cs .\src\ClinicMateAI.Application\Messaging\ReceiveMessageCommand.cs .\src\ClinicMateAI.Application\Inbox\GetInboxConversationsQuery.cs .\src\ClinicMateAI.Logic\Messaging\ReceiveMessageHandler.cs .\src\ClinicMateAI.Logic\Inbox .\src\ClinicMateAI.Web\Endpoints\WebhookEndpoints.cs .\src\ClinicMateAI.Web\Endpoints\InboxEndpoints.cs .\tests\ClinicMateAI.Tests\Messaging\ReceiveMessageHandlerTests.cs .\tests\ClinicMateAI.Tests\Messaging\LineWebhookTests.cs .\tests\ClinicMateAI.Tests\Inbox\InboxQueryHandlersTests.cs
git commit -m "feat: add branch-aware messaging and inbox"
```

## Task 9: Add branch endpoints, selector UI, branch pages, and owner dashboard

**Files:**
- Create: `src\ClinicMateAI.Web\Endpoints\BranchesEndpoints.cs`
- Create: `src\ClinicMateAI.Web\Endpoints\DashboardEndpoints.cs`
- Create: `src\ClinicMateAI.Web\Services\BranchContextState.cs`
- Create: `src\ClinicMateAI.Web\Components\Shared\BranchSelector.razor`
- Create: `src\ClinicMateAI.Web\Components\Pages\Branches.razor`
- Create: `src\ClinicMateAI.Web\Components\Pages\BranchCreate.razor`
- Create: `src\ClinicMateAI.Web\Components\Pages\BranchSettings.razor`
- Create: `src\ClinicMateAI.Application\Dashboard\GetOwnerDashboardQuery.cs`
- Create: `src\ClinicMateAI.Application\Dashboard\OwnerDashboardDto.cs`
- Create: `src\ClinicMateAI.Application\Dashboard\OwnerBranchSummaryDto.cs`
- Create: `src\ClinicMateAI.Application\Dashboard\IGetOwnerDashboardHandler.cs`
- Create: `src\ClinicMateAI.Logic\Dashboard\GetOwnerDashboardHandler.cs`
- Modify: `src\ClinicMateAI.Web\Program.cs`
- Modify: `src\ClinicMateAI.Web\Components\Layout\ClinicLayout.razor`
- Modify: `src\ClinicMateAI.Web\Components\Layout\ClinicNavMenu.razor`
- Modify: `src\ClinicMateAI.Web\Components\Pages\Home.razor`
- Modify: `src\ClinicMateAI.Web\Components\Pages\Appointments.razor`
- Test: `tests\ClinicMateAI.Web.Tests\Branches\BranchesEndpointsTests.cs`
- Test: `tests\ClinicMateAI.Web.Tests\Dashboard\DashboardEndpointsTests.cs`
- Test: `tests\ClinicMateAI.Web.Tests\Layout\BranchSelectorTests.cs`

- [ ] **Step 1: Write the failing web and component tests**

```csharp
[Fact]
public async Task GetBranches_ReturnsEnterpriseBranches()
{
    await using var factory = new ClinicMateWebFactory();
    using var client = factory.CreateClient();

    var response = await client.GetAsync($"/api/branches?clinicId={clinicId}");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
}

[Fact]
public void BranchSelector_ShowsAllBranchesOption_ForOwner()
{
    using var ctx = new Bunit.TestContext();
    var cut = ctx.RenderComponent<BranchSelector>(parameters => parameters
        .Add(p => p.CanSeeAllBranches, true)
        .Add(p => p.Branches, [new AccessibleBranchDto(branchId, "สุขุมวิท")]));

    cut.Markup.Should().Contain("ทุกสาขา");
    cut.Markup.Should().Contain("สุขุมวิท");
}
```

- [ ] **Step 2: Run the web and component tests and confirm they fail**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Web.Tests\ClinicMateAI.Web.Tests.csproj --filter "FullyQualifiedName~BranchesEndpointsTests|FullyQualifiedName~DashboardEndpointsTests|FullyQualifiedName~BranchSelectorTests"
```

Expected: FAIL because the branch endpoints, dashboard endpoint, and selector component do not exist yet.

- [ ] **Step 3: Add branch endpoints and the owner dashboard query**

```csharp
endpoints.MapGet("/api/branches", async (
    Guid clinicId,
    IGetBranchesHandler handler,
    CancellationToken ct) =>
{
    var result = await handler.HandleAsync(new GetBranchesQuery(clinicId), ct);
    return Results.Ok(result);
});

endpoints.MapGet("/api/dashboard/owner", async (
    Guid clinicId,
    IGetOwnerDashboardHandler handler,
    CancellationToken ct) =>
{
    var result = await handler.HandleAsync(new GetOwnerDashboardQuery(clinicId), ct);
    return Results.Ok(result);
});
```

- [ ] **Step 4: Add the selector component and update the clinic layout and nav**

```razor
<header class="app-header">
    <div>
        <h1 class="app-title">@GetHeaderTitle()</h1>
        <div class="app-quota">@PackageSummary</div>
    </div>
    <BranchSelector />
</header>
```

```razor
@if (ActiveClinicPackage == PackageTier.Enterprise)
{
    <NavLink class="cm-nav-link" href="/clinic/branches">
        <span class="cm-nav-icon"><Heroicon Name="@HeroiconName.BuildingStorefront" Type="HeroiconType.Outline" /></span>
        <span class="cm-nav-text">จัดการสาขา</span>
    </NavLink>
}
```

- [ ] **Step 5: Update the dashboard and appointments page for branch context**

```razor
@if (branchContextState.ShowAllBranches)
{
    <section class="cm-panel">
        <header class="cm-panel-header">ภาพรวมทุกสาขา</header>
        @foreach (var branch in ownerDashboard.Branches)
        {
            <div class="cm-summary-row">@branch.BranchName - @branch.MessagesToday ข้อความ</div>
        }
    </section>
}
```

- [ ] **Step 6: Re-run the web and component tests**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Web.Tests\ClinicMateAI.Web.Tests.csproj --filter "FullyQualifiedName~BranchesEndpointsTests|FullyQualifiedName~DashboardEndpointsTests|FullyQualifiedName~BranchSelectorTests"
```

Expected: PASS.

- [ ] **Step 7: Commit**

```powershell
git add .\src\ClinicMateAI.Web\Endpoints\BranchesEndpoints.cs .\src\ClinicMateAI.Web\Endpoints\DashboardEndpoints.cs .\src\ClinicMateAI.Web\Services\BranchContextState.cs .\src\ClinicMateAI.Web\Components\Shared\BranchSelector.razor .\src\ClinicMateAI.Web\Components\Pages\Branches.razor .\src\ClinicMateAI.Web\Components\Pages\BranchCreate.razor .\src\ClinicMateAI.Web\Components\Pages\BranchSettings.razor .\src\ClinicMateAI.Application\Dashboard .\src\ClinicMateAI.Logic\Dashboard\GetOwnerDashboardHandler.cs .\src\ClinicMateAI.Web\Program.cs .\src\ClinicMateAI.Web\Components\Layout\ClinicLayout.razor .\src\ClinicMateAI.Web\Components\Layout\ClinicNavMenu.razor .\src\ClinicMateAI.Web\Components\Pages\Home.razor .\src\ClinicMateAI.Web\Components\Pages\Appointments.razor .\tests\ClinicMateAI.Web.Tests\Branches\BranchesEndpointsTests.cs .\tests\ClinicMateAI.Web.Tests\Dashboard\DashboardEndpointsTests.cs .\tests\ClinicMateAI.Web.Tests\Layout\BranchSelectorTests.cs
git commit -m "feat: add branch ui and owner dashboard"
```

## Task 10: Run end-to-end verification and clean up mismatches

**Files:**
- Modify: any touched file from Tasks 1-9 if verification reveals mismatches
- Verify: `Docs\superpowers\specs\2026-05-16-multi-branch-design.md`

- [ ] **Step 1: Run the domain/application test suite**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj
```

Expected: PASS.

- [ ] **Step 2: Run the web test suite**

Run:

```powershell
dotnet test .\tests\ClinicMateAI.Web.Tests\ClinicMateAI.Web.Tests.csproj
```

Expected: PASS.

- [ ] **Step 3: Run the full repository test command**

Run:

```powershell
dotnet test
```

Expected: PASS for the solution.

- [ ] **Step 4: Run the build**

Run:

```powershell
dotnet build
```

Expected: BUILD SUCCEEDED.

- [ ] **Step 5: Manual verification checklist**

```text
1. Create Starter clinic -> branch management hidden, second branch creation blocked.
2. Create Enterprise clinic -> create second branch succeeds.
3. Owner can switch between a branch and "ทุกสาขา".
4. Branch Admin sees only assigned branches.
5. LINE webhook routes message into the correct branch inbox.
6. Shared promotion shows on every branch; branch-only promotion shows only on its branch.
7. Dashboard totals change when switching between a single branch and all branches.
```

- [ ] **Step 6: Commit the integrated feature**

```powershell
git add .
git commit -m "feat: deliver multi-branch clinic support"
```

## Spec coverage check

- Package simplification to Starter + Enterprise: covered in Task 1.
- Enterprise per-branch pricing contract: covered in Task 1.
- Branch entity, branch-scoped services/promotions/channels/conversations: covered in Tasks 2-4 and 7-8.
- Owner sees all branches while staff see assigned branches: covered in Tasks 5-6 and 9.
- Shared clinic-wide AI tone/safety + branch-specific operational facts: covered in Tasks 7-8.
- Branch selector and all-branches dashboard: covered in Task 9.
- Starter hides branch management and enforces one branch: covered in Tasks 1, 5, and 9.
- Default branch creation and no production-data migration complexity: covered in Task 3.

