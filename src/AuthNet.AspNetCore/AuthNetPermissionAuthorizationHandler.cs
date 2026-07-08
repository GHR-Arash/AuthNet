using AuthNet.Core;
using Microsoft.AspNetCore.Authorization;

namespace AuthNet.AspNetCore;

internal sealed class AuthNetPermissionAuthorizationHandler
    : AuthorizationHandler<AuthNetPermissionRequirement>
{
    public const string AdministratorRoleName = "Administrator";

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AuthNetPermissionRequirement requirement)
    {
        if (context.User.IsInRole(AdministratorRoleName) ||
            context.User.Claims.Any(claim =>
                claim.Type == AuthNetPermissions.ClaimType &&
                AuthNetPermissions.ClaimSatisfies(claim.Value, requirement.Permission)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}

internal sealed class AuthNetPermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
