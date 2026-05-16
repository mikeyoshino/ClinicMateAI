# ARD-001: LINE Webhook Integration
# บันทึกการตัดสินใจสถาปัตยกรรม: การเชื่อมต่อ LINE Webhook

**Status:** Draft — Pending Review  
**สถานะ:** ร่าง — รอการอนุมัติ

---

## Context | บริบท

ClinicMateAI acts as an AI receptionist for Thai beauty clinics on LINE OA. Clinics receive customer messages on LINE and the system must:

1. Receive those messages in real time via LINE's webhook push.
2. Store them in the unified inbox.
3. Generate an AI reply and send it back to the customer on LINE immediately.

The current `/webhooks/line` endpoint is a stub — it accepts arbitrary JSON, has no LINE protocol support, and cannot receive real LINE events.

---

## Problem | ปัญหา

| Gap | Impact |
|---|---|
| No HMAC-SHA256 signature verification | Any HTTP client can inject fake messages |
| No LINE event format parsing | Real LINE payloads are ignored/broken |
| No reply back to LINE | AI replies are computed but never sent to customer |
| No per-clinic LINE credential storage | All clinics would share one token (wrong) |
| Customer display name not fetched | Conversations show empty/unknown names |

---

## Decision | การตัดสินใจ

### 1. Per-Clinic Webhook URL

**Chosen:** `/webhooks/line/{clinicId}`  
**Rejected:** Single `/webhooks/line` with body-based routing

**Reason:** Each clinic has their own LINE OA with their own Channel Secret. A per-clinic URL makes credential lookup trivial — no guessing which clinic owns the incoming event. Clinic registers their unique URL in LINE Developer Console.

---

### 2. Credential Storage — `ClinicChannelConfig` Table

**Chosen:** New `ClinicChannelConfig` entity separate from `Clinic`  
**Rejected:** Adding LINE fields directly onto `Clinic`

```
ClinicChannelConfig
├── Id (Guid)
├── ClinicId (FK → Clinic)
├── Channel ("LINE" | "Facebook")
├── AccessToken (encrypted at rest)
├── Secret (encrypted at rest)
└── ExternalPageId (LINE OA user ID / Facebook page ID)
```

**Reason:** Keeps `Clinic` clean. Reuses exact same pattern for Facebook later with zero `Clinic` changes. Allows multiple channels per clinic.

---

### 3. Signature Verification

LINE signs every webhook request:

```
X-Line-Signature: base64( HMAC-SHA256( rawBody, channelSecret ) )
```

**Flow:**
1. Read raw body as `byte[]` before JSON deserialization.
2. Load clinic's `ClinicChannelConfig` (Channel=LINE).
3. Compute `HMAC-SHA256(body, secret)`, compare to header.
4. Return `400 Bad Request` if mismatch.

**Interface:**
```csharp
// Application/Abstractions/Messaging/ILineSignatureVerifier.cs
interface ILineSignatureVerifier
{
    bool Verify(byte[] body, string signatureHeader, string channelSecret);
}
```

---

### 4. LINE Event Parsing

LINE sends a batch payload:
```json
{
  "destination": "Uxxxxx",
  "events": [
    {
      "type": "message",
      "replyToken": "abc123",
      "source": { "type": "user", "userId": "Uyyy" },
      "message": { "type": "text", "text": "สวัสดี" }
    }
  ]
}
```

**MVP handles:** `event.type = "message"` + `event.message.type = "text"` only.  
**Ignored:** follow, unfollow, postback, image, sticker, location events — silently skipped, still return 200.

**Interface:**
```csharp
// Application/Abstractions/Messaging/ILineWebhookParser.cs
interface ILineWebhookParser
{
    LineWebhookPayload? Parse(byte[] body);
}
```

---

### 5. Customer Display Name

LINE webhook events include `userId` but **not** display name.

**Chosen:** Call LINE Profile API on first message per conversation.
```
GET https://api.line.me/v2/bot/profile/{userId}
Authorization: Bearer {channelAccessToken}
→ { "displayName": "สมชาย", "userId": "Uyyy", "pictureUrl": "..." }
```

**Interface:**
```csharp
// Application/Abstractions/Messaging/ILineProfileProvider.cs
interface ILineProfileProvider
{
    Task<string> GetDisplayNameAsync(string userId, string accessToken, CancellationToken ct);
}
```

Cache result in `Conversation.CustomerDisplayName` — no repeated API calls per message.

---

### 6. Sending Reply Back to LINE

After AI generates a reply, send it back using LINE Reply API (replyToken is valid for **30 seconds**):

```
POST https://api.line.me/v2/bot/message/reply
Authorization: Bearer {channelAccessToken}
{
  "replyToken": "abc123",
  "messages": [{ "type": "text", "text": "โบท็อกกรามเริ่มต้นที่ 2,999 บาทค่ะ..." }]
}
```

**Interface:**
```csharp
// Application/Abstractions/Messaging/ILineMessageSender.cs
interface ILineMessageSender
{
    Task SendReplyAsync(string replyToken, string text, string accessToken, CancellationToken ct);
    Task SendPushAsync(string userId, string text, string accessToken, CancellationToken ct);
}
```

`SendPushAsync` is included for future follow-up reminders (push does not need a replyToken).

---

### 7. Reply Timing Strategy

LINE requires a 200 OK response within **30 seconds**, and `replyToken` expires just as fast.

**Chosen:** Reply synchronously within the webhook request.  
**Rejected:** Queue reply to background job

**Reason:** Simplest path for MVP. AI reply is generated in-process (<2s for mock, <5s for real OpenAI). Background queue adds complexity and risks replyToken expiry anyway.

**If AI is slow:** Degrade gracefully — if AI takes >20s, skip LINE reply (customer gets staff draft instead). Add timeout wrapper around AI call.

---

## Full Request Flow | ขั้นตอนทั้งหมด

```
POST /webhooks/line/{clinicId}
│
├─ 1. Read raw body bytes (before model binding)
├─ 2. Load ClinicChannelConfig for clinicId + Channel=LINE
│     └─ 404 if not configured
├─ 3. Verify X-Line-Signature
│     └─ 400 if mismatch
├─ 4. Parse LINE events from body
├─ 5. For each text message event:
│     ├─ a. Fetch customer display name (LINE Profile API, first message only)
│     ├─ b. ReceiveMessageHandler → save Conversation + Message → AI decision
│     └─ c. If AI reply text available → ILineMessageSender.SendReplyAsync(replyToken, ...)
│           If escalate/draft → skip LINE reply (staff handles in inbox)
└─ 6. Return 200 OK
```

---

## New Files Summary | ไฟล์ที่ต้องสร้าง

### Application layer (contracts only)
```
Application/Abstractions/Messaging/
├── ILineSignatureVerifier.cs
├── ILineWebhookParser.cs
├── ILineMessageSender.cs
├── ILineProfileProvider.cs
└── IClinicChannelConfigRepository.cs

Application/Messaging/
└── LineWebhookPayload.cs   (DTOs: LineWebhookPayload, LineWebhookEvent, LineTextMessage)
```

### Domain
```
Domain/Clinics/
└── ClinicChannelConfig.cs
```

### Infrastructure
```
Infrastructure/Persistence/
├── ClinicChannelConfigRepository.cs
└── (EF config + migration)

Infrastructure/Messaging/
├── LineSignatureVerifier.cs
├── LineWebhookParser.cs
├── LineMessageSender.cs       (HttpClient)
└── LineProfileProvider.cs     (HttpClient)
```

### Web
```
Web/Endpoints/
└── WebhookEndpoints.cs        (rewrite /webhooks/line/{clinicId})
```

### Tests
```
tests/ClinicMateAI.Tests/Messaging/
├── LineSignatureVerifierTests.cs
└── LineWebhookEndpointTests.cs
```

---

## What This Does NOT Cover (Future) | สิ่งที่ยังไม่รวม

- Facebook Messenger webhook (same pattern, separate ARD)
- LINE image/sticker/location message handling
- LINE rich menu setup
- Webhook delivery failure retry (LINE retries automatically up to 3 times)
- Credential encryption at rest (add after basic flow works)
- Multi-page LINE OA per clinic (Enterprise tier)

---

## Open Questions | คำถามที่รอการตัดสินใจ

1. Should the AI reply be skipped entirely when `aiResult.Mode == DraftForStaff`, or should the AI send a "กำลังตรวจสอบ รอสักครู่นะคะ" holding message to the customer?
2. Should credential encryption use .NET `IDataProtector` or a separate secrets manager?
3. For local development testing — use [LINE CLI webhook proxy](https://developers.line.biz/en/docs/messaging-api/testing-webhook/) or a manual curl script to simulate events?
