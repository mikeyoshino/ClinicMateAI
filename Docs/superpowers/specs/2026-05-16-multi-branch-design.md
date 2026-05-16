# Multi-Branch Support Design

## Overview

ClinicMateAI adds multi-branch support so a clinic chain (Enterprise) can manage multiple physical locations under one account while individual clinics (Starter) continue as single-branch operations. Each branch has its own channel config, services, doctors, promotions, and staff access. The clinic owner can see all branches at once. Branch staff see only their assigned branch.

## Package Restructure

The existing four packages (Starter, Growth, Pro Clinic, Enterprise) are reduced to two. Growth and Pro Clinic are merged into Starter since the app has no production data yet.

### Starter — 1,990 THB/month

- 1 branch only (cannot add more)
- LINE OA AI receptionist
- FAQ, services, prices, promotions
- Appointment booking + Google Calendar
- 1,000 AI replies/month
- 1 admin seat
- Up to 20 services
- Branch management UI hidden entirely

### Enterprise — 15,000+ THB/month base + fixed cost per additional branch

- First branch included in base price
- Each additional branch: fixed fee per branch per month (set by Platform Admin per clinic contract)
- LINE + Facebook, multiple channels per branch
- AI chatbot scoped per branch
- Cross-branch owner dashboard
- Unlimited admin seats
- Custom AI reply quota
- SLA support, staff training
- Full branch management UI

### Package Limits Update

| Package | MaxBranches | MaxAdminSeats | MaxAiReplies | MaxChannels | MaxServices |
|---|---|---|---|---|---|
| Starter | 1 | 1 | 1,000 | 1 | 20 |
| Enterprise | unlimited | unlimited | custom | unlimited | unlimited |

`PackageTier` enum: remove `Growth` and `ProClinic`, keep `Starter` and `Enterprise`.

## Branch Model

### Approach

Branch is a sub-entity of Clinic (Option A). It lives inside the Clinic aggregate boundary. Every clinic-owned record that is location-specific is associated with a `BranchId`.

### Shared at Clinic Level (all branches inherit)

- AI tone and personality
- Safety rules and red flag keywords
- Escalation rules and escalation message template
- Clinic brand name and identity

### Scoped at Branch Level (each branch configures independently)

- Branch name, address, phone, map URL, business hours
- LINE OA / Facebook Messenger channel config
- Services and prices
- Promotions (can also be marked as "all branches")
- Doctor and specialist profiles + availability schedules
- FAQ / knowledge base entries
- Booking rules (deposit, cancellation policy, blocked times)
- Google Calendar account

## Domain Changes

### New Entity: Branch

```
Branch
  Id: Guid
  ClinicId: Guid (FK → Clinic)
  Name: string
  Address: string
  Phone: string
  MapUrl: string
  BusinessHours: string (JSON or structured value object)
  Status: BranchStatus (Active | Inactive)
  IsDefault: bool
  CreatedAtUtc: DateTime
```

Every clinic gets exactly one default branch created automatically at onboarding or during migration. For Starter clinics this is the only branch and branch management is hidden.

### New Entity: UserBranchAssignment

```
UserBranchAssignment
  Id: Guid
  UserId: string (FK → AspNetUsers)
  BranchId: Guid (FK → Branch)
  ClinicId: Guid (FK → Clinic, denormalized for fast tenant queries)
  AssignedAtUtc: DateTime
```

A Clinic Owner does not need a UserBranchAssignment — their role gives them access to all branches. Branch Admin, Doctor, and Staff require at least one assignment to access any branch data. One user can be assigned to multiple branches.

### Modified Entities

- `ClinicChannelConfig` — add `BranchId: Guid` (non-nullable; each channel belongs to one branch)
- `Promotion` — add `BranchId: Guid?` (nullable; null means promotion applies to all branches)
- `ClinicService` — add `BranchId: Guid?` (nullable; null means service is offered at all branches)
- `DoctorAvailability` — `BranchId` already exists as nullable; make it non-nullable
- `Conversation` — add `BranchId: Guid` (set when webhook identifies the branch from ChannelConfig)
- `Appointment` — add `BranchId: Guid`

### New Enum: BranchStatus

```
BranchStatus
  Active = 1
  Inactive = 2
```

## Access Control

### Rules

- All clinic-owned queries must filter by `ClinicId` (existing tenant boundary rule).
- Branch-scoped queries must additionally filter by `BranchId` unless the caller has Clinic Owner role.
- Clinic Owner role: full access to all branches within their clinic.
- Branch Admin / Doctor / Staff: access restricted to branches listed in `UserBranchAssignment` for that user.
- Platform Admin: access to all clinics and all branches.

### Branch Context Resolution

The active branch for a request is resolved from:

1. Webhook path: resolved from `ClinicChannelConfig.BranchId` matched by channel credentials.
2. Authenticated UI user: resolved from the selected branch in the branch selector stored in the user's session/claim, validated against `UserBranchAssignment`.
3. Clinic Owner selecting "All Branches": no branch filter applied, returns aggregate data.

### New Application Abstraction: IBranchAccessPolicy

```csharp
// Application/Abstractions/Auth/IBranchAccessPolicy.cs
public interface IBranchAccessPolicy
{
    Task<IReadOnlyList<Guid>> GetAccessibleBranchIdsAsync(string userId, Guid clinicId);
    Task<bool> CanAccessBranchAsync(string userId, Guid branchId);
}
```

Logic layer implements this using `UserBranchAssignment` repository and role checks.

## AI Context per Branch

When the AI processes a message:

1. `ReceiveMessageHandler` identifies the branch from the matched `ClinicChannelConfig`.
2. `AiReceptionistOrchestrator` fetches clinic-level config (tone, safety rules) AND branch-level data (services, promotions for this branch or all-branch promotions, doctors in this branch, FAQ for this branch, booking rules for this branch).
3. AI reply is generated using only the branch-scoped data.
4. Conversation and message are stored with `BranchId`.

Promotion scoping rule for AI:
- Include promotions where `BranchId == currentBranchId` OR `BranchId == null` (all-branches promo).
- Never include promotions from another branch.

## Branch Selector UI

A branch selector component appears in the clinic layout header for users with access to more than one branch.

- Clinic Owner: sees "ทุกสาขา" (All Branches) option + individual branch options.
- Branch Admin / Doctor / Staff: sees only their assigned branches (no "All Branches" option).
- Selecting a branch stores the active branch in the user's Blazor session state.
- All subsequent queries are scoped to the selected branch.
- Selecting "ทุกสาขา" shows aggregate cross-branch data on Dashboard; other screens default to the first branch.

Component: `BranchSelector.razor` in `Web/Components/Shared`.

## Cross-Branch Dashboard (Owner)

When a Clinic Owner selects "ทุกสาขา", the dashboard shows:

- Total new messages today (all branches)
- Total appointments today (all branches)
- AI replies used this month (all branches, vs quota)
- Per-branch summary row: branch name, messages today, appointments today, AI replies this month
- Recent activity feed across all branches

Individual branch selection shows existing single-branch dashboard metrics.

## Branch Management UI

Available only for Enterprise package. Hidden for Starter.

Routes:
- `/clinic/branches` — list all branches
- `/clinic/branches/create` — create new branch
- `/clinic/branches/{id}/settings` — edit branch profile, hours, status
- `/clinic/branches/{id}/staff` — manage UserBranchAssignment for this branch

Branch management is accessible only to Clinic Owner.

## Package Limit Enforcement

When a Clinic Owner on Enterprise tries to add a branch:

1. Logic checks current branch count against `PackageLimit.MaxBranches`.
2. For Enterprise, `MaxBranches` is `int.MaxValue` (no hard cap in code).
3. Platform Admin configures the per-branch fee in the clinic record; quota tracking is informational.
4. Warning to Platform Admin when branch count changes (for billing awareness).

Starter package: "Add Branch" button is hidden in UI. If attempted via API, handler returns `BusinessException` with `BranchLimitExceeded` error code.

## Migration

Since the app is in development with no production data:

1. Remove `Growth` and `ProClinic` from `PackageTier` enum.
2. Update `PackageLimitService` to handle only `Starter` and `Enterprise`.
3. Add `Branch` table migration.
4. Add `UserBranchAssignment` table migration.
5. Add `BranchId` columns to `ClinicChannelConfig`, `Conversation`, `Appointment`, `DoctorAvailability`.
6. Add `BranchId` nullable columns to `Promotion` and `ClinicService`.
7. Update seed data (`DemoDataSeeder`) to create a default branch for the demo clinic and associate all seeded data with it.

No data migration scripts needed beyond seed data updates.

## Repository Changes

### New: IBranchRepository

```csharp
// Application/Abstractions/Persistence/IBranchRepository.cs
Task<Branch?> GetByIdAsync(Guid id, Guid clinicId);
Task<IReadOnlyList<Branch>> GetByClinicIdAsync(Guid clinicId);
Task<Branch?> GetDefaultBranchAsync(Guid clinicId);
Task AddAsync(Branch branch);
Task<bool> ExistsByNameAsync(Guid clinicId, string name);
```

### New: IUserBranchAssignmentRepository

```csharp
Task<IReadOnlyList<Guid>> GetBranchIdsForUserAsync(string userId, Guid clinicId);
Task AssignAsync(UserBranchAssignment assignment);
Task RemoveAsync(string userId, Guid branchId);
Task<bool> IsAssignedAsync(string userId, Guid branchId);
```

### Updated Repositories

All existing repositories that return clinic-scoped data must add optional `branchId` filtering:
- `IConversationRepository` — filter conversations by branchId
- `IPromotionRepository` — filter promotions by branchId (include null-branchId promotions)
- `IClinicServiceRepository` — filter services by branchId (include null-branchId services)
- `IClinicChannelConfigRepository` — look up config by branchId
- Appointment repository (when created) — filter by branchId

## New Application Commands and Queries

### Branch Management
- `CreateBranchCommand` / `ICreateBranchHandler`
- `UpdateBranchCommand` / `IUpdateBranchHandler`
- `DeactivateBranchCommand` / `IDeactivateBranchHandler`
- `GetBranchesQuery` / `IGetBranchesHandler`
- `GetBranchDetailQuery` / `IGetBranchDetailHandler`

### User Branch Assignment
- `AssignUserToBranchCommand` / `IAssignUserToBranchHandler`
- `RemoveUserFromBranchCommand` / `IRemoveUserFromBranchHandler`
- `GetBranchStaffQuery` / `IGetBranchStaffHandler`

### Cross-Branch Dashboard
- `GetOwnerDashboardQuery` / `IGetOwnerDashboardHandler` — returns per-branch summary rows + totals

## Testing Requirements

New tests required:

- Branch limit enforcement: Starter cannot exceed 1 branch.
- Enterprise branch creation succeeds regardless of count.
- UserBranchAssignment: Branch Admin cannot read data from unassigned branch.
- Clinic Owner can read data from all branches without assignment.
- AI promotion scoping: only branch-matching or null-branch promotions included.
- Webhook branch resolution: correct branch identified from ChannelConfig credentials.
- Cross-branch dashboard: totals aggregate correctly across branches.
- Branch deactivation: inactive branch data not returned to AI or inbox.

## Implementation Phases

### Phase 1 — Foundation (Domain + Infrastructure)
Domain entities, enums, repository interfaces, EF migrations, seed data update, package tier cleanup.

### Phase 2 — Logic + Access Control
Handlers for branch CRUD, user branch assignment, branch access policy, update existing handlers to filter by branch, AI orchestrator branch context.

### Phase 3 — Web + UI
Branch selector component, branch management pages, updated inbox/appointments/promotions with branch filtering, cross-branch dashboard, hide branch UI for Starter.

### Phase 4 — Tests
Unit tests for all new domain rules, integration tests for branch-scoped repositories.
