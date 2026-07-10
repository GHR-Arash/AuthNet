using AuthNet.AspNetCore;
using AuthNet.Core;
using AuthNet.Persistence.EntityFrameworkCore;
using AuthNet.Persistence.Postgres;
using AuthNet.Persistence.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.Tests;

public sealed class AuthNetDatabaseBuilderTests
{
    [Fact]
    public void UsePostgres_registers_npgsql_provider()
    {
        var services = new ServiceCollection();

        services.AddAuthNet(
            options => options.UseDevelopmentEmailSender = true,
            db => db.UsePostgres("Host=localhost;Database=authnet;Username=postgres;Password=postgres"));

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<DbContextOptions<AuthNetDbContext>>();

        Assert.Contains(options.Extensions, extension =>
            extension.GetType().Name.Contains("Npgsql", StringComparison.Ordinal));
    }

    [Fact]
    public void UsePostgres_sets_postgres_migrations_assembly()
    {
        var services = new ServiceCollection();

        services.AddAuthNet(
            options => options.UseDevelopmentEmailSender = true,
            db => db.UsePostgres("Host=localhost;Database=authnet;Username=postgres;Password=postgres"));

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<DbContextOptions<AuthNetDbContext>>();
        var relationalExtension = options.Extensions.OfType<RelationalOptionsExtension>().Single();

        Assert.Equal(typeof(AuthNetPostgresMigrationsAssembly).Assembly.GetName().Name, relationalExtension.MigrationsAssembly);
    }

    [Fact]
    public void UseInMemory_registers_inmemory_provider()
    {
        var services = new ServiceCollection();

        services.AddAuthNet(
            options => options.UseDevelopmentEmailSender = true,
            db => db.UseInMemory("AuthNet.Tests"));

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<DbContextOptions<AuthNetDbContext>>();

        Assert.Contains(options.Extensions, extension =>
            extension.GetType().Name.Contains("InMemory", StringComparison.Ordinal));
    }

    [Fact]
    public void UseSqlServer_registers_sql_server_provider()
    {
        var services = new ServiceCollection();

        services.AddAuthNet(
            options => options.UseDevelopmentEmailSender = true,
            db => db.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=AuthNet;Trusted_Connection=True;"));

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<DbContextOptions<AuthNetDbContext>>();

        Assert.Contains(options.Extensions, extension =>
            extension.GetType().Name.Contains("SqlServer", StringComparison.Ordinal));
    }

    [Fact]
    public void UseSqlServer_sets_sql_server_migrations_assembly()
    {
        var services = new ServiceCollection();

        services.AddAuthNet(
            options => options.UseDevelopmentEmailSender = true,
            db => db.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=AuthNet;Trusted_Connection=True;"));

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<DbContextOptions<AuthNetDbContext>>();
        var relationalExtension = options.Extensions.OfType<RelationalOptionsExtension>().Single();

        Assert.Equal(typeof(AuthNetSqlServerMigrationsAssembly).Assembly.GetName().Name, relationalExtension.MigrationsAssembly);
    }

    [Fact]
    public void Missing_database_configuration_throws_actionable_error()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<AuthNetConfigurationException>(() =>
            services.AddAuthNet(options => options.UseDevelopmentEmailSender = true));

        Assert.Contains("db.UsePostgres", exception.Message);
    }

    [Fact]
    public void Legacy_postgres_connection_string_still_registers_npgsql_provider()
    {
        var services = new ServiceCollection();

#pragma warning disable CS0618
        services.AddAuthNet(options =>
        {
            options.UseDevelopmentEmailSender = true;
            options.PostgresConnectionString = "Host=localhost;Database=authnet;Username=postgres;Password=postgres";
        });
#pragma warning restore CS0618

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<DbContextOptions<AuthNetDbContext>>();

        Assert.Contains(options.Extensions, extension =>
            extension.GetType().Name.Contains("Npgsql", StringComparison.Ordinal));
    }

    [Fact]
    public void Legacy_postgres_connection_string_sets_postgres_migrations_assembly()
    {
        var services = new ServiceCollection();

#pragma warning disable CS0618
        services.AddAuthNet(options =>
        {
            options.UseDevelopmentEmailSender = true;
            options.PostgresConnectionString = "Host=localhost;Database=authnet;Username=postgres;Password=postgres";
        });
#pragma warning restore CS0618

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<DbContextOptions<AuthNetDbContext>>();
        var relationalExtension = options.Extensions.OfType<RelationalOptionsExtension>().Single();

        Assert.Equal(typeof(AuthNetPostgresMigrationsAssembly).Assembly.GetName().Name, relationalExtension.MigrationsAssembly);
    }

    [Fact]
    public void Duplicate_database_provider_configuration_fails_fast()
    {
        var builder = new AuthNetDatabaseBuilder();

        builder.UseInMemory("AuthNet.Tests");

        var exception = Assert.Throws<AuthNetConfigurationException>(() =>
            builder.UsePostgres("Host=localhost;Database=authnet;Username=postgres;Password=postgres"));

        Assert.Contains("Configure only one database provider", exception.Message);
    }

    [Fact]
    public void ConfigureDbContext_registers_custom_provider()
    {
        var services = new ServiceCollection();

        services.AddAuthNet(
            options => options.UseDevelopmentEmailSender = true,
            db => db.ConfigureDbContext(options => options.UseInMemoryDatabase("AuthNet.Custom")));

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<DbContextOptions<AuthNetDbContext>>();

        Assert.Contains(options.Extensions, extension =>
            extension.GetType().Name.Contains("InMemory", StringComparison.Ordinal));
    }

    [Fact]
    public void ConfigureDbContext_participates_in_duplicate_provider_rejection()
    {
        var builder = new AuthNetDatabaseBuilder();

        builder.ConfigureDbContext(options => options.UseInMemoryDatabase("AuthNet.Custom"));

        var exception = Assert.Throws<AuthNetConfigurationException>(() =>
            builder.UseInMemory("AuthNet.Tests"));

        Assert.Contains("Configure only one database provider", exception.Message);
    }

    [Fact]
    public void Explicit_database_builder_configuration_wins_over_legacy_postgres_connection_string()
    {
        var services = new ServiceCollection();

#pragma warning disable CS0618
        services.AddAuthNet(
            options =>
            {
                options.UseDevelopmentEmailSender = true;
                options.PostgresConnectionString = "Host=localhost;Database=authnet;Username=postgres;Password=postgres";
            },
            db => db.UseInMemory("AuthNet.Tests"));
#pragma warning restore CS0618

        using var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<DbContextOptions<AuthNetDbContext>>();

        Assert.Contains(options.Extensions, extension =>
            extension.GetType().Name.Contains("InMemory", StringComparison.Ordinal));
        Assert.DoesNotContain(options.Extensions, extension =>
            extension.GetType().Name.Contains("Npgsql", StringComparison.Ordinal));
    }

    [Fact]
    public void Empty_postgres_connection_string_fails_fast()
    {
        var builder = new AuthNetDatabaseBuilder();

        var exception = Assert.Throws<AuthNetConfigurationException>(() =>
            builder.UsePostgres(""));

        Assert.Contains("db.UsePostgres", exception.Message);
    }

    [Fact]
    public void Empty_sql_server_connection_string_fails_fast()
    {
        var builder = new AuthNetDatabaseBuilder();

        var exception = Assert.Throws<AuthNetConfigurationException>(() =>
            builder.UseSqlServer(""));

        Assert.Contains("db.UseSqlServer", exception.Message);
    }

    [Fact]
    public void UseSqlServer_participates_in_duplicate_provider_rejection()
    {
        var builder = new AuthNetDatabaseBuilder();

        builder.UsePostgres("Host=localhost;Database=authnet;Username=postgres;Password=postgres");

        var exception = Assert.Throws<AuthNetConfigurationException>(() =>
            builder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=AuthNet;Trusted_Connection=True;"));

        Assert.Contains("Configure only one database provider", exception.Message);
    }
}
