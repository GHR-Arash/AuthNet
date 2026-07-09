using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.AspNetCore;

internal sealed class AuthNetStartupRunner(IServiceProvider services)
{
    public async Task RunAsync(
        AuthNetStartupOptions options,
        CancellationToken cancellationToken = default)
    {
        if (options.ApplyMigrations)
        {
            var databaseInitializer = services.GetRequiredService<AuthNetDatabaseInitializer>();
            await databaseInitializer.ApplyMigrationsAsync(cancellationToken);
        }

        if (options.InitialAdministrator is not null)
        {
            var seeder = services.GetRequiredService<AuthNetInitialAdministratorSeeder>();
            await seeder.SeedAsync(options.InitialAdministrator, cancellationToken);
        }
    }
}
