using AuthNet.Persistence.Postgres;
using AuthNet.SampleHost;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace AuthNet.Tests;

public sealed class SampleHostAdminBootstrapTests
{
    [Fact]
    public async Task Disabled_bootstrap_does_not_create_admin_user()
    {
        var services = Services();
        var configuration = Configuration();

        await SampleHostAdminBootstrap.BootstrapAsync(services, configuration);

        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        Assert.Null(await userManager.FindByEmailAsync("admin@example.test"));
    }

    [Fact]
    public async Task Enabled_bootstrap_creates_admin_user_and_role()
    {
        var services = Services();
        var configuration = Configuration(
            ("AuthNet:AdminBootstrap:Enabled", "true"),
            ("AuthNet:AdminBootstrap:Email", "admin@example.test"),
            ("AuthNet:AdminBootstrap:Password", "Password1!"));

        await SampleHostAdminBootstrap.BootstrapAsync(services, configuration);

        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var user = await userManager.FindByEmailAsync("admin@example.test");

        Assert.NotNull(user);
        Assert.Equal("admin@example.test", user.UserName);
        Assert.True(user.EmailConfirmed);
        Assert.True(await roleManager.RoleExistsAsync(SampleHostAdminBootstrap.RoleName));
        Assert.True(await userManager.IsInRoleAsync(user, SampleHostAdminBootstrap.RoleName));
    }

    [Fact]
    public async Task Enabled_bootstrap_can_create_admin_with_separate_user_name()
    {
        var services = Services();
        var configuration = Configuration(
            ("AuthNet:AdminBootstrap:Enabled", "true"),
            ("AuthNet:AdminBootstrap:UserName", "admin"),
            ("AuthNet:AdminBootstrap:Email", "admin@example.test"),
            ("AuthNet:AdminBootstrap:Password", "Password1!"));

        await SampleHostAdminBootstrap.BootstrapAsync(services, configuration);

        using var scope = services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByNameAsync("admin");

        Assert.NotNull(user);
        Assert.Equal("admin@example.test", user.Email);
        Assert.True(await userManager.IsInRoleAsync(user, SampleHostAdminBootstrap.RoleName));
    }

    [Fact]
    public async Task Existing_user_can_be_assigned_admin_role_without_password()
    {
        var services = Services();
        var configuration = Configuration(
            ("AuthNet:AdminBootstrap:Enabled", "true"),
            ("AuthNet:AdminBootstrap:Email", "existing@example.test"));

        using (var scope = services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
            var result = await userManager.CreateAsync(new AuthNetUser
            {
                UserName = "existing",
                Email = "existing@example.test",
                EmailConfirmed = true,
                LockoutEnabled = true
            }, "Password1!");
            Assert.True(result.Succeeded);
        }

        await SampleHostAdminBootstrap.BootstrapAsync(services, configuration);

        using var assertScope = services.CreateScope();
        var assertUserManager = assertScope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await assertUserManager.FindByEmailAsync("existing@example.test");

        Assert.NotNull(user);
        Assert.True(await assertUserManager.IsInRoleAsync(user, SampleHostAdminBootstrap.RoleName));
    }

    private static IServiceProvider Services()
    {
        var services = new ServiceCollection();
        var configuration = Configuration(
            ("AuthNet:UseInMemoryDatabase", "true"),
            ("AuthNet:UseDevelopmentEmailSender", "true"));
        var environment = new TestHostEnvironment(Environments.Development);

        SampleHostAuthNetPersistence.AddAuthNet(services, configuration, environment);

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuthNetDbContext>();
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        return provider;
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
