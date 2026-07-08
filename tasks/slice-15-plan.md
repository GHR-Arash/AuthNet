# Slice 15 Plan: OpenAPI Document Endpoint

## Overview

Slice 15 adds a machine-readable OpenAPI document endpoint for the same-origin AuthNet SPA JSON APIs introduced in Slice 14. The goal is a low-friction contract endpoint that package consumers and sample-host users can inspect without forcing a host application to adopt Swagger UI, Swashbuckle, Scalar, or a global OpenAPI setup.

## Scope

In scope:

- Add an OpenAPI 3.1 JSON document endpoint for AuthNet SPA APIs.
- Scope the document to AuthNet-owned JSON routes only.
- Use the configured `AuthNetOptions.AccountRoutePrefix`; default document route should be `/auth/api/openapi.json`.
- Document request and response bodies for session, profile, login, logout, register, forgot-password, and resend-confirmation.
- Document cookie-based same-origin authentication semantics.
- Add tests that parse the generated JSON and verify paths, operations, status codes, and core schemas.
- Add sample-host links or smoke guidance for the OpenAPI document endpoint.
- Update user/developer docs, architecture context, next-iteration context, and `context.md`.

Out of scope:

- Swagger UI, Scalar UI, or any bundled documentation UI.
- Global host OpenAPI document generation.
- Cross-origin CORS documentation beyond noting same-origin assumptions.
- JWT bearer security schemes.
- OpenAPI documents for Razor Pages, admin UI, invitation UI, or future token APIs.
- Client SDK generation.

## Architecture Decisions

- Keep the OpenAPI endpoint inside `AuthNet.Api`, because it documents the package-owned JSON API surface.
- Map the endpoint from `MapAuthNetApi()` so consumers get it through the existing `app.MapAuthNet()` integration path.
- Prefer a package-owned document builder over global host OpenAPI configuration. This keeps the document constrained to AuthNet SPA routes and avoids surprising host applications.
- Return a plain JSON OpenAPI document from `/auth/api/openapi.json`.
- Use `AuthNetOptions.NormalizedAccountRoutePrefix` when building path keys, so custom prefixes are reflected in the document.
- Model authentication as cookie-based browser session security, not JWT bearer auth.
- Keep the document deterministic for snapshot-style integration assertions.

## Candidate Route

With default `AccountRoutePrefix=/auth`:

- `GET /auth/api/openapi.json` returns the OpenAPI document for AuthNet SPA JSON endpoints.

With a custom prefix, for example `/accounts`:

- `GET /accounts/api/openapi.json`

## Document Contents

Minimum required OpenAPI content:

- `openapi`
- `info`
- `paths`
- `components.schemas`
- `components.securitySchemes`

Required documented paths:

- `GET /auth/api/session`
- `GET /auth/api/profile`
- `POST /auth/api/login`
- `POST /auth/api/logout`
- `POST /auth/api/register`
- `POST /auth/api/forgot-password`
- `POST /auth/api/resend-confirmation`

Required documented schemas:

- `AuthNetApiResult`
- `AuthNetApiError`
- `AuthNetSessionResponse`
- `AuthNetProfileResponse`
- `AuthNetLoginRequest`
- `AuthNetRegisterRequest`
- `AuthNetForgotPasswordRequest`
- `AuthNetResendConfirmationRequest`

## Task List

### Phase 1: Document Builder Foundation

- [x] Task 1: Add OpenAPI document contracts and builder.
- [x] Task 2: Add OpenAPI endpoint mapping under the configured API route root.

### Checkpoint: Foundation

- [x] Solution builds.
- [x] `GET /auth/api/openapi.json` returns valid JSON.
- [x] Custom account route prefix is reflected in the document route and path keys.

### Phase 2: Endpoint Contract Coverage

- [x] Task 3: Document all Slice 14 SPA API operations.
- [x] Task 4: Document schemas and cookie security semantics.
- [x] Task 5: Add focused OpenAPI integration tests.

### Checkpoint: Contract Coverage

- [x] Tests prove the OpenAPI document includes all current SPA API routes.
- [x] Tests prove the document excludes non-API Razor/admin routes.
- [x] Tests prove core request/response schemas are present.

### Phase 3: Sample, Docs, and Verification

- [x] Task 6: Add sample-host discoverability for the OpenAPI endpoint.
- [x] Task 7: Update docs, context, and planning artifacts.
- [x] Task 8: Run focused and full verification.

### Checkpoint: Complete

- [x] Focused OpenAPI tests pass.
- [x] Full verification passes.
- [x] Slice 15 artifacts are complete and named with `slice-15`.
- [x] Swagger UI and JWT bearer auth remain explicitly deferred.

## Risks and Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| OpenAPI setup leaks host app routes | Medium | Use a package-owned document builder scoped only to AuthNet SPA paths. |
| Manual document drifts from endpoint implementation | Medium | Add focused tests that assert every mapped SPA API path is present. |
| Dependency choice bloats the package | Medium | Avoid UI packages and keep any OpenAPI dependency minimal; prefer framework/runtime APIs where practical. |
| Cookie auth is misrepresented as bearer auth | High | Document an explicit cookie security scheme and same-origin browser assumption. |
| Route prefix handling is wrong for custom prefixes | Medium | Add custom-prefix integration tests. |

## Open Questions

- Resolved for this slice: the OpenAPI endpoint is always mapped through `MapAuthNet()`.
- Resolved for this slice: the document route is fixed at `/openapi.json` under the API root.
- Deferred: Swagger/Scalar UI can be considered later as a sample-host-only enhancement.
