using System.Security.Claims;
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthNetRazor.Areas.AuthNet.Pages.Account;

public sealed class ExternalLoginModel(
    SignInManager<AuthNetUser> signInManager,
    UserManager<AuthNetUser> userManager)
    : PageModel
{
    public string? ErrorMessage { get; private set; }

    public IActionResult OnPost(string provider, string? returnUrl = null)
    {
        var redirectUrl = Url.Page("./ExternalLogin", "Callback", new { returnUrl });
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return Challenge(properties, provider);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string? returnUrl = null, string? remoteError = null)
    {
        if (remoteError is not null)
        {
            ErrorMessage = "External provider returned an error.";
            return Page();
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            ErrorMessage = "External login information was not available.";
            return Page();
        }

        var signInResult = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true);

        if (signInResult.Succeeded)
        {
            return LocalRedirect(returnUrl ?? "~/");
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            ErrorMessage = "The external provider did not return an email address.";
            return Page();
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            user = new AuthNetUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                DisplayName = info.Principal.Identity?.Name
            };

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                ErrorMessage = string.Join(" ", createResult.Errors.Select(error => error.Description));
                return Page();
            }
        }

        var addLoginResult = await userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded)
        {
            ErrorMessage = string.Join(" ", addLoginResult.Errors.Select(error => error.Description));
            return Page();
        }

        await signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
        return LocalRedirect(returnUrl ?? "~/");
    }
}
