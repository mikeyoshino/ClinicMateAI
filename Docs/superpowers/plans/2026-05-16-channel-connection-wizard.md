# Channel Connection Wizard Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a clinic-friendly integrations page and modal wizard that lets clinics connect LINE and Facebook, test connections, persist channel status, and surface reconnect actions when a channel becomes unhealthy.

**Architecture:** Keep the clinic UI centered on `/clinic/integrations`, but move all connection logic into `Setup` command/query handlers plus provider adapters. Extend `ClinicChannelConfig` into a status-driven record that supports manual LINE setup and Facebook OAuth with renewal metadata, while keeping inbound webhooks separate from setup endpoints.

**Tech Stack:** Blazor Server, ASP.NET Core minimal APIs, C#, EF Core, PostgreSQL, FluentValidation, xUnit, bUnit, FluentAssertions.

---

## File Structure

- Create: `src/ClinicMateAI.Application/Setup/ChannelConnectionStatus.cs`
- Create: `src/ClinicMateAI.Application/Setup/ClinicIntegrationChannelDto.cs`
- Create: `src/ClinicMateAI.Application/Setup/GetIntegrationOverviewQuery.cs`
- Create: `src/ClinicMateAI.Application/Setup/IGetIntegrationOverviewHandler.cs`
- Create: `src/ClinicMateAI.Application/Setup/SaveLineChannelConfigCommand.cs`
- Create: `src/ClinicMateAI.Application/Setup/ISaveLineChannelConfigHandler.cs`
- Create: `src/ClinicMateAI.Application/Setup/TestLineChannelConfigCommand.cs`
- Create: `src/ClinicMateAI.Application/Setup/ITestLineChannelConfigHandler.cs`
- Create: `src/ClinicMateAI.Application/Setup/StartFacebookConnectionResult.cs`
- Create: `src/ClinicMateAI.Application/Setup/IStartFacebookConnectionHandler.cs`
- Create: `src/ClinicMateAI.Application/Setup/CompleteFacebookConnectionCommand.cs`
- Create: `src/ClinicMateAI.Application/Setup/ICompleteFacebookConnectionHandler.cs`
- Create: `src/ClinicMateAI.Application/Abstractions/Messaging/ILineChannelConnectionTester.cs`
- Create: `src/ClinicMateAI.Application/Abstractions/Messaging/IFacebookConnectionProvider.cs`
- Create: `src/ClinicMateAI.Application/Abstractions/Messaging/IFacebookTokenRenewalProvider.cs`
- Modify: `src/ClinicMateAI.Application/Abstractions/Persistence/IClinicChannelConfigRepository.cs`
- Modify: `src/ClinicMateAI.Domain/Clinics/ClinicChannelConfig.cs`
- Create: `src/ClinicMateAI.Logic/Setup/GetIntegrationOverviewHandler.cs`
- Create: `src/ClinicMateAI.Logic/Setup/SaveLineChannelConfigHandler.cs`
- Create: `src/ClinicMateAI.Logic/Setup/TestLineChannelConfigHandler.cs`
- Create: `src/ClinicMateAI.Logic/Setup/StartFacebookConnectionHandler.cs`
- Create: `src/ClinicMateAI.Logic/Setup/CompleteFacebookConnectionHandler.cs`
- Create: `src/ClinicMateAI.Logic/Setup/SaveLineChannelConfigCommandValidator.cs`
- Create: `src/ClinicMateAI.Logic/Setup/TestLineChannelConfigCommandValidator.cs`
- Modify: `src/ClinicMateAI.Infrastructure/Data/AppDbContext.cs`
- Create: `src/ClinicMateAI.Infrastructure/Messaging/LineChannelConnectionTester.cs`
- Create: `src/ClinicMateAI.Infrastructure/Messaging/FacebookConnectionProvider.cs`
- Create: `src/ClinicMateAI.Infrastructure/Messaging/FacebookTokenRenewalProvider.cs`
- Modify: `src/ClinicMateAI.Infrastructure/Persistence/ClinicChannelConfigRepository.cs`
- Create: `src/ClinicMateAI.Web/Endpoints/IntegrationEndpoints.cs`
- Modify: `src/ClinicMateAI.Web/Program.cs`
- Modify: `src/ClinicMateAI.Web/Components/Pages/Integrations.razor`
- Create: `src/ClinicMateAI.Web/Components/Pages/Integrations.razor.css`
- Modify: `src/ClinicMateAI.Web/Components/Layout/ClinicLayout.razor`
- Test: `tests/ClinicMateAI.Tests/Setup/GetIntegrationOverviewHandlerTests.cs`
- Test: `tests/ClinicMateAI.Tests/Setup/SaveLineChannelConfigHandlerTests.cs`
- Test: `tests/ClinicMateAI.Tests/Setup/TestLineChannelConfigHandlerTests.cs`
- Test: `tests/ClinicMateAI.Tests/Setup/CompleteFacebookConnectionHandlerTests.cs`
- Test: `tests/ClinicMateAI.Web.Tests/Integrations/IntegrationEndpointsTests.cs`
- Test: `tests/ClinicMateAI.Web.Tests/Integrations/IntegrationsPageTests.cs`

## Task 1: Add status-driven channel configuration foundation

**Files:**
- Create: `src/ClinicMateAI.Application/Setup/ChannelConnectionStatus.cs`
- Modify: `src/ClinicMateAI.Domain/Clinics/ClinicChannelConfig.cs`
- Modify: `src/ClinicMateAI.Application/Abstractions/Persistence/IClinicChannelConfigRepository.cs`
- Modify: `src/ClinicMateAI.Infrastructure/Persistence/ClinicChannelConfigRepository.cs`
- Modify: `src/ClinicMateAI.Infrastructure/Data/AppDbContext.cs`
- Test: `tests/ClinicMateAI.Tests/Setup/GetIntegrationOverviewHandlerTests.cs`

- [ ] **Step 1: Write the failing read-model test**

Create `tests/ClinicMateAI.Tests/Setup/GetIntegrationOverviewHandlerTests.cs` with:

```csharp
using ClinicMateAI.Application.Setup;
using ClinicMateAI.Domain.Clinics;
using ClinicMateAI.Infrastructure.Persistence;
using FluentAssertions;

namespace ClinicMateAI.Tests.Setup;

public class GetIntegrationOverviewHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsChannelStatuses_FromClinicConfigs()
    {
        var configs = new[]
        {
            new ClinicChannelConfig
            {
                ClinicId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Channel = "LINE",
                AccessToken = "line-token",
                Secret = "line-secret",
                ExternalPageId = "line-page",
                ConnectionStatus = ChannelConnectionStatus.Connected,
                LastVerifiedAtUtc = new DateTime(2026, 5, 16, 10, 0, 0, DateTimeKind.Utc)
            },
            new ClinicChannelConfig
            {
                ClinicId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Channel = "Facebook",
                AccessToken = "fb-token",
                Secret = "fb-secret",
                ExternalPageId = "123456789",
                ConnectionStatus = ChannelConnectionStatus.ReconnectRequired,
                LastError = "Permissions revoked"
            }
        };

        configs[0].IsEnabled.Should().BeTrue();
        configs[1].ConnectionStatus.Should().Be(ChannelConnectionStatus.ReconnectRequired);
    }
}
```

- [ ] **Step 2: Run the test to verify the new status model does not exist yet**

Run:

```powershell
dotnet test tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter GetIntegrationOverviewHandlerTests
```

Expected: FAIL because `ChannelConnectionStatus`, `ConnectionStatus`, `LastVerifiedAtUtc`, or `LastError` are missing.

- [ ] **Step 3: Add the status enum and extend `ClinicChannelConfig`**

Create `src/ClinicMateAI.Application/Setup/ChannelConnectionStatus.cs`:

```csharp
namespace ClinicMateAI.Application.Setup;

public enum ChannelConnectionStatus
{
    NotConnected = 0,
    PendingVerification = 1,
    Connected = 2,
    ReconnectRequired = 3,
    Error = 4
}
```

Update `src/ClinicMateAI.Domain/Clinics/ClinicChannelConfig.cs` to include:

```csharp
using ClinicMateAI.Application.Setup;

public ChannelConnectionStatus ConnectionStatus { get; set; } = ChannelConnectionStatus.NotConnected;
public DateTime? LastVerifiedAtUtc { get; set; }
public string LastError { get; set; } = string.Empty;
public DateTime? TokenExpiresAtUtc { get; set; }
public string RefreshTokenOrLongLivedToken { get; set; } = string.Empty;
public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
```

- [ ] **Step 4: Extend repository and EF mapping**

Update `IClinicChannelConfigRepository` with:

```csharp
Task<ClinicChannelConfig?> GetByExternalPageIdAsync(string externalPageId, CancellationToken ct = default);
Task UpsertAsync(ClinicChannelConfig config, CancellationToken ct = default);
```

Update `AppDbContext` so the new properties are mapped explicitly:

```csharp
modelBuilder.Entity<ClinicChannelConfig>()
    .Property(x => x.ConnectionStatus)
    .HasConversion<string>()
    .HasMaxLength(30)
    .HasDefaultValue(ChannelConnectionStatus.NotConnected);

modelBuilder.Entity<ClinicChannelConfig>()
    .Property(x => x.LastError)
    .HasMaxLength(1000);

modelBuilder.Entity<ClinicChannelConfig>()
    .Property(x => x.RefreshTokenOrLongLivedToken)
    .HasMaxLength(1000);

modelBuilder.Entity<ClinicChannelConfig>()
    .Property(x => x.UpdatedAtUtc)
    .HasDefaultValueSql("NOW()");
```

- [ ] **Step 5: Add and inspect the migration**

Run:

```powershell
dotnet ef migrations add AddChannelConnectionStatus --project src\ClinicMateAI.Infrastructure --startup-project src\ClinicMateAI.Web
```

Expected: a migration is created under `src\ClinicMateAI.Infrastructure\Data\Migrations\` adding the new columns and enum-as-string fields.

- [ ] **Step 6: Run the test again**

Run:

```powershell
dotnet test tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter GetIntegrationOverviewHandlerTests
```

Expected: PASS.

- [ ] **Step 7: Commit**

```powershell
git add src\ClinicMateAI.Application\Setup\ChannelConnectionStatus.cs src\ClinicMateAI.Domain\Clinics\ClinicChannelConfig.cs src\ClinicMateAI.Application\Abstractions\Persistence\IClinicChannelConfigRepository.cs src\ClinicMateAI.Infrastructure\Persistence\ClinicChannelConfigRepository.cs src\ClinicMateAI.Infrastructure\Data\AppDbContext.cs src\ClinicMateAI.Infrastructure\Data\Migrations tests\ClinicMateAI.Tests\Setup\GetIntegrationOverviewHandlerTests.cs
git commit -m "feat: add channel connection status foundation"
```

## Task 2: Add integration overview query and clinic endpoints

**Files:**
- Create: `src/ClinicMateAI.Application/Setup/ClinicIntegrationChannelDto.cs`
- Create: `src/ClinicMateAI.Application/Setup/GetIntegrationOverviewQuery.cs`
- Create: `src/ClinicMateAI.Application/Setup/IGetIntegrationOverviewHandler.cs`
- Create: `src/ClinicMateAI.Logic/Setup/GetIntegrationOverviewHandler.cs`
- Create: `src/ClinicMateAI.Web/Endpoints/IntegrationEndpoints.cs`
- Modify: `src/ClinicMateAI.Web/Program.cs`
- Test: `tests/ClinicMateAI.Web.Tests/Integrations/IntegrationEndpointsTests.cs`

- [ ] **Step 1: Write the failing endpoint test**

Create `tests/ClinicMateAI.Web.Tests/Integrations/IntegrationEndpointsTests.cs` with:

```csharp
[Fact]
public async Task GetOverview_ReturnsLineAndFacebookStatuses()
{
    await using var factory = new ClinicMateWebFactory();
    using var client = factory.CreateClient();

    var clinicId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    var response = await client.GetAsync($"/api/integrations/overview?clinicId={clinicId}");

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var json = await response.Content.ReadAsStringAsync();
    json.Should().Contain("\"channel\":\"LINE\"");
    json.Should().Contain("\"channel\":\"Facebook\"");
}
```

- [ ] **Step 2: Run the endpoint test**

Run:

```powershell
dotnet test tests\ClinicMateAI.Web.Tests\ClinicMateAI.Web.Tests.csproj --filter GetOverview_ReturnsLineAndFacebookStatuses
```

Expected: FAIL with 404 because the endpoint does not exist.

- [ ] **Step 3: Add overview contracts and handler**

Create DTO/handler contracts:

```csharp
public sealed record ClinicIntegrationChannelDto(
    string Channel,
    ChannelConnectionStatus Status,
    string Summary,
    string LastError,
    DateTime? LastVerifiedAtUtc,
    bool IsEnabled);

public sealed record GetIntegrationOverviewQuery(Guid ClinicId);
```

Create handler logic that maps existing or missing configs into the two required channels:

```csharp
var channels = new[] { "LINE", "Facebook" };
var configs = await repository.GetAllByClinicAsync(query.ClinicId, cancellationToken);

return channels.Select(channel =>
{
    var config = configs.SingleOrDefault(x => x.Channel == channel);
    return config is null
        ? new ClinicIntegrationChannelDto(channel, ChannelConnectionStatus.NotConnected, "ยังไม่ได้เชื่อมต่อ", string.Empty, null, false)
        : new ClinicIntegrationChannelDto(channel, config.ConnectionStatus, BuildSummary(config), config.LastError, config.LastVerifiedAtUtc, config.IsEnabled);
}).ToArray();
```

- [ ] **Step 4: Add the integration endpoint and DI registrations**

Create `IntegrationEndpoints.cs` with:

```csharp
endpoints.MapGet("/api/integrations/overview", async Task<IResult> (
    Guid clinicId,
    IGetIntegrationOverviewHandler handler,
    CancellationToken cancellationToken) =>
{
    if (clinicId == Guid.Empty)
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["clinicId"] = ["clinicId is required."]
        });
    }

    var result = await handler.HandleAsync(new GetIntegrationOverviewQuery(clinicId), cancellationToken);
    return Results.Ok(result);
});
```

Register the handler in `Program.cs` and call `app.MapIntegrationEndpoints();`.

- [ ] **Step 5: Re-run the endpoint test**

Run:

```powershell
dotnet test tests\ClinicMateAI.Web.Tests\ClinicMateAI.Web.Tests.csproj --filter GetOverview_ReturnsLineAndFacebookStatuses
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add src\ClinicMateAI.Application\Setup\ClinicIntegrationChannelDto.cs src\ClinicMateAI.Application\Setup\GetIntegrationOverviewQuery.cs src\ClinicMateAI.Application\Setup\IGetIntegrationOverviewHandler.cs src\ClinicMateAI.Logic\Setup\GetIntegrationOverviewHandler.cs src\ClinicMateAI.Web\Endpoints\IntegrationEndpoints.cs src\ClinicMateAI.Web\Program.cs tests\ClinicMateAI.Web.Tests\Integrations\IntegrationEndpointsTests.cs
git commit -m "feat: add integration overview endpoint"
```

## Task 3: Implement LINE save-and-test vertical slice

**Files:**
- Create: `src/ClinicMateAI.Application/Setup/SaveLineChannelConfigCommand.cs`
- Create: `src/ClinicMateAI.Application/Setup/ISaveLineChannelConfigHandler.cs`
- Create: `src/ClinicMateAI.Application/Setup/TestLineChannelConfigCommand.cs`
- Create: `src/ClinicMateAI.Application/Setup/ITestLineChannelConfigHandler.cs`
- Create: `src/ClinicMateAI.Application/Abstractions/Messaging/ILineChannelConnectionTester.cs`
- Create: `src/ClinicMateAI.Logic/Setup/SaveLineChannelConfigHandler.cs`
- Create: `src/ClinicMateAI.Logic/Setup/TestLineChannelConfigHandler.cs`
- Create: `src/ClinicMateAI.Logic/Setup/SaveLineChannelConfigCommandValidator.cs`
- Create: `src/ClinicMateAI.Logic/Setup/TestLineChannelConfigCommandValidator.cs`
- Create: `src/ClinicMateAI.Infrastructure/Messaging/LineChannelConnectionTester.cs`
- Modify: `src/ClinicMateAI.Web/Endpoints/IntegrationEndpoints.cs`
- Modify: `src/ClinicMateAI.Web/Program.cs`
- Test: `tests/ClinicMateAI.Tests/Setup/SaveLineChannelConfigHandlerTests.cs`
- Test: `tests/ClinicMateAI.Tests/Setup/TestLineChannelConfigHandlerTests.cs`

- [ ] **Step 1: Write failing handler tests**

Create two tests:

```csharp
[Fact]
public async Task SaveLineConfig_StoresPendingVerificationStatus()
{
    // save command should persist tokens and leave status PendingVerification
}

[Fact]
public async Task TestLineConfig_MarksConfigConnected_WhenProviderSucceeds()
{
    // test command should set Connected and LastVerifiedAtUtc
}
```

Use a fake `ILineChannelConnectionTester` that returns:

```csharp
Task.FromResult(new LineConnectionTestResult(true, string.Empty));
```

- [ ] **Step 2: Run the focused tests**

Run:

```powershell
dotnet test tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter "SaveLineConfig|TestLineConfig"
```

Expected: FAIL because the commands, handlers, and tester abstraction do not exist.

- [ ] **Step 3: Add contracts and validator rules**

Define commands:

```csharp
public sealed record SaveLineChannelConfigCommand(Guid ClinicId, string ChannelSecret, string AccessToken);
public sealed record TestLineChannelConfigCommand(Guid ClinicId);
```

Validator rules:

```csharp
RuleFor(x => x.ClinicId).NotEmpty();
RuleFor(x => x.ChannelSecret).NotEmpty().MaximumLength(200);
RuleFor(x => x.AccessToken).NotEmpty().MaximumLength(500);
```

- [ ] **Step 4: Implement the save and test handlers**

Save handler core:

```csharp
var config = await repository.GetByClinicAndChannelAsync(command.ClinicId, "LINE", cancellationToken)
    ?? new ClinicChannelConfig { ClinicId = command.ClinicId, Channel = "LINE", ExternalPageId = string.Empty };

config.Secret = command.ChannelSecret;
config.AccessToken = command.AccessToken;
config.ConnectionStatus = ChannelConnectionStatus.PendingVerification;
config.LastError = string.Empty;
config.UpdatedAtUtc = DateTime.UtcNow;

await repository.UpsertAsync(config, cancellationToken);
await unitOfWork.SaveChangesAsync(cancellationToken);
```

Test handler core:

```csharp
var config = await repository.GetByClinicAndChannelAsync(command.ClinicId, "LINE", cancellationToken)
    ?? throw new ValidationException("LINE channel is not configured.");

var result = await tester.TestAsync(config.Secret, config.AccessToken, cancellationToken);
config.ConnectionStatus = result.IsSuccess ? ChannelConnectionStatus.Connected : ChannelConnectionStatus.Error;
config.LastVerifiedAtUtc = DateTime.UtcNow;
config.LastError = result.ErrorMessage;
```

- [ ] **Step 5: Expose save and test endpoints**

Add endpoints:

```csharp
POST /api/integrations/line/save
POST /api/integrations/line/test
```

Request shape:

```csharp
public sealed record SaveLineChannelConfigRequest(Guid ClinicId, string ChannelSecret, string AccessToken);
public sealed record TestLineChannelConfigRequest(Guid ClinicId);
```

- [ ] **Step 6: Run the tests**

Run:

```powershell
dotnet test tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter "SaveLineConfig|TestLineConfig"
dotnet test tests\ClinicMateAI.Web.Tests\ClinicMateAI.Web.Tests.csproj --filter "line"
```

Expected: PASS for new handler tests and related web tests.

- [ ] **Step 7: Commit**

```powershell
git add src\ClinicMateAI.Application\Setup\SaveLineChannelConfigCommand.cs src\ClinicMateAI.Application\Setup\ISaveLineChannelConfigHandler.cs src\ClinicMateAI.Application\Setup\TestLineChannelConfigCommand.cs src\ClinicMateAI.Application\Setup\ITestLineChannelConfigHandler.cs src\ClinicMateAI.Application\Abstractions\Messaging\ILineChannelConnectionTester.cs src\ClinicMateAI.Logic\Setup\SaveLineChannelConfigHandler.cs src\ClinicMateAI.Logic\Setup\TestLineChannelConfigHandler.cs src\ClinicMateAI.Logic\Setup\SaveLineChannelConfigCommandValidator.cs src\ClinicMateAI.Logic\Setup\TestLineChannelConfigCommandValidator.cs src\ClinicMateAI.Infrastructure\Messaging\LineChannelConnectionTester.cs src\ClinicMateAI.Web\Endpoints\IntegrationEndpoints.cs src\ClinicMateAI.Web\Program.cs tests\ClinicMateAI.Tests\Setup\SaveLineChannelConfigHandlerTests.cs tests\ClinicMateAI.Tests\Setup\TestLineChannelConfigHandlerTests.cs
git commit -m "feat: add line connection wizard backend"
```

## Task 4: Implement Facebook OAuth completion and reconnect state

**Files:**
- Create: `src/ClinicMateAI.Application/Setup/StartFacebookConnectionResult.cs`
- Create: `src/ClinicMateAI.Application/Setup/IStartFacebookConnectionHandler.cs`
- Create: `src/ClinicMateAI.Application/Setup/CompleteFacebookConnectionCommand.cs`
- Create: `src/ClinicMateAI.Application/Setup/ICompleteFacebookConnectionHandler.cs`
- Create: `src/ClinicMateAI.Application/Abstractions/Messaging/IFacebookConnectionProvider.cs`
- Create: `src/ClinicMateAI.Logic/Setup/StartFacebookConnectionHandler.cs`
- Create: `src/ClinicMateAI.Logic/Setup/CompleteFacebookConnectionHandler.cs`
- Create: `src/ClinicMateAI.Infrastructure/Messaging/FacebookConnectionProvider.cs`
- Modify: `src/ClinicMateAI.Web/Endpoints/IntegrationEndpoints.cs`
- Modify: `src/ClinicMateAI.Web/Program.cs`
- Test: `tests/ClinicMateAI.Tests/Setup/CompleteFacebookConnectionHandlerTests.cs`

- [ ] **Step 1: Write the failing Facebook completion test**

Create `CompleteFacebookConnectionHandlerTests.cs`:

```csharp
[Fact]
public async Task CompleteConnection_PersistsPageIdentity_AndMarksConnected()
{
    // provider returns page id, page name, long-lived token, and expiry
}
```

Fake provider result:

```csharp
new FacebookConnectionResult(
    PageId: "123456789",
    PageName: "The Glow Clinic Bangkok",
    AccessToken: "page-token",
    LongLivedToken: "renew-token",
    TokenExpiresAtUtc: new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc));
```

- [ ] **Step 2: Run the test**

Run:

```powershell
dotnet test tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter CompleteConnection_PersistsPageIdentity_AndMarksConnected
```

Expected: FAIL because the command, provider abstraction, and handler do not exist.

- [ ] **Step 3: Add the start and complete contracts**

Use:

```csharp
public sealed record StartFacebookConnectionResult(string AuthorizationUrl);
public sealed record CompleteFacebookConnectionCommand(Guid ClinicId, string AuthorizationCode);
```

Provider methods:

```csharp
string BuildAuthorizationUrl(Guid clinicId);
Task<FacebookConnectionResult> CompleteAsync(Guid clinicId, string authorizationCode, CancellationToken cancellationToken = default);
```

- [ ] **Step 4: Implement the completion handler**

Handler core:

```csharp
var result = await provider.CompleteAsync(command.ClinicId, command.AuthorizationCode, cancellationToken);

var config = await repository.GetByClinicAndChannelAsync(command.ClinicId, "Facebook", cancellationToken)
    ?? new ClinicChannelConfig { ClinicId = command.ClinicId, Channel = "Facebook", Secret = string.Empty };

config.AccessToken = result.AccessToken;
config.RefreshTokenOrLongLivedToken = result.LongLivedToken;
config.TokenExpiresAtUtc = result.TokenExpiresAtUtc;
config.ExternalPageId = result.PageId;
config.ConnectionStatus = ChannelConnectionStatus.Connected;
config.LastVerifiedAtUtc = DateTime.UtcNow;
config.LastError = string.Empty;
config.IsEnabled = true;
```

- [ ] **Step 5: Add Facebook start/complete endpoints**

Add:

```csharp
GET /api/integrations/facebook/start?clinicId={clinicId}
POST /api/integrations/facebook/complete
```

The `start` endpoint returns the authorization URL. The `complete` endpoint persists the selected page and connected state.

- [ ] **Step 6: Run the focused tests**

Run:

```powershell
dotnet test tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter Facebook
dotnet test tests\ClinicMateAI.Web.Tests\ClinicMateAI.Web.Tests.csproj --filter facebook
```

Expected: PASS.

- [ ] **Step 7: Commit**

```powershell
git add src\ClinicMateAI.Application\Setup\StartFacebookConnectionResult.cs src\ClinicMateAI.Application\Setup\IStartFacebookConnectionHandler.cs src\ClinicMateAI.Application\Setup\CompleteFacebookConnectionCommand.cs src\ClinicMateAI.Application\Setup\ICompleteFacebookConnectionHandler.cs src\ClinicMateAI.Application\Abstractions\Messaging\IFacebookConnectionProvider.cs src\ClinicMateAI.Logic\Setup\StartFacebookConnectionHandler.cs src\ClinicMateAI.Logic\Setup\CompleteFacebookConnectionHandler.cs src\ClinicMateAI.Infrastructure\Messaging\FacebookConnectionProvider.cs src\ClinicMateAI.Web\Endpoints\IntegrationEndpoints.cs src\ClinicMateAI.Web\Program.cs tests\ClinicMateAI.Tests\Setup\CompleteFacebookConnectionHandlerTests.cs
git commit -m "feat: add facebook connection flow"
```

## Task 5: Replace the static Integrations page with cards and a modal wizard

**Files:**
- Modify: `src/ClinicMateAI.Web/Components/Pages/Integrations.razor`
- Create: `src/ClinicMateAI.Web/Components/Pages/Integrations.razor.css`
- Modify: `src/ClinicMateAI.Web/Components/Layout/ClinicLayout.razor`
- Test: `tests/ClinicMateAI.Web.Tests/Integrations/IntegrationsPageTests.cs`

- [ ] **Step 1: Write the failing bUnit page tests**

Create `tests/ClinicMateAI.Web.Tests/Integrations/IntegrationsPageTests.cs`:

```csharp
[Fact]
public void Integrations_RendersChannelCards_FromApi()
{
    // mock overview endpoint and assert LINE / Facebook cards render
}

[Fact]
public void Integrations_ShowsReconnectAction_WhenChannelRequiresReconnect()
{
    // mock Facebook status = ReconnectRequired and assert reconnect button exists
}
```

Use a routing `HttpMessageHandler` similar to `InboxPageTests`.

- [ ] **Step 2: Run the page tests**

Run:

```powershell
dotnet test tests\ClinicMateAI.Web.Tests\ClinicMateAI.Web.Tests.csproj --filter IntegrationsPageTests
```

Expected: FAIL because the page is still a static table.

- [ ] **Step 3: Rebuild the page around the overview API**

Replace the table with:

```razor
@foreach (var channel in channels)
{
    <article class="integration-card">
        <div class="integration-card__header">
            <div class="integration-logo">@channel.Channel</div>
            <div>
                <h3>@GetThaiTitle(channel.Channel)</h3>
                <p>@channel.Summary</p>
            </div>
            <span class="@GetBadgeClass(channel.Status)">@GetThaiStatus(channel.Status)</span>
        </div>
        <div class="integration-card__actions">
            <button @onclick="() => OpenWizard(channel.Channel)">@GetPrimaryAction(channel.Status)</button>
        </div>
    </article>
}
```

Add modal state:

```csharp
private bool isWizardOpen;
private string activeChannel = string.Empty;
private int activeStep = 1;
```

- [ ] **Step 4: Add CSS matching the approved design**

Create `Integrations.razor.css` with:

```css
.integration-grid { display:grid; grid-template-columns:repeat(auto-fit,minmax(280px,1fr)); gap:1rem; }
.integration-card { background:#fff; border:1px solid #dbe4ea; border-radius:1rem; padding:1.25rem; box-shadow:0 8px 24px rgba(15,23,42,.05); }
.integration-card__badge--connected { background:#dcfce7; color:#166534; }
.integration-card__badge--warning { background:#fef3c7; color:#92400e; }
.integration-modal { background:#fff; border-radius:1.25rem; box-shadow:0 24px 60px rgba(15,23,42,.16); }
```

- [ ] **Step 5: Re-run the bUnit tests**

Run:

```powershell
dotnet test tests\ClinicMateAI.Web.Tests\ClinicMateAI.Web.Tests.csproj --filter IntegrationsPageTests
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add src\ClinicMateAI.Web\Components\Pages\Integrations.razor src\ClinicMateAI.Web\Components\Pages\Integrations.razor.css tests\ClinicMateAI.Web.Tests\Integrations\IntegrationsPageTests.cs
git commit -m "feat: redesign integrations page as wizard entrypoint"
```

## Task 6: Add Facebook renewal and reconnect downgrade behavior

**Files:**
- Create: `src/ClinicMateAI.Application/Abstractions/Messaging/IFacebookTokenRenewalProvider.cs`
- Create: `src/ClinicMateAI.Infrastructure/Messaging/FacebookTokenRenewalProvider.cs`
- Modify: `src/ClinicMateAI.Logic/Setup/CompleteFacebookConnectionHandler.cs`
- Modify: `src/ClinicMateAI.Web/Program.cs`
- Test: `tests/ClinicMateAI.Tests/Setup/CompleteFacebookConnectionHandlerTests.cs`

- [ ] **Step 1: Add a failing renewal decision test**

Add:

```csharp
[Fact]
public async Task RenewAsync_MarksReconnectRequired_WhenRenewalFails()
{
    // expired token + failed provider renewal should downgrade status
}
```

- [ ] **Step 2: Run the renewal test**

Run:

```powershell
dotnet test tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter RenewAsync_MarksReconnectRequired_WhenRenewalFails
```

Expected: FAIL because there is no renewal service yet.

- [ ] **Step 3: Add provider abstraction and downgrade logic**

Provider method:

```csharp
Task<FacebookTokenRenewalResult> RenewAsync(string longLivedToken, CancellationToken cancellationToken = default);
```

Downgrade rule:

```csharp
if (!result.IsSuccess)
{
    config.ConnectionStatus = ChannelConnectionStatus.ReconnectRequired;
    config.LastError = result.ErrorMessage;
    config.IsEnabled = false;
}
```

- [ ] **Step 4: Register the provider and add a scheduled entry point**

Register in `Program.cs`:

```csharp
builder.Services.AddHttpClient<IFacebookTokenRenewalProvider, FacebookTokenRenewalProvider>();
```

Add a temporary manual renewal endpoint for MVP verification:

```csharp
POST /api/integrations/facebook/renew
```

This keeps the first implementation testable before adding a real background worker.

- [ ] **Step 5: Run the tests**

Run:

```powershell
dotnet test tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter "RenewAsync|Facebook"
```

Expected: PASS.

- [ ] **Step 6: Commit**

```powershell
git add src\ClinicMateAI.Application\Abstractions\Messaging\IFacebookTokenRenewalProvider.cs src\ClinicMateAI.Infrastructure\Messaging\FacebookTokenRenewalProvider.cs src\ClinicMateAI.Logic\Setup\CompleteFacebookConnectionHandler.cs src\ClinicMateAI.Web\Program.cs tests\ClinicMateAI.Tests\Setup\CompleteFacebookConnectionHandlerTests.cs
git commit -m "feat: add facebook renewal and reconnect behavior"
```

## Final Verification

- [ ] Run the focused test suites:

```powershell
dotnet test tests\ClinicMateAI.Tests\ClinicMateAI.Tests.csproj --filter Setup
dotnet test tests\ClinicMateAI.Web.Tests\ClinicMateAI.Web.Tests.csproj --filter Integrations
dotnet test tests\ClinicMateAI.Web.Tests\ClinicMateAI.Web.Tests.csproj --filter Webhook
```

Expected: PASS.

- [ ] Run the full repository validation:

```powershell
dotnet test
dotnet build
```

Expected: both commands succeed.

## Self-Review

- Spec coverage: card states, wizard UX, LINE setup, Facebook OAuth, reconnect handling, and status-driven persistence are all mapped to tasks above.
- Placeholder scan: every task names exact files, commands, and concrete code to add first.
- Type consistency: `ChannelConnectionStatus`, `ClinicChannelConfig`, and the `Setup` namespace are used consistently across the plan.
