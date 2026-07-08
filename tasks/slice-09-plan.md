# Slice 09 Plan: Account Invitation Flow

## Overview

Slice 09 implements the next Should Have roadmap item: account invitations. The goal is a minimal server-rendered flow where an administrator can invite a user by email, AuthNet sends a single-use invitation link, and the invited user completes account creation by setting a username/password. This slice keeps public registration disabled by default and gives host applications a controlled onboarding path without introducing bulk invites, role assignment during invite, organization membership, or API/JWT flows.

## Scope

In scope:

- Add a persisted invitation record to the existing AuthNet EF Core persistence model.
- Add PostgreSQL migration coverage for invitation storage.
- Add admin Razor Pages to list invitations and create a new invitation.
- Add an anonymous invitation acceptance Razor Page under the existing AuthNet route prefix.
- Generate cryptographically strong invitation tokens and store only a protected or hashed token value.
- Send invitation links through the existing `IAuthNetEmailSender`.
- Create a local Identity user when a valid unused invitation is accepted.
- Mark accepted invitations as used and prevent reuse.
- Reject expired, invalid, and already accepted invitations.
- Keep invited email confirmation behavior explicit and documented.
- Add focused integration tests for create, email delivery, accept, expired, reused, and invalid-token behavior.
- Update user, developer, architecture, roadmap, and context docs.

Out of scope:

- Bulk invitations.
- Role assignment during invite.
- Custom invitation email templates beyond the existing email sender contract.
- Invitation resend/cancel workflows unless required for basic correctness.
- Organization/team membership.
- External-provider-only invitation acceptance.
- API endpoints, JWT flows, SPA flows, or mobile flows.
- Arbitrary permissions or fine-grained admin policies.

## Proposed Routes

With `AccountRoutePrefix` set to `/auth`, routes should be:

- `/auth/admin/invitations`
- `/auth/admin/invitations/new`
- `/auth/invitations/accept`

Admin routes require the fixed `Administrator` role.

The acceptance route is anonymous but requires a valid invitation token.

## Architecture Decisions

- Store invitations in `AuthNet.Persistence.Postgres` because invitation state must survive restarts, be queryable by admins, and prevent reuse.
- Keep invitation UI in `AuthNet.UI.Razor`; route registration remains in `AuthNet.AspNetCore`.
- Use the existing `IAuthNetEmailSender` contract instead of introducing a dedicated email provider.
- Use the existing Identity `UserManager<AuthNetUser>` for user creation and password validation.
- Do not bypass Identity password policy during invitation acceptance.
- Treat the invited email as verified on successful acceptance by default, because possession of the invitation link sent to that email is the onboarding proof for this slice.
- Preserve public registration settings; invitations are a separate admin-driven onboarding path.
- Do not assign roles during invitation acceptance in this slice; administrators can use the existing user detail page afterward.

## Data Model

Proposed invitation fields:

- `Id`
- `Email`
- `NormalizedEmail`
- `TokenHash`
- `CreatedAtUtc`
- `ExpiresAtUtc`
- `AcceptedAtUtc`
- `AcceptedByUserId`
- `CreatedByUserId`

Recommended constraints:

- Unique `TokenHash`.
- Index `NormalizedEmail`.
- Optional filtered uniqueness for active pending invitations by `NormalizedEmail` if EF/PostgreSQL support is straightforward; otherwise enforce duplicate pending invites in application logic and document the behavior.

## Task List

### Phase 1: Persistence Foundation

- [x] Task 1: Add invitation entity and DbContext mapping.
- [x] Task 2: Add invitation token helper and lifecycle rules.
- [x] Task 3: Add PostgreSQL migration and snapshot update.

### Checkpoint: Foundation

- [x] Invitation persistence tests pass.
- [x] Migration builds cleanly.
- [x] No public registration behavior changes.

### Phase 2: Admin Invitation Creation

- [x] Task 4: Add admin invitation routes and list page.
- [x] Task 5: Add admin invitation creation page.
- [x] Task 6: Send invitation email with acceptance link.

### Checkpoint: Admin Flow

- [x] Administrator can create an invitation.
- [x] Invitation email is emitted through `IAuthNetEmailSender`.
- [x] Anonymous and non-admin users cannot access admin invitation pages.

### Phase 3: Invitation Acceptance

- [x] Task 7: Add invitation acceptance page.
- [x] Task 8: Create user and mark invitation accepted atomically enough for the current persistence model.
- [x] Task 9: Add expired, invalid, and reused invitation handling.

### Checkpoint: Acceptance Flow

- [x] Invited user can set credentials and sign in.
- [x] Accepted invitation cannot be reused.
- [x] Expired and invalid tokens do not create users.

### Phase 4: Tests, Docs, and Review

- [x] Task 10: Add focused invitation integration tests.
- [x] Task 11: Update user, developer, architecture, roadmap, and context docs.
- [x] Task 12: Final verification and local review.
- [x] Task 13: Commit Slice 09.

### Checkpoint: Complete

- [x] `.\scripts\verify.ps1` passes.
- [x] Slice 09 artifacts are complete and named with `slice-09`.
- [x] Invitation flow uses Identity and existing AuthNet email/persistence seams.
- [x] No role assignment during invite, API/JWT, SPA, bulk invite, or organization feature is introduced.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Invitation token leakage allows account creation | High | Generate high-entropy tokens, store only hashes, and reject reused/expired tokens. |
| Duplicate pending invitations create confusing onboarding state | Medium | Normalize email and reject duplicate pending invitations in the create page. |
| Invitation acceptance races create duplicate users | High | Use Identity unique email enforcement and mark invitation accepted after successful user creation; add tests for reuse. |
| Email confirmation semantics become unclear | Medium | Document that accepting an email-delivered invite confirms the invited email for this slice. |
| Migration generation depends on local EF tooling | Medium | Prefer existing project-local tooling if available; otherwise add migration manually and verify build/tests. |
| Scope expands into full provisioning | Medium | Defer roles, teams, bulk invites, resend/cancel, and external-provider-only acceptance. |

## Open Questions

- Should invitation expiration be configurable in `AuthNetOptions`, or fixed initially, for example 7 days?
- Should admins be able to enter a display name on the invitation, or only the email?
- Should acceptance automatically sign in the new user after successful account creation?

## Recommended Default

Use a 7-day invitation expiration, collect only email on the admin create page, collect username/password/display name on acceptance, mark the invited email confirmed after successful acceptance, and sign in the new user immediately after account creation.
