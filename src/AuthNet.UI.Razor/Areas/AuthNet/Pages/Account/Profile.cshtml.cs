using System.ComponentModel.DataAnnotations;
using AuthNet.Core.Email;
using AuthNet.Persistence.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthNetRazor.Areas.AuthNet.Pages.Account;

[Authorize]
public sealed class ProfileModel(
    UserManager<AuthNetUser> userManager,
    SignInManager<AuthNetUser> signInManager,
    IAuthNetEmailSender emailSender) : PageModel
{
    [BindProperty]
    public ProfileInput Input { get; set; } = new();

    public string? StatusMessage { get; private set; }

    public IReadOnlyList<AuthenticationScheme> ExternalSchemes { get; private set; } = [];

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

        await LoadExternalSchemesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        await LoadExternalSchemesAsync();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var currentEmail = user.Email ?? string.Empty;
        var requestedEmail = Input.Email.Trim();

        user.DisplayName = Input.DisplayName;
        user.PhoneNumber = Input.PhoneNumber;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            Input.Email = currentEmail;
            return Page();
        }

        if (!string.Equals(requestedEmail, currentEmail, StringComparison.OrdinalIgnoreCase))
        {
            var token = await userManager.GenerateChangeEmailTokenAsync(user, requestedEmail);
            var callbackUrl = AccountEmailMessages.BuildConfirmEmailUrl(this, user.Id, token, requestedEmail);

            await emailSender.SendAsync(
                AccountEmailMessages.CreateChangeEmailMessage(requestedEmail, callbackUrl),
                HttpContext.RequestAborted);

            StatusMessage = "Profile updated. Check the new email address to confirm the change.";
            Input.Email = currentEmail;
            return Page();
        }

        StatusMessage = "Profile updated.";
        Input.Email = currentEmail;
        return Page();
    }

    private async Task LoadExternalSchemesAsync()
    {
        ExternalSchemes = [.. await signInManager.GetExternalAuthenticationSchemesAsync()];
    }
}

public sealed class ProfileInput
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? DisplayName { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }
}
