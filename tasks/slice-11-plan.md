# Slice 11 Plan: Admin Audit Events

## Overview

Slice 11 adds basic persisted audit events for administrator actions. The goal is production traceability for account-management actions that already exist: direct user creation, invitation creation, fixed administrator role changes, email confirmation changes, lock/unlock, and access-failure reset. This slice keeps the design intentionally simple: one EF entity, one small writer service, and a server-rendered admin audit list.

## Scope

In scope:

- Add persisted audit events to `AuthNet.Persistence.Postgres`.
- Add a migration for `AuthNetAuditEvents`.
- Add a small audit writer service in the UI/admin layer or shared AuthNet layer.
- Record successful admin actions:
  - direct user creation
  - invitation creation
  - administrator role grant
  - administrator role removal
  - email confirm
  - email unconfirm
  - lock user
  - unlock user
  - access failure reset
- Store actor user id/email, target user id/email when available, action, outcome, timestamp, and compact metadata.
- Add an admin-only audit list page at `/auth/admin/audit`.
- Support simple filtering by action, actor, target, and date range.
- Link audit page from built-in admin UI and sample host.
- Add focused integration tests and docs.

Out of scope:

- Tamper-proof signing.
- Export/download.
- Webhook/SIEM integration.
- Full domain event bus.
- Audit retention policy.
- Auditing user-owned account flows.
- Auditing failed admin attempts.
- API/JWT/SPA audit endpoints.

## Proposed Route

With `AccountRoutePrefix` set to `/auth`:

- `/auth/admin/audit`

The route requires a signed-in user in the fixed `Administrator` role.

## Architecture Decisions

- Store audit events in `AuthNet.Persistence.Postgres` because audit state must survive restarts and share the existing EF persistence path.
- Use a lightweight `IAuthNetAuditWriter` service registered by `AddAuthNet`, backed by `AuthNetDbContext`.
- Keep metadata as a bounded string instead of introducing JSON document querying in this slice.
- Record only successful admin mutations initially to avoid ambiguous semantics around validation failures and authorization failures.
- Keep the list page in `AuthNet.UI.Razor/Areas/AuthNet/Pages/Admin/Audit`.
- Do not introduce a custom permission model; audit viewing uses the existing fixed `Administrator` role.

## Task List

### Phase 1: Persistence and Writer

- [x] Task 1: Add audit event entity and DbContext mapping.
- [x] Task 2: Add audit writer service and DI registration.
- [x] Task 3: Add PostgreSQL migration and model snapshot updates.

### Checkpoint: Foundation

- [x] Audit events can be persisted through EF Core InMemory.
- [x] Build succeeds.
- [x] No existing account behavior changes.

### Phase 2: Instrument Admin Actions

- [x] Task 4: Record admin user detail actions.
- [x] Task 5: Record direct user creation.
- [x] Task 6: Record invitation creation.

### Checkpoint: Event Creation

- [x] Mutating admin actions create expected audit events.
- [x] Events include actor, target, action, outcome, timestamp, and metadata.

### Phase 3: Audit UI and Sample Links

- [x] Task 7: Add admin audit list and filters.
- [x] Task 8: Link audit page from built-in admin and sample host UI.

### Checkpoint: Audit Review

- [x] `/auth/admin/audit` is admin-only.
- [x] Admin can filter audit events by action, actor, target, and date range.
- [x] Sample project exposes the audit page.

### Phase 4: Tests, Docs, and Verification

- [x] Task 9: Add focused audit integration tests.
- [x] Task 10: Update docs and project memory.
- [x] Task 11: Final verification and commit.

### Checkpoint: Complete

- [x] `.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-restore --filter AuthNetAuditTests` passes.
- [x] `.\scripts\verify.ps1` passes.
- [x] Slice 11 artifacts are complete and named with `slice-11`.
- [x] No export, retention, tamper-proofing, SIEM, API, or failed-attempt auditing is introduced.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Audit writes break admin actions | High | Keep audit writer simple and test each action; record after successful mutation. |
| Metadata becomes unbounded or sensitive | Medium | Store compact, explicit metadata only; do not store passwords, tokens, or raw invitation links. |
| Migration drift | Medium | Update migration, designer, and snapshot together; run full verification. |
| Scope expands into compliance platform | Medium | Defer signing, export, retention, SIEM, and full event bus. |
| UI filtering becomes expensive | Low | Limit page size and add basic indexes on timestamp, action, actor, and target. |

## Open Questions

- Should failed validation attempts be audited later?
- Should audit events be immutable at the database-permission level in a future hardening slice?

## Recommended Default

Record successful admin mutations only, with compact metadata and a read-only admin list. Treat audit hardening, retention, and export as future production-hardening slices.
