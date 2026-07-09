using AuthNet.Core;
using AuthNet.Persistence.Postgres;
using AuthNet.SampleHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace AuthNet.Tests;

public sealed class SampleHostAuthNetPersistenceTests
{
    [Fact]
    public void Development_can_enable_inmemory_database()
    {
        var configuration = Configuration(
            ("AuthNet:UseInMemoryDatabase", "true"));
        var environment = new TestHostEnvironment(Environments.Development);

        var useInMemory = SampleHostAuthNetPersistence.ShouldUseInMemoryDatabase(environment, configuration);

        Assert.True(useInMemory);
    }

    [Fact]
    public void Non_development_rejects_inmemory_database()
    {
        var configuration = Configuration(
            ("AuthNet:UseInMemoryDatabase", "true"));
        var environment = new TestHostEnvironment(Environments.Production);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            SampleHostAuthNetPersistence.ShouldUseInMemoryDatabase(environment, configuration));

        Assert.Contains("only allowed in Development", exception.Message);
    }

    [Fact]
    public void Sample_host_inmemory_registration_uses_inmemory_provider()
    {
        var services = new ServiceCollection();
        var configuration = Configuration(
            ("AuthNet:UseInMemoryDatabase", "true"),
            ("AuthNet:UseDevelopmentEmailSender", "true"));
        var environment = new TestHostEnvironment(Environments.Development);

        SampleHostAuthNetPersistence.AddAuthNet(services, configuration, environment);

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<DbContextOptions<AuthNetDbContext>>();
        var extension = Assert.Single(
            options.Extensions,
            candidate => candidate.GetType().Name.Contains("InMemory", StringComparison.Ordinal));
        Assert.Contains("InMemory", extension.GetType().Name);
    }

    [Fact]
    public void Sample_host_postgres_registration_requires_connection_string()
    {
        var services = new ServiceCollection();
        var configuration = Configuration(
            ("AuthNet:UseDevelopmentEmailSender", "true"));
        var environment = new TestHostEnvironment(Environments.Development);

        var exception = Assert.Throws<AuthNetConfigurationException>(() =>
            SampleHostAuthNetPersistence.AddAuthNet(services, configuration, environment));

        Assert.Contains("PostgresConnectionString", exception.Message);
    }

    private static IConfiguration Configuration(params (string Key, string Value)[] values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(value =>
                new KeyValuePair<string, string?>(value.Key, value.Value)))
            .Build();
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "AuthNet.Tests";

        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
