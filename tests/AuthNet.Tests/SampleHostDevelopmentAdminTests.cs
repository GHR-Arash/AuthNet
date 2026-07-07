using AuthNet.Persistence.Postgres;
using AuthNet.SampleHost;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace AuthNet.Tests;

public sealed class SampleHostDevelopmentAdminTests
{
    [Fact]
    public async Task Disabled_bootstrap_does_not_create_admin_user()
    {
        var services = Services();
        var configuration = Configuration();
        var environment = new TestHostEnvironment(Environments.Development);

        await SampleHostDevelopmentAdmin.BootstrapAsync(services, configuration, environment);

        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        Assert.Null(await userManager.FindByEmailAsync("admin@example.test"));
    }

    [Fact]
    public async Task Non_development_rejects_enabled_bootstrap()
    {
        var services = Services();
        var configuration = Configuration(
            ("AuthNet:DevelopmentAdmin:Enabled", "true"),
            ("AuthNet:DevelopmentAdmin:Email", "admin@example.test"),
            ("AuthNet:DevelopmentAdmin:Password", "Password1!"));
        var environment = new TestHostEnvironment(Environments.Production);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            SampleHostDevelopmentAdmin.BootstrapAsync(services, configuration, environment));

        Assert.Contains("only allowed in Development", exception.Message);
    }

    [Fact]
    public async Task Enabled_bootstrap_creates_admin_user_and_role()
    {
        var services = Services();
        var configuration = Configuration(
            ("AuthNet:DevelopmentAdmin:Enabled", "true"),
            ("AuthNet:DevelopmentAdmin:Email", "admin@example.test"),
            ("AuthNet:DevelopmentAdmin:Password", "Password1!"));
        var environment = new TestHostEnvironment(Environments.Development);

        await SampleHostDevelopmentAdmin.BootstrapAsync(services, configuration, environment);

        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var user = await userManager.FindByEmailAsync("admin@example.test");

        Assert.NotNull(user);
        Assert.True(user.EmailConfirmed);
        Assert.True(await roleManager.RoleExistsAsync(SampleHostDevelopmentAdmin.RoleName));
        Assert.True(await userManager.IsInRoleAsync(user, SampleHostDevelopmentAdmin.RoleName));
    }

    [Fact]
    public async Task Existing_user_can_be_assigned_admin_role_without_password()
    {
        var services = Services();
        var configuration = Configuration(
            ("AuthNet:DevelopmentAdmin:Enabled", "true"),
            ("AuthNet:DevelopmentAdmin:Email", "existing@example.test"));
        var environment = new TestHostEnvironment(Environments.Development);

        using (var scope = services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
            var result = await userManager.CreateAsync(new AuthNetUser
            {
                UserName = "existing@example.test",
                Email = "existing@example.test",
                EmailConfirmed = true,
                LockoutEnabled = true
            }, "Password1!");
            Assert.True(result.Succeeded);
        }

        await SampleHostDevelopmentAdmin.BootstrapAsync(services, configuration, environment);

        using var assertScope = services.CreateScope();
        var assertUserManager = assertScope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await assertUserManager.FindByEmailAsync("existing@example.test");

        Assert.NotNull(user);
        Assert.True(await assertUserManager.IsInRoleAsync(user, SampleHostDevelopmentAdmin.RoleName));
    }

    private static IServiceProvider Services()
    {
        var services = new ServiceCollection();
        var configuration = Configuration(
            ("AuthNet:UseInMemoryDatabase", "true"),
            ("AuthNet:UseDevelopmentEmailSender", "true"));
        var environment = new TestHostEnvironment(Environments.Development);

        SampleHostAuthNetPersistence.AddAuthNet(services, configuration, environment);

        return services.BuildServiceProvider();
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
