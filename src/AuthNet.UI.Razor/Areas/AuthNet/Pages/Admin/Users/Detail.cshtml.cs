using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthNetRazor.Areas.AuthNet.Pages.Admin.Users;

[Authorize(Roles = "Administrator")]
public sealed class DetailModel(UserManager<AuthNetUser> userManager) : PageModel
{
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

        StatusMessage = "Access failed count reset.";
        return await LoadPageAsync(id);
    }

    private async Task<IActionResult> LoadPageAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

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
            [.. await userManager.GetRolesAsync(user)]);

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
        []);
}
