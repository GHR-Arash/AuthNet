# Slice 11 Todo: Admin Audit Events

## Task 1: Add audit event entity and DbContext mapping

**Description:** Add a persisted audit event model to the existing AuthNet persistence project and expose it through `AuthNetDbContext`.

**Acceptance criteria:**
- [x] Audit events include id, timestamp, action, outcome, actor id/email, target id/email, and metadata.
- [x] `AuthNetDbContext` exposes audit events through a `DbSet`.
- [x] EF mapping includes required fields, string lengths, and indexes for timestamp, action, actor, and target.

**Verification:**
- [x] Persistence-focused tests can create and query an audit event with EF Core InMemory.

**Dependencies:** None

**Files likely touched:**
- `src/AuthNet.Persistence.Postgres/AuthNetAuditEvent.cs`
- `src/AuthNet.Persistence.Postgres/AuthNetDbContext.cs`
- `tests/AuthNet.Tests/Integration/AuthNetAuditTests.cs`

**Estimated scope:** M

## Task 2: Add audit writer service and DI registration

**Description:** Add a lightweight audit writer abstraction and implementation that persists audit events through the existing DbContext.

**Acceptance criteria:**
- [x] `IAuthNetAuditWriter` can record a successful admin action.
- [x] Writer resolves the current actor from `UserManager<AuthNetUser>` and `ClaimsPrincipal`.
- [x] Writer accepts optional target and metadata fields.
- [x] `AddAuthNet` registers the writer.

**Verification:**
- [x] Integration tests can resolve and use the writer from the AuthNet test host.

**Dependencies:** Task 1

**Files likely touched:**
- `src/AuthNet.Core` or `src/AuthNet.UI.Razor` audit writer files
- `src/AuthNet.AspNetCore/AuthNetServiceCollectionExtensions.cs`
- `tests/AuthNet.Tests/Integration/AuthNetAuditTests.cs`

**Estimated scope:** M

## Task 3: Add PostgreSQL migration and snapshot updates

**Description:** Add a migration for the audit event table and update the EF Core model snapshot.

**Acceptance criteria:**
- [x] Migration creates `AuthNetAuditEvents`.
- [x] Migration creates indexes for timestamp, action, actor id, and target user id.
- [x] Model snapshot matches the new entity mapping.

**Verification:**
- [x] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`

**Dependencies:** Tasks 1 and 2

**Files likely touched:**
- `src/AuthNet.Persistence.Postgres/Migrations/*_AddAuthNetAuditEvents.cs`
- `src/AuthNet.Persistence.Postgres/Migrations/*_AddAuthNetAuditEvents.Designer.cs`
- `src/AuthNet.Persistence.Postgres/Migrations/AuthNetDbContextModelSnapshot.cs`

**Estimated scope:** M

## Task 4: Record admin user detail actions

**Description:** Record audit events for existing user detail mutations.

**Acceptance criteria:**
- [x] Confirm email records an audit event.
- [x] Unconfirm email records an audit event.
- [x] Lock user records an audit event.
- [x] Unlock user records an audit event.
- [x] Reset access failures records an audit event.
- [x] Grant administrator records an audit event.
- [x] Remove administrator records an audit event.

**Verification:**
- [x] Integration tests perform each action and verify persisted audit event fields.

**Dependencies:** Tasks 1 and 2

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Detail.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetAuditTests.cs`

**Estimated scope:** M

## Task 5: Record direct user creation

**Description:** Record audit events when administrators directly create local users.

**Acceptance criteria:**
- [x] Successful direct user creation records an audit event.
- [x] Event target points to the created user.
- [x] Metadata does not include password or other secret input.
- [x] Optional administrator assignment remains separately visible through metadata or action naming.

**Verification:**
- [x] Integration test creates a user and verifies the audit event.

**Dependencies:** Tasks 1 and 2

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Create.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetAuditTests.cs`

**Estimated scope:** S

## Task 6: Record invitation creation

**Description:** Record audit events when administrators create account invitations.

**Acceptance criteria:**
- [x] Successful invitation creation records an audit event.
- [x] Event target email is the invited email.
- [x] Metadata does not include raw invitation token or acceptance URL.

**Verification:**
- [x] Integration test creates an invitation and verifies the audit event.

**Dependencies:** Tasks 1 and 2

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Invitations/Create.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetAuditTests.cs`

**Estimated scope:** S

## Task 7: Add admin audit list and filters

**Description:** Add an admin-only Razor Page that lists recent audit events and supports simple filtering.

**Acceptance criteria:**
- [x] `/auth/admin/audit` maps to an admin-only page.
- [x] Page lists timestamp, action, outcome, actor, target, and metadata.
- [x] Page supports filters for action, actor, target, from date, and to date.
- [x] Page limits results to a fixed page size.

**Verification:**
- [x] Integration tests cover route protection and filtered list output.

**Dependencies:** Tasks 1 and 2

**Files likely touched:**
- `src/AuthNet.AspNetCore/AuthNetServiceCollectionExtensions.cs`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Audit/Index.cshtml`
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Audit/Index.cshtml.cs`
- `tests/AuthNet.Tests/Integration/AuthNetAuditTests.cs`

**Estimated scope:** M

## Task 8: Link audit page from built-in admin and sample host UI

**Description:** Make audit review discoverable from built-in admin pages and the sample project.

**Acceptance criteria:**
- [x] Built-in admin user list links to `/auth/admin/audit`.
- [x] Sample home page links to `/auth/admin/audit`.
- [x] Sample `/Admin` page links to `/auth/admin/audit`.
- [x] Sample navbar links to `/auth/admin/audit` if navigation remains readable.

**Verification:**
- [x] Build succeeds and manual HTML review confirms links point to the expected route.

**Dependencies:** Task 7

**Files likely touched:**
- `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Users/Index.cshtml`
- `samples/AuthNet.SampleHost/Pages/Index.cshtml`
- `samples/AuthNet.SampleHost/Pages/Admin.cshtml`
- `samples/AuthNet.SampleHost/Pages/Shared/_Layout.cshtml`

**Estimated scope:** S

## Task 9: Add focused audit integration tests

**Description:** Add integration tests for audit event creation, route protection, and list filtering.

**Acceptance criteria:**
- [x] Tests cover route protection for `/auth/admin/audit`.
- [x] Tests cover direct user creation audit.
- [x] Tests cover invitation creation audit.
- [x] Tests cover representative user detail action audits.
- [x] Tests cover audit list filtering by action, actor, and target.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetAuditTests`

**Dependencies:** Tasks 1 through 8

**Files likely touched:**
- `tests/AuthNet.Tests/Integration/AuthNetAuditTests.cs`
- `tests/AuthNet.Tests/Integration/AuthNetTestHost.cs`

**Estimated scope:** M

## Task 10: Update docs and project memory

**Description:** Document audit event behavior, route, limits, and current implementation state.

**Acceptance criteria:**
- [x] User docs list `/auth/admin/audit`.
- [x] Developer docs include the focused audit test command.
- [x] Architecture context records audit persistence and UI.
- [x] Functional requirements include admin audit events as Should Have.
- [x] `docs/next-iteration-context.md` and `context.md` record Slice 11 status when complete.

**Verification:**
- [x] Manual read-through for stale references to audit events as deferred.

**Dependencies:** Tasks 1 through 9

**Files likely touched:**
- `docs/users/account-pages.md`
- `docs/users/getting-started.md`
- `docs/developer/quick-start.md`
- `docs/architecture-context.md`
- `docs/functional-requirements.md`
- `docs/next-iteration-context.md`
- `context.md`

**Estimated scope:** S

## Task 11: Final verification and commit

**Description:** Run full verification, review the diff, and commit Slice 11.

**Acceptance criteria:**
- [x] Focused audit tests pass.
- [x] Full local verification passes.
- [x] `git diff --check` passes.
- [x] Working tree contains only intended Slice 11 changes plus any pre-existing unrelated user changes.
- [ ] Commit includes only intended Slice 11 changes.

**Verification:**
- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetAuditTests`
- [x] `.\scripts\verify.ps1`
- [x] `git diff --check`
- [x] `git status --short`

**Dependencies:** Tasks 1 through 10

**Files likely touched:** None unless review finds issues

**Estimated scope:** S
