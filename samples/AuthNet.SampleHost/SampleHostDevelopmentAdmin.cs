using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Identity;

namespace AuthNet.SampleHost;

public static class SampleHostDevelopmentAdmin
{
    public const string RoleName = "Administrator";

    public static bool IsEnabled(IConfiguration configuration)
    {
        return configuration.GetValue<bool>("AuthNet:DevelopmentAdmin:Enabled");
    }

    public static async Task BootstrapAsync(
        IServiceProvider services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        if (!IsEnabled(configuration))
        {
            return;
        }

        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException("AuthNet sample development admin bootstrap is only allowed in Development.");
        }

        var email = configuration["AuthNet:DevelopmentAdmin:Email"];
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new InvalidOperationException("AuthNet:DevelopmentAdmin:Email is required when development admin bootstrap is enabled.");
        }

        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();

        if (!await roleManager.RoleExistsAsync(RoleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(RoleName));
            ThrowIfFailed(roleResult, "create development admin role");
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            var password = configuration["AuthNet:DevelopmentAdmin:Password"];
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("AuthNet:DevelopmentAdmin:Password is required to create a development admin user.");
            }

            user = new AuthNetUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var createResult = await userManager.CreateAsync(user, password);
            ThrowIfFailed(createResult, "create development admin user");
        }

        if (!await userManager.IsInRoleAsync(user, RoleName))
        {
            var roleResult = await userManager.AddToRoleAsync(user, RoleName);
            ThrowIfFailed(roleResult, "assign development admin role");
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
