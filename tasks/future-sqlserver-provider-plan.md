# Future Plan: SQL Server Provider

## Status

Promoted to Slice 26.

Current implementation artifacts:

- `tasks/slice-26-plan.md`
- `tasks/slice-26-todo.md`
- `src/AuthNet.Persistence.SqlServer`

SQL Server is now a first-party provider configured through:

```csharp
builder.Services.AddAuthNet(
    options => builder.Configuration.GetSection("AuthNet").Bind(options),
    db => db.UseSqlServer(builder.Configuration.GetConnectionString("AuthNet")));
```

## Remaining Follow-Up

Slice 27 removes the legacy `AuthNetOptions.PostgresConnectionString` compatibility path so database configuration goes only through the database builder API.
