# Slice 03 Package Readiness Notes

This folder holds Slice 03 documentation separate from task artifacts in `tasks/`.

## Goal

Prepare AuthNet for local NuGet package creation and package-based consumption.

## Proposed Packages

Packable:

- `AuthNet.Core`
- `AuthNet.AspNetCore`
- `AuthNet.UI.Razor`
- `AuthNet.Persistence.Postgres`
- `AuthNet.ExternalProviders`
- `AuthNet.Api`

Not packable:

- `AuthNet.SampleHost`
- `AuthNet.PackageConsumer`
- `AuthNet.Tests`

## Package Metadata

Shared metadata is defined in `Directory.Build.props`:

- Version: `0.1.0`
- Authors: `AuthNet Contributors`
- Product/company: `AuthNet`
- Tags: `auth`, `identity`, `aspnetcore`, `razor-pages`, `postgresql`
- Package readme: root `README.md`
- Package output: `artifacts/packages`

Final public publication still needs a real repository/project URL and license decision.

## Public API Inventory

`AuthNet.Core`:

- `AuthNetOptions`
- `AuthNetPasswordOptions`
- `AuthNetLockoutOptions`
- `AuthNetCookieOptions`
- `AuthNetOpenIdConnectOptions`
- `AuthNetConfigurationException`
- `AuthNetEmailMessage`
- `IAuthNetEmailSender`

`AuthNet.AspNetCore`:

- `AddAuthNet(...)`
- `MapAuthNet()`
- `UseAuthNet()` compatibility wrapper
- `DevelopmentEmailStore`
- `DevelopmentAuthNetEmailSender`

`AuthNet.Persistence.Postgres`:

- `AuthNetDbContext`
- `AuthNetUser`

`AuthNet.ExternalProviders`:

- `AddAuthNetOpenIdConnect(...)`

`AuthNet.UI.Razor`:

- Razor Page models and input models are public as part of the Razor Pages UI assembly.

`AuthNet.Api`:

- Same-origin SPA JSON endpoint mapping and OpenAPI document support.

API tightening completed:

- `AuthNetConfigurationValidator` is now internal and exposed to `AuthNet.Tests` through `InternalsVisibleTo`.

## Package Dependency Review

- `AuthNet.AspNetCore` depends on `AuthNet.Core`, `AuthNet.ExternalProviders`, `AuthNet.Persistence.Postgres`, and `AuthNet.UI.Razor`.
- `AuthNet.Api` depends on `AuthNet.Core` and `AuthNet.Persistence.Postgres`.
- `AuthNet.ExternalProviders` depends on `AuthNet.Core` and `Microsoft.AspNetCore.Authentication.OpenIdConnect`.
- `AuthNet.Persistence.Postgres` depends on `AuthNet.Core`, `Microsoft.AspNetCore.Identity.EntityFrameworkCore`, and `Npgsql.EntityFrameworkCore.PostgreSQL`.
- `AuthNet.UI.Razor` depends on `AuthNet.Core` and `AuthNet.Persistence.Postgres`.
- EF design/build dependencies are marked private and do not appear in package dependency metadata.

## Package Asset Inspection

Generated packages:

- `AuthNet.AspNetCore.0.1.0.nupkg`
- `AuthNet.Api.0.1.0.nupkg`
- `AuthNet.Core.0.1.0.nupkg`
- `AuthNet.ExternalProviders.0.1.0.nupkg`
- `AuthNet.Persistence.Postgres.0.1.0.nupkg`
- `AuthNet.UI.Razor.0.1.0.nupkg`

Observed contents:

- Each package contains its `.nuspec`, root `README.md`, and `lib/net10.0/{PackageId}.dll`.
- `AuthNet.UI.Razor` packages the compiled Razor Class Library assembly; Razor pages are compiled into the assembly rather than shipped as loose `.cshtml` files.
- EF Core migrations are compiled into `AuthNet.Persistence.Postgres.dll`; no separate migration content files are required for the current EF Core model.

## Verification Target

Slice 03 should end with:

```powershell
.\.dotnet\dotnet.exe restore AuthNet.slnx
.\.dotnet\dotnet.exe build AuthNet.slnx --configuration Release --no-restore
.\.dotnet\dotnet.exe test AuthNet.slnx --no-build
.\.dotnet\dotnet.exe pack src\AuthNet.Core\AuthNet.Core.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.ExternalProviders\AuthNet.ExternalProviders.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.Persistence.Postgres\AuthNet.Persistence.Postgres.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.UI.Razor\AuthNet.UI.Razor.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.Api\AuthNet.Api.csproj --configuration Release --no-build --output .\artifacts\packages
.\.dotnet\dotnet.exe pack src\AuthNet.AspNetCore\AuthNet.AspNetCore.csproj --configuration Release --no-build --output .\artifacts\packages
```

Generated packages should stay under ignored local artifact output and should not be committed.

## Consumer Smoke Result

The committed sample at `samples/AuthNet.PackageConsumer` restores `AuthNet.AspNetCore` `0.1.0` from `artifacts/packages`, pulls the transitive AuthNet packages from the same source, and compiles a minimal `AddAuthNet` plus `MapAuthNet` setup.

Focused verification:

```powershell
.\scripts\verify-package-consumer.ps1
```
