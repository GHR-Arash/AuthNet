using System.Net;
using AuthNet.AspNetCore;
using AuthNet.Core;
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.Tests.Integration;

public sealed class AuthNetStartupTests
{
    [Fact]
    public async Task Fluent_use_authnet_maps_authnet_routes()
    {
        await using var host = await AuthNetTestHost.CreateAsync(
            configureStartup: authNet => authNet.ApplyMigrations());

        var home = await host.Client.GetAsync("/auth");
        var openApi = await host.Client.GetAsync("/auth/api/openapi.json");

        Assert.Equal(HttpStatusCode.OK, home.StatusCode);
        Assert.Equal(HttpStatusCode.OK, openApi.StatusCode);
    }

    [Fact]
    public async Task Initial_administrator_creates_admin_user_and_role()
    {
        await using var host = await AuthNetTestHost.CreateAsync(
            configureStartup: authNet => authNet.InitialAdministrator(
                username: "admin",
                password: "Password1!",
                email: "admin@example.test"));

        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var user = await userManager.FindByEmailAsync("admin@example.test");

        Assert.NotNull(user);
        Assert.Equal("admin", user.UserName);
        Assert.True(user.EmailConfirmed);
        Assert.True(await roleManager.RoleExistsAsync("Administrator"));
        Assert.True(await userManager.IsInRoleAsync(user, "Administrator"));

        var form = await host.GetFormAsync("/auth/login");
        var response = await host.PostFormAsync("/auth/login", form,
            ("Input.Email", "admin"),
            ("Input.Password", "Password1!"),
            ("Input.RememberMe", "false"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    [Fact]
    public async Task Initial_administrator_from_configuration_creates_admin_user()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["UserName"] = "configured-admin",
                ["Email"] = "configured-admin@example.test",
                ["Password"] = "Password1!"
            })
            .Build();

        await using var host = await AuthNetTestHost.CreateAsync(
            configureStartup: authNet => authNet.InitialAdministrator(configuration));

        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByNameAsync("configured-admin");

        Assert.NotNull(user);
        Assert.Equal("configured-admin@example.test", user.Email);
        Assert.True(await userManager.IsInRoleAsync(user, "Administrator"));
    }

    [Fact]
    public async Task Initial_administrator_promotes_existing_user_without_password()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync(
            "existing-admin@example.test",
            password: "Original1!",
            emailConfirmed: true,
            displayName: "Existing Admin");

        var startupBuilder = new AuthNetStartupBuilder()
            .InitialAdministrator(
                username: "ignored",
                password: "",
                email: "existing-admin@example.test");

        await host.Services.GetRequiredService<AuthNetStartupRunner>().RunAsync(startupBuilder.Options);

        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByEmailAsync("existing-admin@example.test");

        Assert.NotNull(user);
        Assert.Equal("Existing Admin", user.DisplayName);
        Assert.True(await userManager.CheckPasswordAsync(user, "Original1!"));
        Assert.True(await userManager.IsInRoleAsync(user, "Administrator"));
    }

    [Fact]
    public async Task Initial_administrator_is_idempotent()
    {
        await using var host = await AuthNetTestHost.CreateAsync(
            configureStartup: authNet => authNet.InitialAdministrator(
                username: "admin",
                password: "Password1!",
                email: "admin@example.test"));

        var startupBuilder = new AuthNetStartupBuilder()
            .InitialAdministrator(
                username: "admin",
                password: "Different1!",
                email: "admin@example.test");

        await host.Services.GetRequiredService<AuthNetStartupRunner>().RunAsync(startupBuilder.Options);

        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var users = userManager.Users.Where(user => user.Email == "admin@example.test").ToList();

        Assert.Single(users);
        Assert.True(await userManager.CheckPasswordAsync(users[0], "Password1!"));
        Assert.False(await userManager.CheckPasswordAsync(users[0], "Different1!"));
        Assert.True(await userManager.IsInRoleAsync(users[0], "Administrator"));
    }

    [Fact]
    public async Task Initial_administrator_requires_password_for_missing_user()
    {
        var exception = await Assert.ThrowsAsync<AuthNetConfigurationException>(() =>
            AuthNetTestHost.CreateAsync(
                configureStartup: authNet => authNet.InitialAdministrator(
                    username: "admin",
                    password: "",
                    email: "missing-password@example.test")));

        Assert.Contains("password is required", exception.Message);
        Assert.DoesNotContain("missing-password@example.test", exception.Message);
    }

    [Fact]
    public async Task Initial_administrator_reports_invalid_password_without_echoing_password()
    {
        var exception = await Assert.ThrowsAsync<AuthNetConfigurationException>(() =>
            AuthNetTestHost.CreateAsync(
                configureStartup: authNet => authNet.InitialAdministrator(
                    username: "admin",
                    password: "short",
                    email: "invalid-password@example.test")));

        Assert.Contains("Failed to create the AuthNet initial administrator user", exception.Message);
        Assert.DoesNotContain("short", exception.Message);
    }

    [Fact]
    public async Task Apply_migrations_is_noop_for_inmemory_provider()
    {
        await using var host = await AuthNetTestHost.CreateAsync(
            configureStartup: authNet => authNet.ApplyMigrations());

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthNetDbContext>();

        Assert.False(db.Database.IsRelational());
    }

    [Fact]
    public async Task Apply_migrations_attempts_relational_provider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<AuthNetDbContext>(db => db.UseNpgsql(
            "Host=127.0.0.1;Port=1;Database=authnet_unreachable;Username=postgres;Password=postgres;Timeout=1;Command Timeout=1"));
        services.AddSingleton<AuthNetDatabaseInitializer>();

        using var provider = services.BuildServiceProvider();
        var initializer = provider.GetRequiredService<AuthNetDatabaseInitializer>();

        await Assert.ThrowsAnyAsync<Exception>(() => initializer.ApplyMigrationsAsync());
    }
}
