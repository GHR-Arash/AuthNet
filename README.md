# AuthNet

AuthNet is a reusable .NET 10 ASP.NET Identity and Access Management component.

MVP slice 1 provides ASP.NET Core Identity integration, EF Core/PostgreSQL persistence, Razor Pages account UI, cookie authentication, role infrastructure, generic OpenID Connect login, and email-based account flows.

## Quick Start

This repo uses a project-local .NET 10 SDK in `.dotnet/`.

```powershell
.\.dotnet\dotnet.exe restore AuthNet.slnx
.\.dotnet\dotnet.exe build AuthNet.slnx
.\.dotnet\dotnet.exe test AuthNet.slnx
```

Run the sample host:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
.\.dotnet\dotnet.exe run --project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --urls http://127.0.0.1:5127
```

Apply the Identity schema to PostgreSQL by setting `AuthNet:ApplyMigrations` to `true` in the sample host configuration, or by using EF tooling:

```powershell
.\.dotnet\dotnet.exe tool install dotnet-ef --version 10.0.9 --tool-path .tools
.\.tools\dotnet-ef.exe database update --project src\AuthNet.Persistence.Postgres\AuthNet.Persistence.Postgres.csproj --startup-project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --context AuthNetDbContext
```

## Docs

- [Architecture Context](docs/architecture-context.md)
- [MVP Tasks](docs/tasks.md)
- [Product Decisions](docs/product-decisions.md)
