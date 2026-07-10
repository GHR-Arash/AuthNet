using AuthNet.Core;
using Microsoft.EntityFrameworkCore;

namespace AuthNet.AspNetCore;

public sealed class AuthNetDatabaseBuilder
{
    private string? providerName;

    internal Action<DbContextOptionsBuilder>? ConfigureDbContextAction { get; private set; }

    public AuthNetDatabaseBuilder UsePostgres(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new AuthNetConfigurationException("AuthNet PostgreSQL configuration requires a non-empty connection string. Pass one to db.UsePostgres(connectionString).");
        }

        SetProvider(nameof(UsePostgres), db => db.UseNpgsql(connectionString));

        return this;
    }

    public AuthNetDatabaseBuilder UseInMemory(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new AuthNetConfigurationException("AuthNet InMemory configuration requires a non-empty database name. Pass one to db.UseInMemory(databaseName).");
        }

        SetProvider(nameof(UseInMemory), db => db.UseInMemoryDatabase(databaseName));

        return this;
    }

    public AuthNetDatabaseBuilder ConfigureDbContext(Action<DbContextOptionsBuilder> configureDbContext)
    {
        ArgumentNullException.ThrowIfNull(configureDbContext);

        SetProvider(nameof(ConfigureDbContext), configureDbContext);

        return this;
    }

    private void SetProvider(string name, Action<DbContextOptionsBuilder> configureDbContext)
    {
        if (ConfigureDbContextAction is not null)
        {
            throw new AuthNetConfigurationException($"AuthNet database provider is already configured with {providerName}. Configure only one database provider.");
        }

        providerName = name;
        ConfigureDbContextAction = configureDbContext;
    }
}
