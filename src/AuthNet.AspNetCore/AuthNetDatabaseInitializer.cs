using AuthNet.Persistence.Postgres;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AuthNet.AspNetCore;

internal sealed class AuthNetDatabaseInitializer(
    IServiceProvider services,
    ILogger<AuthNetDatabaseInitializer> logger)
{
    public async Task ApplyMigrationsAsync(CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthNetDbContext>();

        if (!db.Database.IsRelational())
        {
            logger.LogInformation("Skipping AuthNet migrations because the configured database provider is not relational.");
            return;
        }

        await db.Database.MigrateAsync(cancellationToken);
    }
}
