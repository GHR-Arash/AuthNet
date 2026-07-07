# Slice 04: Development InMemory Persistence

## Intent

Make the sample host runnable in Development without requiring a local PostgreSQL server.

## Implemented Behavior

- `samples/AuthNet.SampleHost` can use EF Core InMemory when:
  - `ASPNETCORE_ENVIRONMENT=Development`
  - `AuthNet:UseInMemoryDatabase=true`
- PostgreSQL remains the default in `appsettings.json`.
- Production rejects InMemory mode.
- `AuthNet:ApplyMigrations=true` is skipped when InMemory is active because EF Core InMemory does not use relational migrations.

## Configuration

In `samples/AuthNet.SampleHost/appsettings.Development.json`:

```json
{
  "AuthNet": {
    "UseInMemoryDatabase": true
  }
}
```

## Startup Shape

The sample host centralizes this decision in `SampleHostAuthNetPersistence`.

Conceptually:

```csharp
var useInMemory = builder.Environment.IsDevelopment()
    && builder.Configuration.GetValue<bool>("AuthNet:UseInMemoryDatabase");

if (useInMemory)
{
    builder.Services.AddAuthNet(
        options => builder.Configuration.GetSection("AuthNet").Bind(options),
        db => db.UseInMemoryDatabase("AuthNetSampleHost"));
}
else
{
    builder.Services.AddAuthNet(options =>
    {
        builder.Configuration.GetSection("AuthNet").Bind(options);
        options.PostgresConnectionString = builder.Configuration.GetConnectionString("AuthNet");
    });
}
```

## Non-Goals

- InMemory is not a supported production persistence provider.
- InMemory does not replace PostgreSQL testing for migration, relational constraint, or deployment behavior.
- No new `AuthNet.Persistence.InMemory` package should be added for this slice.

## Verification

```powershell
.\.dotnet\dotnet.exe restore AuthNet.slnx
.\.dotnet\dotnet.exe build AuthNet.slnx --no-restore
.\.dotnet\dotnet.exe test AuthNet.slnx --no-build
```

Focused tests:

```powershell
.\.dotnet\dotnet.exe test tests\AuthNet.Tests\AuthNet.Tests.csproj --no-build --filter SampleHostAuthNetPersistenceTests
```

Manual sample startup:

```powershell
$env:ASPNETCORE_ENVIRONMENT='Development'
.\.dotnet\dotnet.exe run --project samples\AuthNet.SampleHost\AuthNet.SampleHost.csproj --no-build --no-launch-profile --urls http://127.0.0.1:5127
```

Expected outcome: sample host starts and account pages are usable without a PostgreSQL server.

Latest manual smoke result: `/auth/login` returned HTTP 200 in Development InMemory mode.
