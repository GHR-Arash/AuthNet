using AuthNet.Persistence.Postgres;
using AuthNetRazor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthNetRazor.Areas.AuthNet.Pages.Admin.Users;

[Authorize(Roles = "Administrator")]
public sealed class DetailModel(
    UserManager<AuthNetUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IAuthNetAuditWriter auditWriter) : PageModel
{
    private const string AdministratorRoleName = "Administrator";

    public AdminUserDetail UserDetail { get; private set; } = AdminUserDetail.Empty;

    public string? StatusMessage { get; private set; }

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
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (!await roleManager.RoleExistsAsync(AdministratorRoleName))
        {
            var createRoleResult = await roleManager.CreateAsync(new IdentityRole(AdministratorRoleName));
            if (!createRoleResult.Succeeded)
            {
                AddErrors(createRoleResult);
                return await LoadPageAsync(id);
            }
        }

        if (!await userManager.IsInRoleAsync(user, AdministratorRoleName))
        {
            var addRoleResult = await userManager.AddToRoleAsync(user, AdministratorRoleName);
            if (!addRoleResult.Succeeded)
            {
                AddErrors(addRoleResult);
                return await LoadPageAsync(id);
            }
        }

        await auditWriter.RecordAsync(User, "AdministratorGranted", user, metadata: "Role=Administrator", cancellationToken: HttpContext.RequestAborted);
        StatusMessage = "Administrator access granted.";
        return await LoadPageAsync(id);
    }

    public async Task<IActionResult> OnPostRemoveAdministratorAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        if (!await userManager.IsInRoleAsync(user, AdministratorRoleName))
        {
            StatusMessage = "Administrator access removed.";
            return await LoadPageAsync(id);
        }

        var administrators = await userManager.GetUsersInRoleAsync(AdministratorRoleName);
        if (administrators.Count <= 1)
        {
            ModelState.AddModelError(string.Empty, "Cannot remove administrator access from the last administrator.");
            return await LoadPageAsync(id);
        }

        var removeRoleResult = await userManager.RemoveFromRoleAsync(user, AdministratorRoleName);
        if (!removeRoleResult.Succeeded)
        {
            AddErrors(removeRoleResult);
            return await LoadPageAsync(id);
        }

        await auditWriter.RecordAsync(User, "AdministratorRemoved", user, metadata: "Role=Administrator", cancellationToken: HttpContext.RequestAborted);
        StatusMessage = "Administrator access removed.";
        return await LoadPageAsync(id);
    }

    private async Task<IActionResult> LoadPageAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await userManager.GetRolesAsync(user);

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
            [.. roles]);

        return Page();
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
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
