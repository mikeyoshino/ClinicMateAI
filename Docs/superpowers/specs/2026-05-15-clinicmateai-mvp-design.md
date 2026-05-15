# ClinicMateAI MVP Design

## Overview

ClinicMateAI is a working web app for Thai beauty and aesthetic clinics. It acts as an AI receptionist for LINE OA and Facebook Messenger while giving clinic staff a simple dashboard to manage clinic facts, promotions, bookings, safety rules, and handoff cases.

The first MVP focuses on one clinic type: beauty/aesthetic clinics. It must still use a multi-tenant design so more clinics, branches, and packages can be added later.

The implementation stack is Blazor, .NET, and C#.

## UI Design Reference

The MVP UI must follow the visual and interaction direction in `Docs/UIDesignIdea.html`.

Required UI direction:

- Build an app-first clinic operations dashboard, not a landing page.
- Use Thai-first interface labels and copy.
- Use the same practical layout: fixed left sidebar, top header, and scrollable main workspace.
- Use a restrained clinic admin palette based on white, slate, teal, rose alerts, amber drafts, and green success states.
- Use compact operational cards, tables, badges, and panels instead of decorative marketing sections.
- Use lucide-style icons for navigation, statuses, and actions.
- Use the Prompt font or a close Thai-friendly fallback.
- Keep the main navigation aligned with the design idea: Dashboard, Inbox, Appointments, Promotions, AI Test, Clinic Settings, Integrations, and Platform Admin.
- Match the UX patterns shown in the design idea:
  - dashboard metric cards and recent activity list,
  - three-column inbox with conversation list, message thread, and customer/safety panel,
  - AI Test simulator with customer chat preview and decision logs,
  - promotions management table with Published/Draft status badges.
- Convert the HTML/Tailwind prototype into Blazor components and local CSS. Do not depend on Tailwind CDN or external script CDNs in the production MVP.

## Goals

- Let a non-technical clinic set up the data the AI needs without writing prompts.
- Receive customer messages from LINE and Facebook through real webhook endpoints.
- Reply automatically when the answer is safe, approved, and high confidence.
- Create staff drafts or handoff tasks when the request is risky, low confidence, missing data, or medically sensitive.
- Support appointment booking from service duration, doctor availability, branch, and Google Calendar busy slots.
- Let clinics manage active promotions so AI can answer promotion questions accurately.
- Let Platform Admin manage packages, setup fees, and usage limits without exposing billing controls to clinic users in the MVP.

## Non-Goals For MVP

- Native mobile app.
- AI voice receptionist.
- Full CRM, accounting, inventory, or payroll.
- Live payment gateway settlement.
- Complex usage overage billing shown to clinics.
- Clinic users writing raw AI prompts.

## Roles

### Platform Admin

The product owner or internal operations team. Platform Admin can:

- Create and manage clinic tenants.
- Assign packages.
- Track usage and quota.
- Configure LINE, Facebook, Google Calendar, and future payment credentials.
- Help clinics set up AI behavior and safety rules.
- See integration health and webhook logs.

### Clinic Owner

The clinic business owner or manager. Clinic Owner can:

- Manage clinic profile, branches, services, promotions, booking rules, staff, AI settings, and approved FAQ.
- Approve and publish setup changes.
- View dashboard and operational reports.

### Clinic Admin

Clinic staff who handle daily operations. Clinic Admin can:

- Manage inbox and handoff.
- Approve, edit, and send AI draft replies.
- Manage appointments.
- Update service and promotion data if granted permission.

### Doctor / Specialist

Clinical specialist who can:

- View assigned appointments.
- View escalated cases.
- Contribute approved guidance for FAQ or aftercare content.

### Staff

Limited user who can:

- View appointments.
- Handle basic inbox or task actions if granted permission.

## Technology Stack

- Frontend: Blazor web app.
- Backend: ASP.NET Core APIs and webhook endpoints.
- Language: C#.
- Database: relational database through Entity Framework Core.
- Background jobs: .NET hosted services or a job library for reminders, follow-up, and scheduled tasks.
- AI provider: isolated behind an `IAiReceptionistService` style boundary.
- Calendar provider: isolated behind an `ICalendarService` style boundary, starting with Google Calendar.
- Messaging providers: isolated behind channel adapters for LINE and Facebook.

The MVP can use local development configuration and test inbox simulation before real credentials are available.

## Core Modules

### Clinic Setup Wizard

The setup wizard should feel like filling out clinic information, not configuring software.

Steps:

1. Clinic Profile
   - Clinic name, branch, address, map link, phone, business hours, parking, payment methods, LINE/Facebook connection status.

2. Services & Prices
   - Service name, category, price range, duration, related doctors, assessment requirement, preparation notes, aftercare notes, forbidden claims, and suggested AI wording.
   - Beauty examples: Botox jaw, filler, laser, facial treatment.

3. Promotions
   - Promotion name, related services, promo price, start date, end date, conditions, customer type, branch, deposit requirement, active status, and approved AI wording.
   - AI may only use active, published promotions.

4. Doctors & Availability
   - Doctor/specialist profile, branch, working days, working hours, services they can perform, appointment duration, and Google Calendar link.

5. Booking Rules
   - Deposit amount, cancellation/reschedule rules, walk-in policy, free consultation setting, reminder timing, confirmation message, and blocked times.

6. FAQ / Knowledge Base
   - Clinic-approved answers with statuses: Draft, Approved, Disabled.
   - AI can only use Approved and published answers.

7. Safety & Handoff Rules
   - Red flag keywords, cases that must go to staff, forbidden claims, default handoff message, and escalation behavior.

8. Test AI
   - Staff can test questions before publishing, such as:
     - "โบท็อกกรามเท่าไร"
     - "พรุ่งนี้หมอว่างไหม"
     - "ฟิลเลอร์บวมกี่วัน"
     - "ตอนนี้มีโปรอะไรบ้าง"

Setup changes start as draft. Owner or authorized admin clicks Approve & Publish before AI uses the changes.

### Unified Inbox

The inbox combines LINE and Facebook conversations.

Each conversation shows:

- Customer name.
- Channel.
- Last message.
- Current intent.
- AI reply status.
- Handoff state.
- Assigned staff.
- Appointment or lead status.

AI can:

- Reply automatically for safe, approved, high-confidence answers.
- Create a draft for staff approval.
- Escalate to staff immediately for red flags or sensitive cases.
- Summarize the conversation for staff on packages that allow it.

Staff can:

- Take over any conversation.
- Edit and approve AI draft replies.
- Mark a handoff case resolved.
- Create an appointment from the conversation.

### AI Receptionist Behavior

Default AI tone:

- Thai language.
- Address customer as "คุณลูกค้า".
- Use polite endings such as "ค่ะ" and "นะคะ".
- Service-minded, warm, concise, and helpful.
- Ask one useful follow-up question at a time.
- Gently guide customers toward booking when appropriate.
- Avoid hard selling.

Example service answer:

Customer: "โบท็อกกรามเท่าไรคะ"

AI: "โบท็อกกรามเริ่มต้นที่ 2,999 บาทค่ะคุณลูกค้า ราคาจะขึ้นอยู่กับยี่ห้อและจำนวนยูนิตที่เหมาะสม แนะนำให้คุณหมอประเมินก่อนเพื่อความปลอดภัยและผลลัพธ์ที่เหมาะกับแต่ละท่านนะคะ คุณลูกค้าเคยฉีดโบท็อกมาก่อนหรือยังคะ?"

AI must not:

- Invent prices, promotions, schedules, or medical facts.
- Diagnose disease.
- Prescribe or recommend medication.
- Guarantee results.
- Say a treatment is 100% safe or 100% effective.
- Confirm appointments without checking availability.

AI should escalate when:

- Clinic data is missing.
- AI confidence is low.
- Customer asks for diagnosis, medication, refund, or complaint handling.
- Message includes red flag terms such as แพ้, หายใจไม่ออก, บวมมาก, ปวดมาก, มีไข้, หนอง, ติดเชื้อ, หน้าชา, ตามัว, เลือดออก, ฟิลเลอร์ไหล, ฉีดแล้วเป็นก้อน.

Example red flag reply:

"อาการนี้ควรให้เจ้าหน้าที่หรือคุณหมอประเมินโดยตรงนะคะคุณลูกค้า เดี๋ยวส่งเรื่องให้แอดมินดูแลต่อทันทีค่ะ"

### Promotions

Promotions are a core MVP module.

Clinic can:

- Create, edit, disable, and publish promotions.
- Attach promotions to one or more services.
- Set promo validity dates and conditions.
- Define approved wording the AI can say.

AI can:

- Answer "มีโปรอะไรบ้าง" from active published promotions.
- Mention a matching promotion when a customer asks about a related service.
- Ask whether the customer wants help checking available appointment times.

AI must not:

- Mention expired promotions.
- Use draft or disabled promotions.
- Change promo conditions.
- Offer discounts not configured by the clinic.

### Appointments And Calendar

Booking uses:

- Clinic branch.
- Service duration.
- Doctor/specialist availability.
- Services each doctor can perform.
- Google Calendar busy slots.
- Booking rules and blocked times.

Customer can ask:

- "พรุ่งนี้หมอว่างกี่โมง"
- "วันเสาร์มีคิวไหม"
- "ขอเลื่อนคิวได้ไหม"

The AI only offers real available slots. Appointment records store:

- Customer.
- Channel.
- Service.
- Doctor.
- Branch.
- Date and time.
- Deposit status.
- Confirmation status.
- Reminder status.
- Reschedule/cancel status.

### Integrations

The MVP includes real integration boundaries even before credentials are available.

LINE:

- Webhook endpoint.
- Channel access token and secret configuration.
- Reply/push message adapter.
- Connection status screen.

Facebook Messenger:

- Webhook endpoint.
- Page token and app secret configuration.
- Reply adapter.
- Connection status screen.

Google Calendar:

- Calendar account configuration.
- Read busy slots.
- Create appointment event.
- Update/cancel event.

Credentials can be added later by Platform Admin.

### Packages And Billing

Billing is admin-only in the MVP. Clinic users do not see pricing controls or package assignment screens.

Packages:

#### Starter

- 1,990 THB/month.
- Setup fee: 3,000-5,000 THB.
- LINE OA AI receptionist.
- FAQ, prices, promotions.
- Basic booking.
- Google Calendar.
- Appointment reminders.
- Human handoff.
- Up to 20 services.
- 1 admin.
- 1,000 AI replies/month.

#### Growth

- 4,900 THB/month.
- Setup fee: 8,000-12,000 THB.
- LINE and Facebook Messenger.
- AI customer replies and lead screening.
- Booking, reschedule, and cancel.
- Up to 50 services.
- Unlimited promotions.
- Follow-up for leads who asked but did not book.
- Appointment reminders.
- Daily dashboard.
- Follow-up report.
- 3 admins.
- 3,000 AI replies/month.

#### Pro Clinic

- 9,900 THB/month.
- Setup fee: 15,000-25,000 THB.
- Everything in Growth.
- Multiple branches.
- Multiple admins.
- AI lead scoring.
- Multi-step follow-up sequence.
- Deposit/payment link support.
- Conversion report.
- Lead source tracking from LINE, Facebook, and ads.
- AI conversation summary for staff.
- 10 admins.
- 8,000 AI replies/month.

#### Enterprise

- 15,000+ THB/month.
- Setup fee: 30,000+ THB.
- Custom workflow.
- Multiple branches.
- Multiple pages.
- Multiple LINE OAs.
- Custom integrations.
- SLA support.
- Staff training.
- Custom dashboard.
- AI voice receptionist.

Usage limits:

- AI replies per month.
- Connected channels.
- Branch count.
- Admin seats.
- Services in knowledge base.
- Appointment volume later if needed.

Over quota behavior:

- MVP warns Platform Admin.
- Platform Admin can upgrade the clinic package.
- Clinic users are not shown complex overage billing.

Excluded third-party costs:

- LINE OA package.
- Facebook ads.
- SMS fees.
- Payment gateway fees.

### Message Processing Flow

1. Customer sends message on LINE or Facebook.
2. Webhook receives and stores the message.
3. Conversation and customer record are created or updated.
4. System detects intent and red flags.
5. System fetches relevant clinic facts: services, promotions, FAQ, booking rules, doctor availability, and customer context.
6. AI drafts a response using only approved and published clinic data.
7. Safety decision runs:
   - Safe and high confidence: send automatically.
   - Low confidence or missing data: create staff draft.
   - Red flag or sensitive case: escalate immediately.
8. Outcome is logged for inbox, analytics, and usage tracking.

### Testing Strategy

Automated tests should cover:

- Package limits and quota warnings.
- Promotion active/expired/draft/disabled logic.
- Red flag detection.
- AI auto-reply vs draft vs handoff decision logic.
- Booking slot selection.
- Doctor-service availability.
- Webhook receive and message persistence.
- Appointment create/update/cancel flow.

Manual MVP tests should cover:

- Thai service-minded reply style using "คุณลูกค้า".
- Service price answer.
- Active promotion answer.
- Expired promotion not mentioned.
- Doctor availability question.
- Booking confirmation.
- Low-confidence draft creation.
- Red flag escalation.
- LINE webhook test mode.
- Facebook webhook test mode.

## Implementation Decisions For MVP

- App model: Blazor Web App with interactive server rendering.
- API model: ASP.NET Core endpoints in the same solution for webhook and app APIs.
- Database provider: PostgreSQL for local MVP development through Entity Framework Core and Npgsql. Run only PostgreSQL in Docker via `docker compose`; run the Blazor/.NET app directly on the host with `dotnet run`. Keep EF Core abstractions clean so managed PostgreSQL or another relational provider can be adopted later.
- Authentication: ASP.NET Core Identity with role-based authorization for Platform Admin, Clinic Owner, Clinic Admin, Doctor/Specialist, and Staff.
- Background jobs: .NET hosted services for the first MVP, covering reminders, follow-up checks, and quota monitoring.
- AI provider: OpenAI-compatible service boundary behind an application interface. Store prompts and clinic context generation in application services, not in UI components.
- Local test mode: seed one demo beauty clinic with services, promotions, doctors, FAQs, and sample conversations so the app can be evaluated before external credentials are available.
