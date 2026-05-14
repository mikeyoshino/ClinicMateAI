# ClinicMateAI Agent Instructions

These instructions apply to all future coding agents working in this repository.

## Project Context

ClinicMateAI is a Blazor/.NET/C# web app for Thai beauty and aesthetic clinics. The MVP is an AI receptionist and clinic operations dashboard that supports:

- LINE OA and Facebook Messenger webhook-ready messaging.
- A unified inbox.
- AI auto-replies, staff drafts, and handoff escalation.
- Clinic-managed services, prices, promotions, FAQ, booking rules, safety rules, and doctor availability.
- Google Calendar-based availability integration.
- Admin-only package and quota management.

Read these files before implementation work:

- `Docs/ProjectDetailDemo.md` - original product concept.
- `Docs/UIDesignIdea.html` - required UI/UX visual reference.
- `docs/superpowers/specs/2026-05-15-clinicmateai-mvp-design.md` - approved design spec.
- `docs/superpowers/plans/2026-05-15-clinicmateai-mvp.md` - implementation checklist.
- `docs/superpowers/plans/2026-05-15-clinicmateai-mvp-th.md` - Thai implementation summary.

## Architecture Rules

Use Clean Architecture boundaries:

- `Domain` contains entities, enums, value objects, and pure domain rules only.
- `Application` contains use cases, DTOs, service interfaces, orchestration, validation, and business workflows.
- `Infrastructure` contains EF Core, SQLite, repositories, seed data, provider implementations, and external adapters.
- `Web` contains Blazor UI, ASP.NET Core endpoints, authentication, dependency injection, and presentation concerns.
- Tests live under `tests/`.

Dependency direction must be:

`Web -> Infrastructure -> Application -> Domain`

`Application -> Domain`

Never make `Domain` depend on EF Core, ASP.NET Core, Blazor, OpenAI, LINE, Facebook, Google Calendar, or any infrastructure package.

## SOLID Principles

Follow SOLID pragmatically:

- Single Responsibility: one class/component should have one clear reason to change.
- Open/Closed: add new providers or workflows behind interfaces instead of modifying unrelated code.
- Liskov Substitution: provider implementations must honor their interface contracts.
- Interface Segregation: prefer small interfaces such as `IAiReplyProvider`, `ICalendarService`, and `IMessageChannelAdapter`.
- Dependency Inversion: application services depend on abstractions, not concrete infrastructure providers.

Avoid service classes that become generic dumping grounds. If a service grows too broad, split it by use case.

## MVP Scope Discipline

Build the working MVP first. Do not add these unless explicitly requested:

- Native mobile app.
- AI voice receptionist.
- Real payment settlement.
- Inventory, accounting, payroll, or full CRM.
- Complex usage overage billing visible to clinics.
- Clinic-user raw prompt editing.

The MVP should run locally with demo data and simulated providers before real external credentials exist.

## Blazor And UI Rules

Use `Docs/UIDesignIdea.html` as the required visual reference.

UI requirements:

- Thai-first interface labels and copy.
- App-first operations dashboard, not a marketing landing page.
- Fixed left sidebar, top header, scrollable main workspace.
- White/slate/teal visual system, with rose for alerts, amber for drafts, and green for success.
- Compact cards, tables, badges, and operational panels.
- Dashboard metric cards and recent activity list.
- Three-column inbox: conversation list, message thread, customer/safety panel.
- AI Test simulator with customer chat preview and decision logs.
- Promotions table with Published/Draft/Disabled status badges.

Implementation requirements:

- Convert the HTML prototype to Blazor components and local CSS.
- Do not depend on Tailwind CDN, Google Fonts CDN, or external script CDNs in production code.
- Keep reusable UI parts in focused components such as metric tiles, badges, conversation list, conversation thread, and safety panels.
- Text must fit in cards/buttons across desktop and mobile.
- Avoid nested cards and decorative marketing sections.

## AI Safety Rules

The AI must be service-minded and Thai-first:

- Address customers as `คุณลูกค้า`.
- Use polite endings like `ค่ะ` and `นะคะ`.
- Be warm, concise, helpful, and not hard-selling.
- Ask one useful follow-up question at a time.

The AI must never:

- Invent prices, promotions, schedules, or medical facts.
- Diagnose disease.
- Prescribe or recommend medication.
- Guarantee results.
- Say treatment is 100% safe or 100% effective.
- Confirm appointments without checking availability.

Escalate immediately for red flags such as:

- `แพ้`
- `หายใจไม่ออก`
- `บวมมาก`
- `ปวดมาก`
- `มีไข้`
- `หนอง`
- `ติดเชื้อ`
- `หน้าชา`
- `ตามัว`
- `เลือดออก`
- `ฟิลเลอร์ไหล`
- `ฉีดแล้วเป็นก้อน`
- `ขอคืนเงิน`
- `ร้องเรียน`

Use this escalation style:

`อาการนี้ควรให้เจ้าหน้าที่หรือคุณหมอประเมินโดยตรงนะคะคุณลูกค้า เดี๋ยวส่งเรื่องให้แอดมินดูแลต่อทันทีค่ะ`

## Data And Tenant Rules

Design for multi-tenant use from the start:

- Every clinic-owned record must have a `ClinicId` unless it is truly global platform data.
- Never mix data across clinics.
- Packages and billing controls are Platform Admin only in the MVP.
- Clinic users can manage business data such as services, prices, promotions, FAQ, booking rules, and safety settings.
- AI can only use published/approved clinic data.

Promotion rules:

- AI may use only Published promotions within active date range.
- Draft, Disabled, and expired promotions must not be used in AI replies.
- AI must not invent discounts or change promo conditions.

Booking rules:

- Use doctor availability, service duration, branch, and calendar busy slots.
- AI may only offer real available slots.
- Google Calendar integration should be behind an application interface.

## Integration Rules

External systems must be behind adapters/interfaces:

- LINE OA messaging adapter.
- Facebook Messenger adapter.
- Google Calendar adapter.
- AI provider adapter.
- Future payment adapter.

Webhook endpoints should be real and testable even before production credentials are available. Local/test mode should accept simple JSON payloads and store conversations/messages.

Do not hardcode secrets. Use configuration, user secrets, environment variables, or secure storage.

## Testing Rules

Use TDD for domain and application rules.

Required test coverage:

- Package limits and quota warnings.
- Promotion active/expired/draft/disabled logic.
- Red flag detection.
- AI auto-reply vs draft vs handoff decision.
- Thai service-minded reply containing `คุณลูกค้า`.
- Booking slot selection.
- Doctor-service availability.
- Webhook receive and message persistence.
- Appointment create/update/cancel flow.

Run relevant tests after each focused change. Before claiming completion, run:

```powershell
dotnet test
dotnet build
```

If a command cannot run, state exactly why.

## Code Style

- Prefer small, focused files.
- Use explicit names that reflect clinic domain language.
- Keep business rules out of Razor components.
- Keep UI state and presentation in components; keep decisions in application services.
- Use async APIs for database and external-provider operations.
- Avoid static helpers except for pure domain/application rules where dependency injection is not needed.
- Avoid premature abstractions, but introduce interfaces for external providers and cross-boundary services.
- Do not swallow exceptions silently. Return useful errors or log at the boundary.

## Workflow

Use the implementation plan as the progress tracker:

- `[ ]` means not done.
- `[x]` means done.

Only mark a step done after its verification command passes or after a documented manual check.

The workspace may not be a git repository yet. If git is later initialized, make small commits at milestone boundaries or after coherent tasks.

