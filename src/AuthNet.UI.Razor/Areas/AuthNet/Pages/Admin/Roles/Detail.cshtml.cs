using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AuthNet.Core;
using AuthNetRazor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthNetRazor.Areas.AuthNet.Pages.Admin.Roles;

[Authorize(Policy = AuthNetPermissions.RolesView)]
public sealed class DetailModel(
    RoleManager<IdentityRole> roleManager,
    IAuthNetAuditWriter auditWriter) : PageModel
{
    public RoleDetail RoleDetail { get; private set; } = RoleDetail.Empty;

    public IReadOnlyList<AuthNetPermissionDefinition> AvailablePermissions { get; private set; } = [];

    public string? StatusMessage { get; private set; }

    [BindProperty]
    public PermissionInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        return await LoadPageAsync(id);
    }

    public async Task<IActionResult> OnPostAddPermissionAsync(string id)
    {
        if (!UserHasManageAccess())
        {
            return Forbid();
        }

        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        var permission = Input.Permission.Trim();
        if (!AuthNetPermissions.IsKnown(permission))
        {
            ModelState.AddModelError("Input.Permission", "Unknown permission.");
            return await LoadPageAsync(id);
        }

        var claims = await roleManager.GetClaimsAsync(role);
        if (!claims.Any(claim => claim.Type == AuthNetPermissions.ClaimType && claim.Value == permission))
        {
            var result = await roleManager.AddClaimAsync(
                role,
                new Claim(AuthNetPermissions.ClaimType, permission));
            if (!result.Succeeded)
            {
                AddErrors(result);
                return await LoadPageAsync(id);
            }
        }

        await auditWriter.RecordAsync(
            User,
            "RolePermissionAdded",
            metadata: $"RoleId={role.Id};Role={role.Name};Permission={permission}",
            cancellationToken: HttpContext.RequestAborted);

        StatusMessage = "Permission added.";
        return await LoadPageAsync(id);
    }

    public async Task<IActionResult> OnPostRemovePermissionAsync(string id, string permission)
    {
        if (!UserHasManageAccess())
        {
            return Forbid();
        }

        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        if (!AuthNetPermissions.IsKnown(permission))
        {
            ModelState.AddModelError(string.Empty, "Unknown permission.");
            return await LoadPageAsync(id);
        }

        var claims = await roleManager.GetClaimsAsync(role);
        var claim = claims.FirstOrDefault(candidate =>
            candidate.Type == AuthNetPermissions.ClaimType &&
            candidate.Value == permission);
        if (claim is not null)
        {
            var result = await roleManager.RemoveClaimAsync(role, claim);
            if (!result.Succeeded)
            {
                AddErrors(result);
                return await LoadPageAsync(id);
            }
        }

        await auditWriter.RecordAsync(
            User,
            "RolePermissionRemoved",
            metadata: $"RoleId={role.Id};Role={role.Name};Permission={permission}",
            cancellationToken: HttpContext.RequestAborted);

        StatusMessage = "Permission removed.";
        return await LoadPageAsync(id);
    }

    private async Task<IActionResult> LoadPageAsync(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role is null)
        {
            return NotFound();
        }

        var permissions = (await roleManager.GetClaimsAsync(role))
            .Where(claim => claim.Type == AuthNetPermissions.ClaimType)
            .Select(claim => claim.Value)
            .Where(AuthNetPermissions.IsKnown)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(permission => permission)
            .ToList();

        RoleDetail = new RoleDetail(role.Id, role.Name ?? string.Empty, permissions);
        AvailablePermissions = [.. AuthNetPermissions.All.Where(permission => !permissions.Contains(permission.Value, StringComparer.Ordinal))];
        return Page();
    }

    private bool UserHasManageAccess()
    {
        return User.IsInRole("Administrator") ||
            User.Claims.Any(claim =>
                claim.Type == AuthNetPermissions.ClaimType &&
                AuthNetPermissions.ClaimSatisfies(claim.Value, AuthNetPermissions.RolesManage));
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    public sealed class PermissionInput
    {
        [Required]
        public string Permission { get; set; } = string.Empty;
    }
}

public sealed record RoleDetail(string Id, string Name, IReadOnlyList<string> Permissions)
{
    public static RoleDetail Empty { get; } = new(string.Empty, string.Empty, []);
}
