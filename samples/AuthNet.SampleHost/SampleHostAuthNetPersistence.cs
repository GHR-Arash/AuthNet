using AuthNet.AspNetCore;
using Microsoft.EntityFrameworkCore;

namespace AuthNet.SampleHost;

public static class SampleHostAuthNetPersistence
{
    public const string InMemoryDatabaseName = "AuthNetSampleHost";

    public static bool ShouldUseInMemoryDatabase(IHostEnvironment environment, IConfiguration configuration)
    {
        var useInMemoryDatabase = configuration.GetValue<bool>("AuthNet:UseInMemoryDatabase");
        if (useInMemoryDatabase && !environment.IsDevelopment())
        {
            throw new InvalidOperationException("AuthNet sample InMemory database mode is only allowed in Development.");
        }

        return useInMemoryDatabase;
    }

    public static bool ShouldApplyMigrations(IConfiguration configuration, bool useInMemoryDatabase)
    {
        return configuration.GetValue<bool>("AuthNet:ApplyMigrations") && !useInMemoryDatabase;
    }

    public static void AddAuthNet(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var useInMemoryDatabase = ShouldUseInMemoryDatabase(environment, configuration);
        if (useInMemoryDatabase)
        {
            services.AddAuthNet(
                options => configuration.GetSection("AuthNet").Bind(options),
                db => db.UseInMemoryDatabase(InMemoryDatabaseName));
            return;
        }

        services.AddAuthNet(options =>
        {
            configuration.GetSection("AuthNet").Bind(options);
            options.PostgresConnectionString = configuration.GetConnectionString("AuthNet");
        });
    }
}
