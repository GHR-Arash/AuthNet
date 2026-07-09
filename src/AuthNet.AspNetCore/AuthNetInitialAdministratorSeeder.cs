using AuthNet.Core;
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.AspNetCore;

internal sealed class AuthNetInitialAdministratorSeeder(IServiceProvider services)
{
    private const string AdministratorRoleName = "Administrator";

    public async Task SeedAsync(
        InitialAdministratorOptions options,
        CancellationToken cancellationToken = default)
    {
        if (!options.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(options.Email))
        {
            throw new AuthNetConfigurationException("AuthNet initial administrator email is required.");
        }

        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();

        cancellationToken.ThrowIfCancellationRequested();
        if (!await roleManager.RoleExistsAsync(AdministratorRoleName))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole(AdministratorRoleName));
            ThrowIfFailed(roleResult, "create the AuthNet administrator role");
        }

        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByEmailAsync(options.Email);
        if (user is null)
        {
            if (string.IsNullOrWhiteSpace(options.Password))
            {
                throw new AuthNetConfigurationException(
                    "AuthNet initial administrator password is required when the user does not already exist.");
            }

            user = new AuthNetUser
            {
                UserName = string.IsNullOrWhiteSpace(options.UserName) ? options.Email : options.UserName,
                Email = options.Email,
                EmailConfirmed = true,
                LockoutEnabled = true
            };

            var createResult = await userManager.CreateAsync(user, options.Password);
            ThrowIfFailed(createResult, "create the AuthNet initial administrator user");
        }

        cancellationToken.ThrowIfCancellationRequested();
        if (!await userManager.IsInRoleAsync(user, AdministratorRoleName))
        {
            var roleResult = await userManager.AddToRoleAsync(user, AdministratorRoleName);
            ThrowIfFailed(roleResult, "assign AuthNet administrator access");
        }
    }

    private static void ThrowIfFailed(IdentityResult result, string action)
    {
        if (result.Succeeded)
        {
            return;
        }

        throw new AuthNetConfigurationException(
            $"Failed to {action}: {string.Join(" ", result.Errors.Select(error => error.Description))}");
    }
}
