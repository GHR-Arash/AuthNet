using System.ComponentModel.DataAnnotations;
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthNetRazor.Areas.AuthNet.Pages.Account;

[Authorize]
public sealed class ChangePasswordModel(
    UserManager<AuthNetUser> userManager,
    SignInManager<AuthNetUser> signInManager)
    : PageModel
{
    [BindProperty]
    public ChangePasswordInput Input { get; set; } = new();

    public string? StatusMessage { get; private set; }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await userManager.ChangePasswordAsync(user, Input.CurrentPassword, Input.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        await signInManager.RefreshSignInAsync(user);
        StatusMessage = "Password changed.";
        return Page();
    }
}

public sealed class ChangePasswordInput
{
    [Required, DataType(DataType.Password), Display(Name = "Current password")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Display(Name = "New password")]
    public string NewPassword { get; set; } = string.Empty;
}
