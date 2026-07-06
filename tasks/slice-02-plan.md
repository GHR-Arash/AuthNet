# Implementation Plan: Slice 02 - Integration Hardening

## Overview

Slice 2 hardens the current Razor Pages/cookie-based AuthNet MVP before packaging. The goal is to prove the shipped account flows through host-level integration tests, clean up endpoint mapping so host apps keep control of their own Razor Pages, and update the developer/user docs to match the refined integration surface.

Artifact naming convention: plan and task artifacts use the slice number in the filename, for example `slice-02-plan.md` and `slice-02-todo.md`. If a future slice needs phase-specific artifacts, use `slice-02-phase-01-*.md`.

## Scope

In scope:

- Host-level integration test harness.
- Route and page behavior tests for the built-in account UI.
- Email sender test doubles for verification-link assertions.
- Endpoint mapping cleanup from broad `UseAuthNet()` mapping toward explicit AuthNet endpoint mapping.
- Documentation and compact context updates.

Out of scope:

- JWT/API authentication.
- Refresh tokens.
- SPA flows.
- Admin UI.
- MFA.
- Alternative database providers.
- NuGet publishing automation.

## Architecture Decisions

- Use ASP.NET Core integration testing with `WebApplicationFactory` or `TestServer` rather than unit-testing Razor Page internals.
- Keep tests focused on AuthNet's public integration contract: service registration, routes, redirects, email messages, and authentication behavior.
- Prefer an explicit endpoint-mapping API such as `MapAuthNet()` while keeping `UseAuthNet()` temporarily as a compatibility wrapper if practical.
- Use a test email sender/store for assertions instead of parsing logs.
- Do not introduce a real PostgreSQL dependency into the fast integration test suite unless a specific persistence behavior requires it.

## Task List

### Phase 1: Test Harness Foundation

- [ ] Task 1: Add ASP.NET Core integration test dependencies.
- [ ] Task 2: Build a minimal AuthNet test host factory.
- [ ] Task 3: Add reusable test email sender assertions.

### Checkpoint: Harness

- [ ] `.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore`
- [ ] `.\.dotnet\dotnet.exe test AuthNet.slnx --no-build`
- [ ] At least one integration test proves an AuthNet account page route renders.

### Phase 2: Account Flow Coverage

- [ ] Task 4: Cover route availability and auth requirements.
- [ ] Task 5: Cover registration and email confirmation message generation.
- [ ] Task 6: Cover resend confirmation and forgot-password email behavior.
- [ ] Task 7: Cover profile, verified email change, and change password access rules.
- [ ] Task 8: Cover external-login safety behavior at the route/callback boundary.

### Checkpoint: Flow Coverage

- [ ] Tests cover every built-in account route documented in `docs/users/account-pages.md`.
- [ ] Tests verify no user enumeration in resend/forgot-password responses.
- [ ] Tests verify existing accounts are not linked to external identities by email alone.

### Phase 3: Endpoint Mapping Cleanup

- [ ] Task 9: Introduce explicit AuthNet endpoint mapping API.
- [ ] Task 10: Update sample host and docs to use the explicit mapping API.
- [ ] Task 11: Preserve or intentionally deprecate `UseAuthNet()` with tests documenting behavior.

### Checkpoint: Mapping Contract

- [ ] Host-owned Razor Pages still work.
- [ ] AuthNet account pages still map under `AccountRoutePrefix`.
- [ ] The sample host starts and routes remain documented.

### Phase 4: Context and Review

- [ ] Task 12: Update compact docs and next-iteration memory.
- [ ] Task 13: Run final build/test and perform local code review.
- [ ] Task 14: Commit the completed slice.

### Checkpoint: Complete

- [ ] All integration and unit tests pass.
- [ ] Documentation matches the public integration API.
- [ ] `docs/architecture-context.md` and `docs/next-iteration-context.md` are compact and current.
- [ ] Working tree is clean after commit.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|---|---|---|
| Integration tests become slow or require PostgreSQL. | High | Start with test-host route and behavior coverage using replaceable services; add database-backed tests only where required. |
| Endpoint API rename breaks consumers. | Medium | Keep `UseAuthNet()` as a compatibility wrapper for one slice or document a deliberate breaking change before removing it. |
| Razor Pages are difficult to assert without brittle HTML tests. | Medium | Assert status codes, redirects, route availability, email messages, and security behavior first; keep HTML assertions minimal. |
| External login tests need complex OIDC setup. | Medium | Test callback boundary and linking rules with controlled authentication schemes before full OIDC provider simulation. |

## Open Questions

- Should `UseAuthNet()` remain as an alias for one release, or should the MVP switch directly to `MapAuthNet()` before packaging?
- Should the integration test suite use an in-memory store for speed, or should a small PostgreSQL-backed test category be added later?
- Should package-readiness be the next slice after this, or should we first add a real email sender sample?
