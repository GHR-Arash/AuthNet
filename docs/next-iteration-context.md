# Next Iteration Context

Compact memory for future development sessions. Read this first, then `docs/architecture-context.md` only if deeper context is needed.

## Current State

- Repo is a Git repository on `master`.
- Latest known commits:
  - `852d48e Add library user documentation`
  - `9cdebdf Add developer onboarding guides`
  - `9e71cae Implement AuthNet MVP slice one`
- MVP slice 1 is implemented and `docs/tasks.md` is fully checked off.
- Working tree was clean when this file was written.

## Implemented Product Surface

- .NET 10 solution: `AuthNet.slnx`.
- Project-local SDK: `.dotnet\dotnet.exe` (ignored).
- Projects:
  - `src/AuthNet.Core`
  - `src/AuthNet.AspNetCore`
  - `src/AuthNet.Persistence.Postgres`
  - `src/AuthNet.UI.Razor`
  - `src/AuthNet.ExternalProviders`
  - `samples/AuthNet.SampleHost`
  - `tests/AuthNet.Tests`
- Built-in Razor Pages account UI under `src/AuthNet.UI.Razor/Areas/AuthNet/Pages/Account`.
- ASP.NET Core Identity user/context in `AuthNet.Persistence.Postgres`.
- Initial PostgreSQL Identity migration exists in `src/AuthNet.Persistence.Postgres/Migrations`.
- Generic OpenID Connect extension exists in `AuthNet.ExternalProviders`.
- Sample host wires `AddAuthNet`, `UseAuthentication`, `UseAuthorization`, and `UseAuthNet`.

## Current Verification

Known passing commands:

```powershell
.\.dotnet\dotnet.exe restore AuthNet.slnx
.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore
.\.dotnet\dotnet.exe test AuthNet.slnx --no-build
```

Known passing sample startup:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
.\.dotnet\dotnet.exe run --project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --no-build --no-launch-profile --urls http://127.0.0.1:5127
```

Expected startup signal:

```text
Now listening on: http://127.0.0.1:5127
Application started.
```

## Important Constraints

- Keep MVP slice 1 server-rendered and cookie-based.
- Do not add API/JWT/refresh-token/SPA flows unless explicitly re-scoped.
- Do not replace ASP.NET Core Identity primitives.
- PostgreSQL/EF Core is the only persistence path for now.
- Production must use a real `IAuthNetEmailSender`; development sender is rejected in production.
- Public registration remains disabled by default.
- Keep `docs/architecture-context.md` compact and synchronized when architecture changes.

## Documentation Map

For library consumers:

- `docs/users/getting-started.md`
- `docs/users/configuration.md`
- `docs/users/account-pages.md`

For contributors:

- `docs/developer/onboarding.md`
- `docs/developer/quick-start.md`

For product/architecture:

- `docs/architecture-context.md`
- `docs/product-decisions.md`
- `docs/tasks.md`
- `docs/prd.md`

## Likely Next Work

Good next iteration candidates:

- Add integration tests for account routes and auth redirects.
- Improve UI layout/branding hooks beyond current minimal support.
- Add a real email sender sample implementation.
- Add packaging metadata and NuGet packing workflow.
- Add CI workflow for restore/build/test.
- Add documentation for consuming AuthNet from NuGet once package IDs are finalized.

Before starting any next feature, check whether it belongs to MVP slice 1 or deferred scope.

