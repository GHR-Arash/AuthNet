namespace AuthNet.Core;

public static class AuthNetPermissions
{
    public const string ClaimType = "authnet.permission";

    public const string UsersView = "authnet.users.view";
    public const string UsersManage = "authnet.users.manage";
    public const string RolesView = "authnet.roles.view";
    public const string RolesManage = "authnet.roles.manage";
    public const string InvitationsManage = "authnet.invitations.manage";
    public const string AuditView = "authnet.audit.view";

    public static IReadOnlyList<AuthNetPermissionDefinition> All { get; } =
    [
        new(UsersView, "View users", "View users and user details."),
        new(UsersManage, "Manage users", "Create users and change user account state."),
        new(RolesView, "View roles", "View roles and role permissions."),
        new(RolesManage, "Manage roles", "Create roles and change role permissions."),
        new(InvitationsManage, "Manage invitations", "Create and view account invitations."),
        new(AuditView, "View audit", "View admin audit events.")
    ];

    public static bool IsKnown(string permission)
    {
        return All.Any(candidate => string.Equals(candidate.Value, permission, StringComparison.Ordinal));
    }

    public static bool ClaimSatisfies(string claimValue, string requiredPermission)
    {
        if (string.Equals(claimValue, requiredPermission, StringComparison.Ordinal))
        {
            return true;
        }

        return requiredPermission switch
        {
            UsersView => string.Equals(claimValue, UsersManage, StringComparison.Ordinal),
            RolesView => string.Equals(claimValue, RolesManage, StringComparison.Ordinal),
            _ => false
        };
    }
}

public sealed record AuthNetPermissionDefinition(string Value, string DisplayName, string Description);
