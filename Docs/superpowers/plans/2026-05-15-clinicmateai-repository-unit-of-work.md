# ClinicMateAI Repository And Unit Of Work Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add command-pattern application contracts, FluentValidation input validation, a dedicated business logic layer, and tenant-safe Repository/Unit of Work patterns so use cases persist data through explicit abstractions instead of direct `DbContext` usage.

**Architecture:** Keep `Application` contract-only: commands, queries, DTOs, repository interfaces, provider interfaces, and handler/use-case interfaces. Put command/query handler implementations, FluentValidation validators, and all business workflow logic in a new `Logic` project. Keep repository implementations in `Infrastructure`, use `AppDbContext` as the transaction boundary, expose commit through `IUnitOfWork`, and enforce `ClinicId` filtering in repository methods so cross-tenant reads/writes are blocked by design.

**Tech Stack:** .NET 8, C#, EF Core, Npgsql/PostgreSQL, FluentValidation, xUnit, FluentAssertions.

---

## File Structure

- Create: `src/ClinicMateAI.Logic/ClinicMateAI.Logic.csproj`
- Create: `src/ClinicMateAI.Application/Abstractions/Messaging/ICommandHandler.cs`
- Create: `src/ClinicMateAI.Application/Abstractions/Messaging/IQueryHandler.cs`
- Create: `src/ClinicMateAI.Application/Abstractions/Persistence/IUnitOfWork.cs`
- Create: `src/ClinicMateAI.Application/Abstractions/Persistence/IClinicRepository.cs`
- Create: `src/ClinicMateAI.Application/Abstractions/Persistence/IConversationRepository.cs`
- Create: `src/ClinicMateAI.Application/Abstractions/Persistence/IMessageRepository.cs`
- Create: `src/ClinicMateAI.Application/Messaging/ReceiveMessageCommand.cs`
- Create: `src/ClinicMateAI.Application/Messaging/ReceiveMessageResult.cs`
- Create: `src/ClinicMateAI.Application/Messaging/IReceiveMessageHandler.cs`
- Create: `src/ClinicMateAI.Logic/Messaging/ReceiveMessageCommandValidator.cs`
- Create: `src/ClinicMateAI.Logic/Messaging/ReceiveMessageHandler.cs`
- Move later: existing business services from `src/ClinicMateAI.Application/*` into `src/ClinicMateAI.Logic/*` when each workflow is touched.
- Create: `src/ClinicMateAI.Infrastructure/Persistence/UnitOfWork.cs`
- Create: `src/ClinicMateAI.Infrastructure/Persistence/ClinicRepository.cs`
- Create: `src/ClinicMateAI.Infrastructure/Persistence/ConversationRepository.cs`
- Create: `src/ClinicMateAI.Infrastructure/Persistence/MessageRepository.cs`
- Modify: `src/ClinicMateAI.Web/Program.cs`
- Modify: `ClinicMateAI.sln`
- Modify: project references and package references for `ClinicMateAI.Logic`, `ClinicMateAI.Web`, and tests.
- Test: `tests/ClinicMateAI.Tests/Messaging/ReceiveMessageCommandValidatorTests.cs`
- Test: `tests/ClinicMateAI.Tests/Messaging/ReceiveMessageHandlerTests.cs`
- Test: `tests/ClinicMateAI.Tests/Persistence/RepositoryTenantBoundaryTests.cs`
- Test: `tests/ClinicMateAI.Tests/Persistence/UnitOfWorkTests.cs`

## Task 1: Add Logic project and command contracts

**Files:**
- Create: `src/ClinicMateAI.Logic/ClinicMateAI.Logic.csproj`
- Create: `src/ClinicMateAI.Application/Abstractions/Messaging/ICommandHandler.cs`
- Create: `src/ClinicMateAI.Application/Abstractions/Messaging/IQueryHandler.cs`
- Create: `src/ClinicMateAI.Application/Messaging/ReceiveMessageCommand.cs`
- Create: `src/ClinicMateAI.Application/Messaging/ReceiveMessageResult.cs`
- Create: `src/ClinicMateAI.Application/Messaging/IReceiveMessageHandler.cs`
- Modify: `ClinicMateAI.sln`
- Modify: `src/ClinicMateAI.Web/ClinicMateAI.Web.csproj`
- Modify: `tests/ClinicMateAI.Tests/ClinicMateAI.Tests.csproj`

- [ ] **Step 1: Add the Logic class library**

Run:

```powershell
dotnet new classlib -n ClinicMateAI.Logic -o src/ClinicMateAI.Logic
dotnet sln add src/ClinicMateAI.Logic
dotnet add src/ClinicMateAI.Logic reference src/ClinicMateAI.Application
dotnet add src/ClinicMateAI.Logic reference src/ClinicMateAI.Domain
dotnet add src/ClinicMateAI.Web reference src/ClinicMateAI.Logic
dotnet add tests/ClinicMateAI.Tests reference src/ClinicMateAI.Logic
dotnet add src/ClinicMateAI.Logic package FluentValidation
dotnet add src/ClinicMateAI.Web package FluentValidation.DependencyInjectionExtensions
```

Expected: project is created and references compile.

- [ ] **Step 2: Define generic command/query handler contracts**

Create:
- `ICommandHandler<TCommand, TResult>`
- `IQueryHandler<TQuery, TResult>`

Both interfaces expose:
- `Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);`

- [ ] **Step 3: Define receive-message command contract**

Create `ReceiveMessageCommand`, `ReceiveMessageResult`, and `IReceiveMessageHandler` in `Application/Messaging`.

Required command fields:
- `Guid ClinicId`
- `string Channel`
- `string ExternalConversationId`
- `string CustomerDisplayName`
- `string Text`
- `DateTimeOffset ReceivedAt`

Required result fields:
- `Guid ConversationId`
- `Guid MessageId`
- `bool RequiresHandoff`
- `string ReplyText`

- [ ] **Step 4: Verify Application remains contract-only**

Run:

```powershell
dotnet build src/ClinicMateAI.Application/ClinicMateAI.Application.csproj
```

Expected: build succeeds and `Application` contains no EF Core or provider implementation references.

## Task 2: Add FluentValidation validators in Logic

**Files:**
- Create: `tests/ClinicMateAI.Tests/Messaging/ReceiveMessageCommandValidatorTests.cs`
- Create: `src/ClinicMateAI.Logic/Messaging/ReceiveMessageCommandValidator.cs`

- [ ] **Step 1: Write failing validator tests**

Test cases:
1. `ClinicId` must not be empty.
2. `Channel` must be `LINE` or `Facebook`.
3. `ExternalConversationId`, `CustomerDisplayName`, and `Text` must not be empty.
4. `Text` must be limited to a practical inbound message length.

- [ ] **Step 2: Run tests to verify failure**

Run:

```powershell
dotnet test tests/ClinicMateAI.Tests/ClinicMateAI.Tests.csproj --filter ReceiveMessageCommandValidatorTests
```

Expected: fails before `ReceiveMessageCommandValidator` exists.

- [ ] **Step 3: Implement validator in Logic**

Create `ReceiveMessageCommandValidator : AbstractValidator<ReceiveMessageCommand>` in `Logic`.

Rules:
- `ClinicId` is not empty.
- `Channel` is not empty and must be one of `LINE` or `Facebook`.
- `ExternalConversationId` is not empty and max 200 characters.
- `CustomerDisplayName` is not empty and max 200 characters.
- `Text` is not empty and max 4000 characters.
- `ReceivedAt` is not default.

- [ ] **Step 4: Re-run focused validator tests**

Run same command above.

Expected: passes.

## Task 3: Add persistence abstractions in Application

**Files:**
- Create: `src/ClinicMateAI.Application/Abstractions/Persistence/IUnitOfWork.cs`
- Create: `src/ClinicMateAI.Application/Abstractions/Persistence/IClinicRepository.cs`
- Create: `src/ClinicMateAI.Application/Abstractions/Persistence/IConversationRepository.cs`
- Create: `src/ClinicMateAI.Application/Abstractions/Persistence/IMessageRepository.cs`

- [ ] **Step 1: Define Unit of Work contract**

Create `IUnitOfWork` with:
- `Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);`

- [ ] **Step 2: Define clinic repository contract**

Add methods with explicit tenant-safe intent:
- `Task<Clinic?> GetByIdAsync(Guid clinicId, CancellationToken cancellationToken = default);`
- `Task AddAsync(Clinic clinic, CancellationToken cancellationToken = default);`

- [ ] **Step 3: Define conversation and message repository contracts**

Add methods requiring `clinicId` in read paths:
- conversation lookup by `conversationId` + `clinicId`
- list recent conversations by `clinicId`
- add/update conversation
- add/list messages by `conversationId` + `clinicId`

- [ ] **Step 4: Verify compile**

Run:

```powershell
dotnet build src/ClinicMateAI.Application/ClinicMateAI.Application.csproj
```

Expected: build succeeds with new interfaces.

## Task 4: Implement Unit of Work and repositories in Infrastructure

**Files:**
- Create: `src/ClinicMateAI.Infrastructure/Persistence/UnitOfWork.cs`
- Create: `src/ClinicMateAI.Infrastructure/Persistence/ClinicRepository.cs`
- Create: `src/ClinicMateAI.Infrastructure/Persistence/ConversationRepository.cs`
- Create: `src/ClinicMateAI.Infrastructure/Persistence/MessageRepository.cs`
- Modify: `src/ClinicMateAI.Infrastructure/Data/AppDbContext.cs` (only if needed for DbSet exposure/config)

- [ ] **Step 1: Implement `UnitOfWork`**

Wrap `AppDbContext.SaveChangesAsync` and keep no domain logic inside this class.

- [ ] **Step 2: Implement `ClinicRepository`**

Use EF queries with strict `ClinicId` matching for reads and normal tracked entity add/update behavior for writes.

- [ ] **Step 3: Implement conversation/message repositories**

Ensure every read method enforces both aggregate id and `ClinicId` predicate where applicable.

- [ ] **Step 4: Verify compile**

Run:

```powershell
dotnet build src/ClinicMateAI.Infrastructure/ClinicMateAI.Infrastructure.csproj
```

Expected: build succeeds.

## Task 5: Add receive-message command handler in Logic

**Files:**
- Create: `tests/ClinicMateAI.Tests/Messaging/ReceiveMessageHandlerTests.cs`
- Create: `src/ClinicMateAI.Logic/Messaging/ReceiveMessageHandler.cs`

- [ ] **Step 1: Write failing handler test**

Test that `ReceiveMessageHandler`:
1. runs `ReceiveMessageCommandValidator` before persistence,
2. returns or throws a controlled validation failure for invalid commands,
3. creates or updates a tenant-scoped conversation for valid commands,
4. persists the incoming message,
5. uses `IUnitOfWork.SaveChangesAsync`,
6. returns a Thai service-minded reply containing `คุณลูกค้า`.

- [ ] **Step 2: Run test to verify failure**

Run:

```powershell
dotnet test tests/ClinicMateAI.Tests/ClinicMateAI.Tests.csproj --filter ReceiveMessageHandlerTests
```

Expected: fails before `ReceiveMessageHandler` exists.

- [ ] **Step 3: Implement handler in Logic**

The handler should coordinate FluentValidation, repositories, AI safety/orchestration contracts, domain rules, and `IUnitOfWork`. Keep no EF Core code in the handler.

- [ ] **Step 4: Re-run focused tests**

Run same command above.

Expected: passes.

## Task 6: Wire DI registrations in Web

**Files:**
- Modify: `src/ClinicMateAI.Web/Program.cs`

- [ ] **Step 1: Register interfaces to implementations**

Add scoped registrations:
- `IUnitOfWork -> UnitOfWork`
- `IClinicRepository -> ClinicRepository`
- `IConversationRepository -> ConversationRepository`
- `IMessageRepository -> MessageRepository`
- `IValidator<ReceiveMessageCommand> -> ReceiveMessageCommandValidator`
- `IReceiveMessageHandler -> ReceiveMessageHandler`

- [ ] **Step 2: Verify host build**

Run:

```powershell
dotnet build src/ClinicMateAI.Web/ClinicMateAI.Web.csproj
```

Expected: web project builds with DI graph resolved.

## Task 7: Add tenant boundary tests for repositories

**Files:**
- Create: `tests/ClinicMateAI.Tests/Persistence/RepositoryTenantBoundaryTests.cs`

- [ ] **Step 1: Write failing tests**

Test cases:
1. Querying a conversation with wrong `ClinicId` returns null.
2. Listing messages by `conversationId` returns only records belonging to the same clinic.
3. Listing recent conversations excludes other clinics.

- [ ] **Step 2: Run tests to verify failure**

Run:

```powershell
dotnet test tests/ClinicMateAI.Tests/ClinicMateAI.Tests.csproj --filter RepositoryTenantBoundaryTests
```

Expected: fails before repository behavior is complete.

- [ ] **Step 3: Complete implementation until tests pass**

Adjust repository predicates and includes as required.

- [ ] **Step 4: Re-run focused tests**

Run same command above.

Expected: passes.

## Task 8: Add Unit of Work commit tests

**Files:**
- Create: `tests/ClinicMateAI.Tests/Persistence/UnitOfWorkTests.cs`

- [ ] **Step 1: Write failing test**

Test that adding an entity through repository is not committed until `IUnitOfWork.SaveChangesAsync` is called.

- [ ] **Step 2: Run test to verify failure**

Run:

```powershell
dotnet test tests/ClinicMateAI.Tests/ClinicMateAI.Tests.csproj --filter UnitOfWorkTests
```

Expected: fails before commit flow is enforced.

- [ ] **Step 3: Finalize implementation**

Ensure app/services call `SaveChangesAsync` at use-case boundary.

- [ ] **Step 4: Re-run focused tests**

Run same command above.

Expected: passes.

## Task 9: Move existing business logic out of Application incrementally

**Files:**
- Move later as touched: `src/ClinicMateAI.Application/Ai/*`
- Move later as touched: `src/ClinicMateAI.Application/Appointments/*`
- Move later as touched: `src/ClinicMateAI.Application/Packages/*`
- Update namespaces/tests as each file moves.

- [ ] **Step 1: Identify existing implementation files in Application**

Run:

```powershell
rg -n "class .*Service|class .*Handler|class .*Orchestrator|class .*Decider|class .*Detector|interface I.*Provider" src/ClinicMateAI.Application
```

Expected: list all current implementation classes and provider contracts.

- [ ] **Step 2: Move implementation classes to Logic when workflow is touched**

Rules:
- Interfaces and DTOs stay in `Application`.
- Concrete orchestrators, deciders, detectors, services, FluentValidation validators, and handlers move to `Logic`.
- Tests may reference `Logic` for concrete implementation tests.

- [ ] **Step 3: Keep Application build free of implementation dependencies**

Run:

```powershell
dotnet build src/ClinicMateAI.Application/ClinicMateAI.Application.csproj
```

Expected: build succeeds with contracts only.

## Task 10: Full verification

**Files:**
- Modify: none (verification only)

- [ ] **Step 1: Run unit tests**

```powershell
dotnet test tests/ClinicMateAI.Tests/ClinicMateAI.Tests.csproj
```

Expected: all tests pass.

- [ ] **Step 2: Run solution build**

```powershell
dotnet build
```

Expected: build succeeds.

- [ ] **Step 3: Update progress trackers**

Mark repository/UoW-related checklist items as complete in MVP implementation tracking docs if all checks pass.
