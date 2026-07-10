# AuthNet Package Consumer Sample

This sample verifies AuthNet can be consumed from locally packed packages instead of project references.

Run the repository verification from the repository root:

```powershell
.\scripts\verify.ps1
```

This creates local package artifacts, validates the expected package output, then restores and builds this sample.

To rerun only the package-consumer check after packages already exist:

```powershell
.\scripts\verify-package-consumer.ps1
```

The sample references `AuthNet.AspNetCore` `0.1.0`; the primary package brings the current AuthNet package dependencies transitively.

The sample configures AuthNet persistence through the unified database builder:

```csharp
builder.Services.AddAuthNet(
    options => options.UseDevelopmentEmailSender = true,
    db => db.UsePostgres(connectionString));
```

The focused verification is build-only. Runtime startup still requires a PostgreSQL database and production applications should register a real `IAuthNetEmailSender` instead of using the development sender.

Troubleshooting:

- If the focused script reports missing `AuthNet.*.0.1.0.nupkg` files, run `.\scripts\verify.ps1` to rebuild the local package output.
- If restore fails after package changes, delete `samples\AuthNet.PackageConsumer\obj` and rerun `.\scripts\verify-package-consumer.ps1`.
