# ClinicMateAI Channel Connection Wizard Design

## Overview

This design adds a clinic-friendly setup wizard for connecting messaging channels from `/clinic/integrations`.

The goal is to let non-technical clinic owners connect LINE Official Account and Facebook Messenger with clear Thai-first guidance, visible connection status, and explicit recovery actions when something fails. The wizard must feel like part of the existing clinic operations UI rather than a separate admin tool.

## Goals

- Make channel connection understandable for non-technical clinic staff.
- Keep the main integrations page focused on simple actions: connect, test, reconnect, edit.
- Support the existing LINE webhook implementation with a guided setup and test flow.
- Add a Facebook connection flow that is easy for clinics and minimizes repeated maintenance.
- Persist channel state so the UI always reflects real backend status.
- Keep channel-specific complexity behind handlers and infrastructure adapters.

## Non-Goals

- Replacing the existing message receive pipeline.
- Implementing a full integration audit center in this phase.
- Supporting every possible Facebook permission or webhook subscription edge case in the clinic UI.
- Adding unrelated integrations beyond the shared card-and-wizard pattern needed for future reuse.

## UX Direction

The wizard must visually match the clinic Integrations page:

- White cards and modals on soft slate backgrounds.
- Teal as the main clinic action color.
- Channel-specific brand accents only in logos and primary channel actions.
- Rounded cards, compact status badges, and simple Thai-first helper text.
- One consistent modal shell across channels so LINE and Facebook feel like the same product experience.

The `/clinic/integrations` page remains the entry point. Each integration appears as a status card with:

- Channel logo and short description.
- Current connection status badge.
- One primary action based on state.
- Secondary actions only when relevant, such as edit or reconnect.

## Supported Channel Flows

### LINE Official Account

LINE uses a guided manual connection because LINE OA does not provide the same OAuth-style connection flow needed for this use case.

Wizard flow:

1. Show the clinic-specific webhook URL with copy action and Thai guidance on where to paste it in LINE Developer Console.
2. Collect `Channel Secret` and `Channel Access Token`.
3. Run a connection test that validates the configuration and confirms the webhook can be used.
4. Show a completion state with connected account summary and return to Integrations.

Key UX rules:

- The wizard must explain where each value is found in LINE Developer Console.
- Required fields must be clearly marked and blocked when missing.
- The test step must not silently pass; it must show a visible success or actionable error result.

### Facebook Messenger

Facebook uses OAuth from the clinic UI so the clinic owner does not need to manage expiring personal tokens manually.

Wizard flow:

1. Clinic clicks a Facebook login action from the modal.
2. Clinic signs in and authorizes the app.
3. Clinic chooses the Facebook Page to connect.
4. The system stores the page connection, verifies permissions and webhook readiness, and shows completion.

Connection renewal rules:

- The platform owns one shared Facebook app configuration.
- The clinic completes the connection only once.
- The system stores the long-lived token required for page operations.
- A background renewal process refreshes Facebook access before expiry.
- If refresh or permissions fail, the channel status moves to `ReconnectRequired` and the clinic sees a reconnect action.

## Shared Wizard Pattern

Both channels use the same modal shell:

- Header with channel logo, title, short explanation, and step indicator.
- Horizontal progress indicator.
- One focused task per step.
- Thai helper text under labels and technical fields.
- Sticky footer actions: back, next, test, complete.

This gives the clinic one mental model even though the backend mechanics differ.

## Integration Card States

Each channel card on `/clinic/integrations` is driven from persisted backend state.

Proposed states:

- `NotConnected`
- `PendingVerification`
- `Connected`
- `ReconnectRequired`
- `Error`

Expected actions by state:

| State | Primary action | Secondary action | User message |
|---|---|---|---|
| NotConnected | Connect | None | Channel not set up yet |
| PendingVerification | Continue setup | Cancel | More information or verification is needed |
| Connected | View details | Edit / Reconnect | Ready to receive messages |
| ReconnectRequired | Reconnect | View issue | Connection needs clinic attention |
| Error | Retry | View issue | Last connection test failed |

## Architecture

### Web

- Keep `/clinic/integrations` as the clinic-facing page.
- Add clinic setup endpoints specifically for integrations and connection testing.
- Keep webhook endpoints separate from setup endpoints.

### Application

Add contracts for integration setup and status retrieval, for example:

- `GetIntegrationOverviewQuery`
- `SaveLineConnectionCommand`
- `TestLineConnectionCommand`
- `StartFacebookConnectionCommand`
- `CompleteFacebookConnectionCommand`
- `ReconnectFacebookChannelCommand`

These contracts remain DTO/command/query definitions only.

### Logic

Logic handlers coordinate:

- validation,
- clinic/channel lookup,
- provider adapter calls,
- status transitions,
- persistence through repositories and `IUnitOfWork`.

No channel workflow logic belongs in Razor components or endpoints.

### Infrastructure

Infrastructure implements:

- LINE connection test behavior,
- Facebook OAuth completion and token renewal behavior,
- webhook verification helpers,
- provider-specific API calls.

## Data Model

Keep `ClinicChannelConfig` as the primary per-clinic channel record and extend it to support status-driven setup.

Recommended fields:

- `ClinicId`
- `Channel`
- `AccessToken`
- `Secret`
- `ExternalPageId`
- `IsEnabled`
- `ConnectionStatus`
- `LastVerifiedAtUtc`
- `LastError`
- `TokenExpiresAtUtc`
- `RefreshTokenOrLongLivedToken` where needed for Facebook renewal
- `UpdatedAtUtc`

Notes:

- LINE mainly uses `Secret`, `AccessToken`, and verification timestamps.
- Facebook mainly uses page identity, token lifetime, and reconnect status.
- `ExternalPageId` continues to support channel-specific external identity lookup and already fits the Facebook webhook routing need.

## Endpoint and Webhook Flow

### Clinic setup endpoints

Add setup endpoints for:

- getting integration overview by clinic,
- saving LINE configuration,
- testing LINE configuration,
- starting Facebook OAuth,
- completing Facebook OAuth callback,
- reconnecting or disabling a channel.

These endpoints belong with clinic setup/integration behavior, not with message receive webhooks.

### Message webhooks

LINE webhook remains clinic-specific:

- `POST /webhooks/line/{clinicId}`

Facebook webhook uses shared endpoint routing by external page identity:

- `POST /webhooks/facebook`

Routing difference:

- LINE resolves the clinic from `clinicId` in the route.
- Facebook resolves the clinic from the incoming `page_id` or equivalent page identity and matches that to `ClinicChannelConfig.ExternalPageId`.

Both channels then flow into the same internal message receive pipeline.

## Error Handling

The UI must always show explicit recovery guidance.

Examples:

- Invalid LINE credentials: remain on the test step and show which value likely needs correction.
- Facebook permission revoked: change card state to `ReconnectRequired`.
- Webhook verification issue: show warning or error state instead of pretending the channel is healthy.
- Expired or invalid Facebook token after renewal attempt: stop treating the channel as connected and surface reconnect CTA.

Do not use silent fallback states that hide connection failures from clinic staff.

## Testing Strategy

### Logic tests

- connection status transitions,
- LINE test success and failure handling,
- Facebook reconnect-required transitions,
- token renewal decision rules,
- package/channel gating where applicable.

### Integration tests

- LINE webhook still resolves clinic by `clinicId`,
- Facebook webhook resolves clinic by `ExternalPageId`,
- invalid signatures and provider failures surface the correct state,
- successful connection flows persist the correct clinic channel configuration.

### UI checks

- channel cards render the correct badge and actions for each state,
- modal steps block invalid progression,
- Thai helper text remains visible for required fields,
- completion and reconnect states match backend truth.

### Happy-path verification

1. Connect LINE manually and pass test.
2. Connect Facebook through OAuth and store the selected page.
3. Fail a LINE test and keep the clinic on the actionable recovery step.
4. Force a Facebook token/permission failure and surface reconnect state.

## Delivery Sequence

1. Add integration overview read model and status-driven card rendering.
2. Implement LINE wizard flow on top of existing webhook groundwork.
3. Implement Facebook OAuth connection and reconnect flow.
4. Add channel test/status persistence.
5. Polish modal/card UX so it fully matches the clinic integrations page.

## Open Design Decisions Resolved

- **Wizard shell:** modal opened from integration cards.
- **LINE connection model:** guided manual entry with test step.
- **Facebook connection model:** OAuth with automated renewal.
- **Primary clinic UX:** simple card states and explicit actions instead of technical configuration screens.

## Recommendation

Proceed with one unified integrations experience in the clinic UI, while implementing channel-specific connection mechanics behind separate handlers and adapters. This gives clinic users a simple setup path without flattening real provider differences into the UI.
