using AuthNet.AspNetCore;
using AuthNet.Core.Email;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

    public static void AddAuthNet(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ConfigureEmailSender(services, configuration);

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

    public static void ConfigureEmailSender(IServiceCollection services, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("AuthNet:UseDevelopmentEmailSender"))
        {
            return;
        }

        var smtpOptions = SampleHostSmtpEmailOptionsValidator.GetAndValidate(configuration);
        services.TryAddSingleton(smtpOptions);
        services.TryAddSingleton<IAuthNetEmailSender, SampleHostSmtpEmailSender>();
    }
}
