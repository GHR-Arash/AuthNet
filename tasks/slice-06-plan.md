# Slice 06 Plan: Admin User Management UI

## Overview

Slice 06 starts the first Should Have roadmap item: Admin user management UI. The goal is a minimal, server-rendered, built-in AuthNet admin surface that lets authorized administrators find users, inspect account status, and perform safe account-management actions using ASP.NET Core Identity primitives. This slice should not introduce API/JWT flows, a custom permission system, or a broad admin platform.

## Scope

In scope:

- Add a built-in AuthNet admin area under the existing AuthNet route prefix.
- Protect all admin pages with standard ASP.NET Core authorization and the `Administrator` role.
- List users with basic search and pagination.
- Show a user detail page with email, display name, phone number, email confirmation state, lockout state, external login count, and roles.
- Support safe first actions: confirm/unconfirm email, lock/unlock user, and reset access-failed count.
- Document how a host creates/assigns the `Administrator` role.
- Add integration tests for authorization, listing, searching, and admin actions.

Out of scope:

- Creating administrators automatically.
- Role management UI beyond displaying roles.
- User creation/invitation flow.
- Deleting users.
- Impersonation.
- Audit event storage.
- API endpoints for admin features.
- Fine-grained permissions.

## Proposed Routes

With `AccountRoutePrefix` set to `/auth`, admin routes should be:

- `/auth/admin/users`
- `/auth/admin/users/{id}`

Route names should continue to be configured from `AccountRoutePrefix`, not a separate admin prefix for this first slice.

## Architecture Decisions

- Keep the admin UI in `AuthNet.UI.Razor` beside the account UI so package consumers get a coherent built-in Razor UI.
- Use `UserManager<AuthNetUser>` and `RoleManager<IdentityRole>` instead of custom persistence queries where practical.
- Use EF Core query composition only for list/search/pagination where Identity APIs do not provide efficient list querying.
- Use `[Authorize(Roles = "Administrator")]` for this slice to match the existing role infrastructure and sample host guidance.
- Keep mutation actions POST-only with antiforgery protection.
- Prefer account-state actions over destructive actions for the first admin UI slice.

## Task List

### Phase 1: Admin Routing and Authorization

- [x] Task 1: Add admin route conventions and route docs.
- [x] Task 2: Add admin user list page protected by `Administrator`.
- [x] Task 3: Add authorization tests for admin routes.

### Checkpoint: Admin Shell

- [x] Build passes.
- [x] Unauthenticated users are challenged.
- [x] Authenticated non-admin users are forbidden or redirected.
- [x] Admin users can load the user list.

### Phase 2: User Detail and Actions

- [x] Task 4: Add admin user detail page.
- [x] Task 5: Add email confirmation and lockout actions.
- [x] Task 6: Add integration tests for detail and actions.

### Checkpoint: Admin Management

- [x] Admin can search users.
- [x] Admin can view user details.
- [x] Admin can confirm/unconfirm email.
- [x] Admin can lock/unlock users.

### Phase 3: Docs and Closeout

- [x] Task 7: Update user, developer, architecture, and route docs.
- [x] Task 8: Update compact context docs.
- [x] Task 9: Final verification and local review.
- [x] Task 10: Commit Slice 06.

### Checkpoint: Complete

- [x] `.\scripts\verify.ps1` passes.
- [x] Admin UI scope remains server-rendered and role-based.
- [x] Slice 06 artifacts are complete and named with `slice-06`.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Admin UI accidentally becomes a broad permissions system | High | Keep only `Administrator` role and defer fine-grained permissions. |
| Dangerous user actions ship too early | High | Avoid delete/impersonation/password viewing; use reversible state actions first. |
| User listing leaks to non-admins | High | Add route-level authorization tests for anonymous, non-admin, and admin users. |
| EF query behavior differs between InMemory tests and PostgreSQL | Medium | Keep list/search simple and validate through Identity/EF APIs already used by the app. |
| Built-in admin routes conflict with host routes | Medium | Keep routes under `AccountRoutePrefix` and document them. |

## Open Questions

- Should the admin role name be configurable later, or fixed as `Administrator` for this slice?
- Should locking a user use a far-future lockout date or a configured duration?
- Should admins be able to edit display name and phone number in this slice, or should detail remain mostly read-only?
- Should role assignment UI be the next admin slice after user state management?

## Recommended Default

Use a fixed `Administrator` role for Slice 06, implement list/detail plus reversible account-state actions, and defer role assignment, invitation, deletion, and audit events to later slices.
