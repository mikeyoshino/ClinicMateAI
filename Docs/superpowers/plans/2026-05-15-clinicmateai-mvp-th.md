# แผนพัฒนา ClinicMateAI MVP

**เป้าหมาย:** สร้างเว็บแอป ClinicMateAI เวอร์ชันแรกด้วย Blazor, .NET และ C# สำหรับคลินิกเสริมความงาม โดยมีระบบตั้งค่าคลินิก, Inbox รวม LINE/Facebook, AI receptionist, โปรโมชัน, จองคิว, ตารางหมอ, แพ็กเกจแบบ admin-only และ webhook ที่พร้อมเชื่อมต่อจริงเมื่อมี credentials

**แนวทางสถาปัตยกรรม:** ใช้ .NET solution แยกชั้นงานชัดเจน ได้แก่ Domain, Application, Infrastructure, Web และ Tests เพื่อไม่ให้ business logic ไปปนอยู่ในหน้า UI โดยตรง เว็บหลักเป็น Blazor Web App แบบ interactive server rendering และ backend ใช้ ASP.NET Core endpoints สำหรับ webhook/API

**Tech Stack:** Blazor Web App, ASP.NET Core, C#, Entity Framework Core, SQLite สำหรับ local MVP, ASP.NET Core Identity, xUnit, bUnit, FluentAssertions

**UI Reference:** ให้ใช้ `Docs/UIDesignIdea.html` เป็นแนวทางหลักของหน้าตาและ interaction ของแอป โดยแปลงจาก HTML prototype ไปเป็น Blazor components และ CSS ภายในโปรเจกต์ ห้ามพึ่ง Tailwind CDN, Google Fonts CDN หรือ external script CDN ใน production MVP

แนวทาง UI ที่ต้องยึด:

- เป็น dashboard สำหรับใช้งานจริง ไม่ใช่ landing page
- ใช้ภาษาไทยเป็นหลัก
- layout หลักเป็น sidebar ซ้าย, header บน, content area ตรงกลาง
- โทนสี white/slate/teal พร้อม rose สำหรับ alert, amber สำหรับ draft, green สำหรับ success
- ใช้ card, table, badge, panel แบบกระชับ เหมาะกับงานแอดมินคลินิก
- ใช้ icon แนว lucide สำหรับเมนู สถานะ และปุ่ม action
- ใช้ฟอนต์ Prompt หรือ fallback ที่อ่านภาษาไทยดี
- ต้องทำ pattern สำคัญจากไฟล์ design: dashboard metric cards, recent activities, inbox 3 columns, AI Test simulator, promotions table

---

## โครงสร้างโปรเจกต์

- `ClinicMateAI.sln` - solution หลัก
- `src/ClinicMateAI.Domain/` - entity, enum, domain rule ที่ไม่ผูกกับ database หรือ UI
- `src/ClinicMateAI.Application/` - use case, service interface, AI orchestration, booking logic, package logic
- `src/ClinicMateAI.Infrastructure/` - EF Core, SQLite, seed data, provider จำลอง, adapter สำหรับ LINE/Facebook/Calendar
- `src/ClinicMateAI.Web/` - Blazor UI, Identity, webhook endpoints, dependency injection
- `tests/ClinicMateAI.Tests/` - unit tests สำหรับ domain/application
- `tests/ClinicMateAI.Web.Tests/` - component/endpoint tests

ไฟล์สำคัญ:

- `Clinic.cs` - ข้อมูลคลินิก
- `ClinicPackage.cs` - แพ็กเกจและ usage ของคลินิก
- `ClinicService.cs` - รายการบริการและราคา
- `Promotion.cs` - โปรโมชันและกฎ active/published
- `DoctorAvailability.cs` - ตารางหมอและเวลาว่าง
- `Conversation.cs` / `Message.cs` - แชตลูกค้า
- `AiSafetyDecision.cs` - ผลตัดสินใจว่าให้ AI ตอบเอง, ทำ draft, หรือส่งต่อแอดมิน
- `AiReceptionistOrchestrator.cs` - workflow สร้างคำตอบ AI
- `RedFlagDetector.cs` - ตรวจ keyword เสี่ยงภาษาไทย
- `AvailabilityService.cs` - คำนวณเวลาว่างสำหรับจองคิว
- `PackageLimitService.cs` - กฎ quota ของแพ็กเกจ
- `WebhookEndpoints.cs` - endpoint สำหรับ LINE/Facebook
- `clinicmate-theme.css` - CSS ภายในโปรเจกต์ที่ถอด visual system จาก `Docs/UIDesignIdea.html`
- `AppIcon.razor` - wrapper/helper สำหรับ icon ให้ใช้สม่ำเสมอใน UI

---

## Milestone 1: ตั้งค่า Solution และ Test Foundation

### Task 1: สร้าง solution และ project

สิ่งที่ต้องทำ:

1. สร้าง `.sln`
2. สร้าง project:
   - Domain
   - Application
   - Infrastructure
   - Web
   - Unit Tests
   - Web Tests
3. เพิ่ม project references ให้ถูกต้อง:
   - Application อ้างถึง Domain
   - Infrastructure อ้างถึง Application และ Domain
   - Web อ้างถึง Application และ Infrastructure
   - Tests อ้างถึง project ที่ต้องทดสอบ
4. เพิ่ม NuGet packages:
   - `Microsoft.EntityFrameworkCore.Sqlite`
   - `Microsoft.EntityFrameworkCore.Design`
   - `FluentAssertions`
   - `bunit`
5. รัน `dotnet build` ให้ผ่าน

ผลลัพธ์ที่ต้องได้:

- โครงสร้าง solution พร้อมเริ่มเขียน domain/application/UI
- build ผ่านก่อนเริ่ม feature

---

## Milestone 2: Domain Model และ Business Rules

### Task 2: ระบบแพ็กเกจและ usage limits

แพ็กเกจที่ต้องรองรับ:

- Starter
- Growth
- Pro Clinic
- Enterprise

กฎ quota:

- Starter: 1,000 AI replies/month, 20 services, 1 admin, 1 channel, 1 branch
- Growth: 3,000 AI replies/month, 50 services, 3 admins, 2 channels, 1 branch
- Pro Clinic: 8,000 AI replies/month, unlimited services, 10 admins, 3 channels, unlimited branches
- Enterprise: unlimited ตามการตั้งค่าของ Platform Admin

สิ่งที่ต้องทดสอบ:

- ดึง quota ของแต่ละแพ็กเกจได้ถูกต้อง
- ถ้าใช้ AI replies เกิน quota ต้องแจ้งว่าเกิน

ไฟล์หลัก:

- `PackageTier.cs`
- `PackageLimit.cs`
- `PackageLimitService.cs`
- `PackageLimitServiceTests.cs`

### Task 3: ระบบโปรโมชัน

โปรโมชันต้องมีสถานะ:

- Draft
- Published
- Disabled

AI ใช้โปรโมชันได้เฉพาะเมื่อ:

- status เป็น Published
- วันที่ปัจจุบันอยู่ในช่วง start/end date
- โปรโมชันผูกกับบริการที่เกี่ยวข้อง

AI ห้ามใช้:

- โปรหมดอายุ
- โปรที่ยังเป็น Draft
- โปรที่ Disabled

สิ่งที่ต้องทดสอบ:

- Published และ active ใช้ได้
- Draft ใช้ไม่ได้
- Disabled ใช้ไม่ได้
- Expired ใช้ไม่ได้

ไฟล์หลัก:

- `PromotionStatus.cs`
- `Promotion.cs`
- `PromotionTests.cs`

### Task 4: AI Safety และ Red Flag Detection

AI reply mode:

- `AutoReply` - ตอบลูกค้าอัตโนมัติ
- `DraftForStaff` - ทำ draft ให้แอดมินตรวจ
- `Escalate` - ส่งต่อแอดมินทันที

Red flag ภาษาไทย:

- แพ้
- หายใจไม่ออก
- บวมมาก
- ปวดมาก
- มีไข้
- หนอง
- ติดเชื้อ
- หน้าชา
- ตามัว
- เลือดออก
- ฟิลเลอร์ไหล
- ฉีดแล้วเป็นก้อน
- ขอคืนเงิน
- ร้องเรียน

กฎตัดสินใจ:

- ถ้ามี red flag ให้ `Escalate`
- ถ้าไม่มี approved clinic data ให้ `DraftForStaff`
- ถ้า confidence ต่ำกว่า 0.70 ให้ `DraftForStaff`
- ถ้าปลอดภัยและ confidence สูง ให้ `AutoReply`

ไฟล์หลัก:

- `AiReplyMode.cs`
- `AiSafetyDecision.cs`
- `RedFlagDetector.cs`
- `AiSafetyDecider.cs`
- `AiSafetyDeciderTests.cs`

### Task 5: Appointment Availability

ระบบต้องคำนวณเวลาว่างจาก:

- วันและเวลาทำงานของหมอ
- slot duration
- busy slots จาก Google Calendar หรือ provider จำลอง
- service duration

สิ่งที่ต้องทดสอบ:

- ถ้าหมอว่าง 13:00-16:00 และ slot 60 นาที แต่ 14:00-15:00 ไม่ว่าง ระบบต้องคืน 13:00 และ 15:00
- ถ้าวันไม่ตรงกับวันที่หมอทำงาน ต้องไม่มี slot

ไฟล์หลัก:

- `TimeRange.cs`
- `DoctorAvailability.cs`
- `AvailabilityService.cs`
- `AvailabilityServiceTests.cs`

---

## Milestone 3: Database และ Demo Data

### Task 6: EF Core และ Seed Data

ใช้ SQLite สำหรับ MVP local development

Entity สำคัญ:

- Clinic
- ClinicService
- Promotion
- Conversation
- Message
- Appointment
- DoctorAvailability
- IntegrationConnection

Demo clinic:

- ชื่อ: Demo Aesthetic Clinic
- บริการ: Botox Jaw
- ราคาเริ่มต้น: 2,999 บาท
- โปร: Botox Jaw New Customer
- โปร active ช่วงเดือนพฤษภาคม 2026
- AI wording ใช้คำว่า “คุณลูกค้า”

สิ่งที่ต้องทดสอบ:

- seed แล้วมี clinic demo
- seed แล้วมี service Botox Jaw
- seed แล้วมี promotion ที่ Published

ไฟล์หลัก:

- `AppDbContext.cs`
- `DemoDataSeeder.cs`
- `DemoDataSeederTests.cs`

---

## Milestone 4: AI Orchestration และ Messaging

### Task 7: AI Receptionist Orchestrator

หน้าที่:

1. รับข้อความลูกค้า
2. ตรวจ red flag
3. ตรวจว่ามี approved data หรือไม่
4. ตัดสินใจ AutoReply/Draft/Escalate
5. สร้างข้อความตอบกลับภาษาไทย
6. ใช้โทน service mind และเรียกลูกค้าว่า “คุณลูกค้า”

ตัวอย่างคำตอบ:

> โบท็อกกรามเริ่มต้นที่ 2,999 บาทค่ะคุณลูกค้า ราคาจะขึ้นอยู่กับยี่ห้อและจำนวนยูนิตที่เหมาะสม แนะนำให้คุณหมอประเมินก่อนนะคะ คุณลูกค้าเคยฉีดโบท็อกมาก่อนหรือยังคะ?

ข้อความ escalate:

> อาการนี้ควรให้เจ้าหน้าที่หรือคุณหมอประเมินโดยตรงนะคะคุณลูกค้า เดี๋ยวส่งเรื่องให้แอดมินดูแลต่อทันทีค่ะ

ไฟล์หลัก:

- `AiReplyRequest.cs`
- `AiReplyResult.cs`
- `IAiReplyProvider.cs`
- `SimulatedAiReplyProvider.cs`
- `AiReceptionistOrchestrator.cs`
- `AiReceptionistOrchestratorTests.cs`

### Task 8: LINE/Facebook Webhook Skeleton

endpoint ที่ต้องมี:

- `POST /webhooks/line`
- `POST /webhooks/facebook`

local test body:

```json
{
  "clinicId": "11111111-1111-1111-1111-111111111111",
  "customerName": "คุณลูกค้า Demo",
  "text": "โบท็อกกรามเท่าไรคะ"
}
```

response:

```json
{
  "received": true
}
```

handler ต้อง:

- สร้างหรือหา conversation
- บันทึก customer message
- เรียก AI orchestrator
- บันทึก AI reply หรือ draft/handoff
- นับ usage ถ้ามี AI reply

ไฟล์หลัก:

- `WebhookEndpoints.cs`
- `ReceiveMessageCommand.cs`
- `ReceiveMessageHandler.cs`
- `WebhookEndpointTests.cs`

---

## Milestone 5: Blazor MVP UI

### Task 9: Shell Navigation และ Dashboard

เมนูหลัก:

- Dashboard
- Inbox
- Appointments
- Setup
- Services
- Promotions
- AI Settings
- Integrations
- Platform Admin

Dashboard ต้องแสดง:

- ลูกค้าใหม่วันนี้
- AI ตอบเอง
- ส่งต่อแอดมิน
- จองคิวสำเร็จ
- ลูกค้าที่ควร follow-up

UI ควรเป็นแบบใช้งานจริง ไม่ใช่ landing page

ต้องยึด `Docs/UIDesignIdea.html`:

- sidebar สีขาวพร้อมโลโก้ ClinicMateAI
- active menu สี teal
- badge จำนวนข้อความใน Inbox
- header แสดงชื่อหน้าปัจจุบัน
- package usage chip เช่น Pro Clinic (AI: 450/8000)
- metric cards สำหรับลูกค้าใหม่, AI ตอบเอง, escalate, จองคิวสำเร็จ
- recent activity list พร้อมสี rose/green/teal ตามสถานะ

ไฟล์หลัก:

- `NavMenu.razor`
- `Dashboard.razor`
- `MetricTile.razor`
- `clinicmate-theme.css`
- `DashboardTests.cs`

### Task 10: Setup Wizard, Services, Promotions

Setup wizard แสดง card:

- Clinic Profile
- Services
- Promotions
- Doctors & Availability
- Booking Rules
- FAQ
- Safety Rules
- Test AI

หน้า Promotions ต้องให้คลินิกจัดการ:

- ชื่อโปร
- บริการที่เกี่ยวข้อง
- ราคาโปร
- วันเริ่ม
- วันหมดอายุ
- เงื่อนไข
- AI wording ที่อนุมัติแล้ว
- status

ปุ่ม:

- Save Draft
- Publish
- Disable

ไฟล์หลัก:

- `SetupWizard.razor`
- `Services.razor`
- `Promotions.razor`
- `PromotionService.cs`
- `PromotionServiceTests.cs`

### Task 11: Inbox และ AI Test Screen

Inbox layout:

- ซ้าย: รายการ conversation
- กลาง: thread แชต
- ขวา: customer/safety panel

ต้องยึด `Docs/UIDesignIdea.html`:

- conversation list ซ้าย มี search และ channel badge LINE/Facebook
- chat thread กลาง มี bubble ลูกค้าและ AI
- safety/customer panel ขวา
- rose สำหรับ red flag/handoff
- teal สำหรับ AI ตอบแล้ว
- amber สำหรับ draft

Inbox ต้องแสดง:

- channel badge: LINE/Facebook
- ชื่อลูกค้า
- ข้อความล่าสุด
- AI reply status
- handoff state
- ปุ่ม approve draft เมื่อ AI ทำ draft

AI Test screen:

- ใส่ข้อความทดสอบ
- แสดง reply mode
- แสดงข้อความ AI
- แสดง data source ที่ใช้
- แสดงว่าจะ auto-send หรือไม่

ต้องมีรูปแบบใกล้เคียง design:

- phone-style customer simulator
- AI Safety Decision Engine logs
- confidence score
- reasoning/เหตุผลที่ตัดสินใจ
- red flag keyword display

ไฟล์หลัก:

- `Inbox.razor`
- `AiTest.razor`
- `ConversationList.razor`
- `ConversationThread.razor`
- `InboxTests.cs`

### Task 12: Appointments และ Doctors

Appointments page ต้องแสดง:

- นัดวันนี้
- บริการ
- หมอ
- ลูกค้า/channel
- สถานะนัด
- deposit status
- reminder status

Doctors page ต้องให้ตั้งค่า:

- ชื่อหมอ
- สาขา
- วันทำงาน
- เวลาทำงาน
- บริการที่ทำได้
- slot duration
- Google Calendar connection label

ไฟล์หลัก:

- `Appointments.razor`
- `Doctors.razor`
- `AppointmentService.cs`
- `AppointmentServiceTests.cs`

---

## Milestone 6: Platform Admin, Integrations และ Verification

### Task 13: Platform Admin Package Assignment

หน้านี้เห็นเฉพาะ Platform Admin

ต้องแสดง:

- รายชื่อคลินิก
- package ปัจจุบัน
- AI usage เดือนนี้
- service count
- admin seat count
- warning เมื่อเกิน quota

ต้องทำได้:

- assign package
- ดู quota หลังเปลี่ยน package

ไฟล์หลัก:

- `PlatformAdmin/Clinics.razor`
- `ClinicAdminService.cs`
- `PackageAssignmentTests.cs`

### Task 14: Integrations Screen

หน้าจอ Integrations ต้องแสดง:

- LINE OA: connected/not connected
- Facebook Messenger: connected/not connected
- Google Calendar: connected/not connected

Credential input ให้ Platform Admin เห็นเท่านั้น

ข้อมูล connection:

- ClinicId
- Provider
- IsConnected
- DisplayName
- LastCheckedAtUtc
- MaskedCredentialLabel

ไฟล์หลัก:

- `Integrations.razor`
- `IntegrationSettingsService.cs`
- `IntegrationConnection.cs`

### Task 15: End-to-End Local Verification

ต้องตรวจ:

1. `dotnet test` ผ่านทั้งหมด
2. `dotnet run --project src/ClinicMateAI.Web` เปิดแอปได้
3. Dashboard โหลดได้
4. Setup wizard โหลดได้
5. Promotions แสดง demo Botox promo
6. AI Test ถาม “โบท็อกกรามเท่าไรคะ” แล้วตอบด้วย “คุณลูกค้า”
7. AI Test ถาม “ฉีดแล้วบวมมาก” แล้วเป็น escalation
8. Inbox แสดง simulated LINE/Facebook message
9. Appointments แสดง availability
10. Platform Admin แสดง package/usage
11. Integrations แสดง LINE/Facebook/Google Calendar เป็น not connected

README ต้องมี:

- วิธี run app
- demo login accounts
- LINE/Facebook credentials ยังไม่จำเป็นใน local test mode
- ตำแหน่งที่จะใส่ credentials เมื่อพร้อม

---

## ลำดับการทำงานที่แนะนำ

1. สร้าง solution และ test foundation
2. ทำ domain rules พร้อม tests ก่อน
3. ทำ EF Core และ demo data
4. ทำ AI orchestration แบบ simulated provider
5. ทำ webhook skeleton
6. ทำ Blazor UI ทีละ module
7. ทำ Platform Admin และ Integrations
8. รัน full verification

## ขอบเขต MVP ที่ต้องรักษา

ทำในเวอร์ชันแรก:

- working local app
- demo beauty clinic
- setup UI
- promotion management
- AI test
- unified inbox
- booking availability
- webhook endpoint พร้อมต่อจริง
- package/admin-only quota

ยังไม่ทำในเวอร์ชันแรก:

- mobile app
- AI voice
- payment settlement จริง
- full CRM
- inventory/accounting
- complex overage billing ให้คลินิกเห็น
