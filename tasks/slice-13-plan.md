# Slice 13 Plan: Role Management and Permission Enhancement

## Overview

Slice 13 promotes a bounded part of the previously deferred role and permission scope. AuthNet already uses ASP.NET Core Identity roles and has fixed `Administrator` role assignment. This slice adds server-rendered admin role creation, arbitrary role assignment on user detail, and a small permission model backed by Identity role claims.

## Scope

In scope:

- Add AuthNet permission constants and policy registration.
- Keep the existing `Administrator` role as the default superuser path.
- Add admin role list/create/detail pages.
- Let administrators assign and remove non-administrator roles from user detail.
- Preserve last-administrator protection for the `Administrator` role.
- Add role permission assignment using Identity role claims.
- Record audit events for role creation, role assignment/removal, and permission assignment/removal.
- Update sample navigation, user docs, and project memory.
- Add integration tests for role pages, role assignment, permission persistence, authorization, and route protection.

Out of scope:

- Custom role store or custom permission store.
- Role deletion.
- Permission groups edited by end users.
- Dynamic permission discovery from host applications.
- API/JWT authorization.
- Tenant-scoped roles or permissions.
- Invitation-time role assignment.
- Admin MFA reset, impersonation, audit export, or audit retention policy.

## Architecture Decisions

- Use ASP.NET Core Identity `IdentityRole` for roles.
- Use ASP.NET Core Identity role claims for permissions.
- Use a stable AuthNet permission claim type, for example `authnet.permission`.
- Introduce a small built-in permission catalog for AuthNet UI operations.
- Keep `Administrator` as a superuser role so existing admin bootstrap and links keep working.
- Admin pages should authorize with `Administrator` or the appropriate AuthNet permission.
- Do not add new EF entities or migrations unless Identity role claims prove insufficient.
- Role names should be validated through `RoleManager<IdentityRole>` and should reject duplicates.

## Candidate Built-In Permissions

Initial catalog:

- `authnet.users.view`
- `authnet.users.manage`
- `authnet.roles.view`
- `authnet.roles.manage`
- `authnet.invitations.manage`
- `authnet.audit.view`

Policy behavior:

- `Administrator` satisfies every AuthNet permission policy.
- A user satisfies a permission policy when any assigned role has the matching role claim.

## Task List

### Phase 1: Permission Foundation

- [x] Task 1: Add permission constants and policy registration.
- [x] Task 2: Add tests for permission policy behavior.

### Checkpoint: Permission Foundation

- [x] Existing `Administrator` access still works.
- [x] Permission claims can authorize a non-administrator user for a covered policy.
- [x] No persistence migration is required beyond Identity role claims.

### Phase 2: Role Management UI

- [x] Task 3: Add admin role list and create pages.
- [x] Task 4: Add admin role detail page with permission assignment.
- [x] Task 5: Add admin user arbitrary role assignment.

### Checkpoint: Role Management UI

- [x] Administrators can create roles.
- [x] Administrators can assign permissions to roles.
- [x] Administrators can assign and remove roles from users.
- [x] Last-administrator protection still holds.

### Phase 3: Integration, Docs, and Sample

- [x] Task 6: Add audit coverage for role and permission mutations.
- [x] Task 7: Update sample navigation and documentation.
- [x] Task 8: Update project memory and run final verification.

### Checkpoint: Complete

- [x] Focused role/permission tests pass.
- [x] Full verification passes.
- [x] Slice 13 artifacts are complete and named with `slice-13`.
- [x] Fine-grained permissions remain bounded to AuthNet UI permissions.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Scope expands into a full authorization platform | High | Keep permissions limited to AuthNet built-in UI operations and Identity role claims. |
| Existing admin access breaks | High | Keep `Administrator` as a superuser and test current admin routes. |
| Permission checks require stale role claims in cookies | Medium | Rely on ASP.NET Core Identity role claims through normal principal generation and document sign-in refresh expectations where needed. |
| Role assignment can remove last admin indirectly | High | Keep special handling for the `Administrator` role and test last-admin protection. |
| UI becomes hard to use with many roles | Medium | Start with simple list/detail pages and searchable user detail assignment; defer bulk tools. |

## Open Questions

- Should role names be case-preserving while uniqueness remains normalized through Identity?
- Should permission changes force sign-out/security-stamp refresh for affected users in this slice, or should docs state changes apply on next sign-in/session refresh?
- Should host applications be able to add custom permissions in this slice, or should that wait until the built-in catalog is stable?
