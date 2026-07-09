using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Identity;

namespace AuthNet.SampleHost;

public static class SampleHostAdminBootstrap
{
    public const string RoleName = "Administrator";
    public const string DemoAdminUserName = "admin";
    public const string DemoAdminEmail = "admin@admin.com";
    public const string DemoAdminPassword = "Password1!";

    public static bool IsEnabled(IConfiguration configuration)
    {
        return configuration.GetValue<bool>("AuthNet:AdminBootstrap:Enabled");
    }

    public static async Task BootstrapAsync(
        IServiceProvider services,
        IConfiguration configuration)
    {
        if (!IsEnabled(configuration))
        {
            return;
        }

        var email = configuration["AuthNet:AdminBootstrap:Email"];
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("AuthNet:AdminBootstrap:Email is required when admin bootstrap is enabled.");
        }

        await EnsureAdminUserAsync(
            services,
            email,
            configuration["AuthNet:AdminBootstrap:UserName"],
            configuration["AuthNet:AdminBootstrap:Password"]);
    }

    public static Task BootstrapDemoAdminAsync(IServiceProvider services)
    {
        return EnsureAdminUserAsync(
            services,
            DemoAdminEmail,
            DemoAdminUserName,
            DemoAdminPassword);
    }

    private static async Task EnsureAdminUserAsync(
        IServiceProvider services,
        string email,
        string? userName,
        string? password)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();

        if (!await roleManager.RoleExistsAsync(RoleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(RoleName));
            ThrowIfFailed(roleResult, "create admin role");
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("AuthNet:AdminBootstrap:Password is required to create an admin user.");
            }

            user = new AuthNetUser
            {
                UserName = string.IsNullOrWhiteSpace(userName) ? email : userName,
                Email = email,
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            ThrowIfFailed(createResult, "create admin user");
        }

        if (!await userManager.IsInRoleAsync(user, RoleName))
        {
            var roleResult = await userManager.AddToRoleAsync(user, RoleName);
            ThrowIfFailed(roleResult, "assign admin role");
        }
    }

    private static void ThrowIfFailed(IdentityResult result, string action)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to {action}: {string.Join(" ", result.Errors.Select(error => error.Description))}");
        }
    }
}
