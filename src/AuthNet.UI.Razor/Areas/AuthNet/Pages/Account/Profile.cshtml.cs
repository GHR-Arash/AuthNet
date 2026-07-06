using System.ComponentModel.DataAnnotations;
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthNetRazor.Areas.AuthNet.Pages.Account;

[Authorize]
public sealed class ProfileModel(UserManager<AuthNetUser> userManager) : PageModel
{
    [BindProperty]
    public ProfileInput Input { get; set; } = new();

    public string? StatusMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        Input = new ProfileInput
        {
            Email = user.Email ?? string.Empty,
            DisplayName = user.DisplayName,
            PhoneNumber = user.PhoneNumber
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            Input.Email = user.Email ?? string.Empty;
            return Page();
        }

        user.DisplayName = Input.DisplayName;
        user.PhoneNumber = Input.PhoneNumber;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            Input.Email = user.Email ?? string.Empty;
            return Page();
        }

        StatusMessage = "Profile updated.";
        Input.Email = user.Email ?? string.Empty;
        return Page();
    }
}

public sealed class ProfileInput
{
    public string Email { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? DisplayName { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }
}
