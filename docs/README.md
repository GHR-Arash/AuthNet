# AuthNet Documents

AuthNet is a reusable ASP.NET Identity and Access Management component.

The first MVP slice targets .NET 10 and plugs into ASP.NET applications as middleware. It provides ASP.NET Core Identity integration, Razor Pages account UI, same-origin SPA JSON account endpoints with an OpenAPI document, admin user-management UI, account invitations, cookie authentication, PostgreSQL-backed EF Core persistence, role infrastructure, generic OpenID Connect login, authenticator-app MFA, and email-based account flows.

API/JWT token authentication, refresh tokens, cross-origin SPA token flows, custom persistence stores, and provider-specific helpers are deferred.

## Documents

Product and planning artifacts:

- [Product Requirements Document](./prd.md)
- [Functional Requirements](./functional-requirements.md)
- [Integration Requirements](./integration-requirements.md)
- [MVP Roadmap](./mvp-roadmap.md)
- [Product Decisions](./product-decisions.md)
- [Architecture Context](./architecture-context.md)
- [Next Iteration Context](./next-iteration-context.md)
- [MVP Tasks](./tasks.md)

Developer guides:

- [Developer Onboarding](./developer/onboarding.md)
- [Developer Quick Start](./developer/quick-start.md)

Library user guides:

- [AuthNet User Guide](./users/getting-started.md)
- [Configuration Reference](./users/configuration.md)
- [Account Pages](./users/account-pages.md)

## Commands

Use the project-local .NET 10 SDK:

```powershell
..\.dotnet\dotnet.exe build ..\AuthNet.slnx
```

From the repository root:

```powershell
.\.dotnet\dotnet.exe restore AuthNet.slnx
.\.dotnet\dotnet.exe build AuthNet.slnx
.\.dotnet\dotnet.exe test AuthNet.slnx
```
