using AuthNet.Core;
using AuthNet.Persistence.EntityFrameworkCore;
using AuthNetRazor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AuthNetRazor.Areas.AuthNet.Pages.Admin.Users;

[Authorize(Policy = AuthNetPermissions.UsersManage)]
public sealed class DetailModel(
    UserManager<AuthNetUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IAuthNetAuditWriter auditWriter) : PageModel
{
    private const string AdministratorRoleName = "Administrator";

    public AdminUserDetail UserDetail { get; private set; } = AdminUserDetail.Empty;

    public IReadOnlyList<string> AvailableRoles { get; private set; } = [];

    public string? StatusMessage { get; private set; }

    [BindProperty]
    public RoleInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(string id)
    {
        return await LoadPageAsync(id);
    }

    public async Task<IActionResult> OnPostConfirmEmailAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        user.EmailConfirmed = true;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return await LoadPageAsync(id);
        }

        await auditWriter.RecordAsync(User, "UserEmailConfirmed", user, metadata: "EmailConfirmed=true", cancellationToken: HttpContext.RequestAborted);
        StatusMessage = "Email confirmed.";
        return await LoadPageAsync(id);
    }

    public async Task<IActionResult> OnPostUnconfirmEmailAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        user.EmailConfirmed = false;
        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return await LoadPageAsync(id);
        }

        await auditWriter.RecordAsync(User, "UserEmailUnconfirmed", user, metadata: "EmailConfirmed=false", cancellationToken: HttpContext.RequestAborted);
        StatusMessage = "Email unconfirmed.";
        return await LoadPageAsync(id);
    }

    public async Task<IActionResult> OnPostLockAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var result = await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        if (!result.Succeeded)
        {
            AddErrors(result);
            return await LoadPageAsync(id);
        }

        await auditWriter.RecordAsync(User, "UserLocked", user, metadata: "LockoutEnd=100years", cancellationToken: HttpContext.RequestAborted);
        StatusMessage = "User locked.";
        return await LoadPageAsync(id);
    }

    public async Task<IActionResult> OnPostUnlockAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var result = await userManager.SetLockoutEndDateAsync(user, null);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return await LoadPageAsync(id);
        }

        await auditWriter.RecordAsync(User, "UserUnlocked", user, metadata: "LockoutEnd=null", cancellationToken: HttpContext.RequestAborted);
        StatusMessage = "User unlocked.";
        return await LoadPageAsync(id);
    }

    public async Task<IActionResult> OnPostResetAccessFailedCountAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var result = await userManager.ResetAccessFailedCountAsync(user);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return await LoadPageAsync(id);
        }

        await auditWriter.RecordAsync(User, "UserAccessFailuresReset", user, metadata: "AccessFailedCount=0", cancellationToken: HttpContext.RequestAborted);
        StatusMessage = "Access failed count reset.";
        return await LoadPageAsync(id);
    }

    public async Task<IActionResult> OnPostGrantAdministratorAsync(string id)
    {
        if (!await roleManager.RoleExistsAsync(AdministratorRoleName))
        {
            var createRoleResult = await roleManager.CreateAsync(new IdentityRole(AdministratorRoleName));
            if (!createRoleResult.Succeeded)
            {
                AddErrors(createRoleResult);
                return await LoadPageAsync(id);
            }
        }

        Input.RoleName = AdministratorRoleName;
        return await AddRoleAsync(id, "Administrator access granted.", "AdministratorGranted");
    }

    public async Task<IActionResult> OnPostRemoveAdministratorAsync(string id)
    {
        return await RemoveRoleAsync(
            id,
            AdministratorRoleName,
            "Administrator access removed.",
            "AdministratorRemoved");
    }

    public async Task<IActionResult> OnPostAddRoleAsync(string id)
    {
        return await AddRoleAsync(id, "Role assigned.", "UserRoleAssigned");
    }

    public async Task<IActionResult> OnPostRemoveRoleAsync(string id, string roleName)
    {
        return await RemoveRoleAsync(id, roleName, "Role removed.", "UserRoleRemoved");
    }

    private async Task<IActionResult> AddRoleAsync(string id, string successMessage, string auditAction)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var roleName = Input.RoleName.Trim();
        if (string.IsNullOrWhiteSpace(roleName))
        {
            ModelState.AddModelError("Input.RoleName", "Role is required.");
            return await LoadPageAsync(id);
        }

        if (!await roleManager.RoleExistsAsync(roleName))
        {
            ModelState.AddModelError("Input.RoleName", "Role does not exist.");
            return await LoadPageAsync(id);
        }

        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            var addRoleResult = await userManager.AddToRoleAsync(user, roleName);
            if (!addRoleResult.Succeeded)
            {
                AddErrors(addRoleResult);
                return await LoadPageAsync(id);
            }
        }

        await auditWriter.RecordAsync(User, auditAction, user, metadata: $"Role={roleName}", cancellationToken: HttpContext.RequestAborted);
        StatusMessage = successMessage;
        return await LoadPageAsync(id);
    }

    private async Task<IActionResult> RemoveRoleAsync(
        string id,
        string roleName,
        string successMessage,
        string auditAction)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(roleName))
        {
            ModelState.AddModelError(string.Empty, "Role is required.");
            return await LoadPageAsync(id);
        }

        if (!await userManager.IsInRoleAsync(user, roleName))
        {
            StatusMessage = successMessage;
            return await LoadPageAsync(id);
        }

        if (string.Equals(roleName, AdministratorRoleName, StringComparison.Ordinal))
        {
            var administrators = await userManager.GetUsersInRoleAsync(AdministratorRoleName);
            if (administrators.Count <= 1)
            {
                ModelState.AddModelError(string.Empty, "Cannot remove administrator access from the last administrator.");
                return await LoadPageAsync(id);
            }
        }

        var removeRoleResult = await userManager.RemoveFromRoleAsync(user, roleName);
        if (!removeRoleResult.Succeeded)
        {
            AddErrors(removeRoleResult);
            return await LoadPageAsync(id);
        }

        await auditWriter.RecordAsync(User, auditAction, user, metadata: $"Role={roleName}", cancellationToken: HttpContext.RequestAborted);
        StatusMessage = successMessage;
        return await LoadPageAsync(id);
    }

    private async Task<IActionResult> LoadPageAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var roles = (await userManager.GetRolesAsync(user))
            .OrderBy(role => role)
            .ToList();
        AvailableRoles = await roleManager.Roles
            .AsNoTracking()
            .Where(role => role.Name != null && !roles.Contains(role.Name))
            .OrderBy(role => role.Name)
            .Select(role => role.Name!)
            .ToListAsync();

        UserDetail = new AdminUserDetail(
            user.Id,
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty,
            user.DisplayName,
            user.PhoneNumber,
            user.EmailConfirmed,
            await userManager.IsLockedOutAsync(user),
            user.AccessFailedCount,
            (await userManager.GetLoginsAsync(user)).Count,
            roles.Contains(AdministratorRoleName),
            roles);

        return Page();
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    public sealed class RoleInput
    {
        public string RoleName { get; set; } = string.Empty;
    }
}

public sealed record AdminUserDetail(
    string Id,
    string Email,
    string UserName,
    string? DisplayName,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool IsLockedOut,
    int AccessFailedCount,
    int ExternalLoginCount,
    bool IsAdministrator,
    IReadOnlyList<string> Roles)
{
    public static AdminUserDetail Empty { get; } = new(
        string.Empty,
        string.Empty,
        string.Empty,
        null,
        null,
        false,
        false,
        0,
        0,
        false,
        []);
}
