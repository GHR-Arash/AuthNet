using System.ComponentModel.DataAnnotations;
using System.Net;
using AuthNet.Core;
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthNetRazor.Areas.AuthNet.Pages.Account;

[Authorize]
public sealed class MfaSetupModel(
    UserManager<AuthNetUser> userManager,
    AuthNetOptions authNetOptions) : PageModel
{
    [BindProperty]
    public MfaSetupInput Input { get; set; } = new();

    public string SharedKey { get; private set; } = string.Empty;

    public string AuthenticatorUri { get; private set; } = string.Empty;

    public IReadOnlyList<string> RecoveryCodes { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        await LoadSharedKeyAndUriAsync(user);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        await LoadSharedKeyAndUriAsync(user);

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var verificationCode = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var isValid = await userManager.VerifyTwoFactorTokenAsync(
            user,
            TokenOptions.DefaultAuthenticatorProvider,
            verificationCode);

        if (!isValid)
        {
            ModelState.AddModelError(string.Empty, "Authenticator code is invalid.");
            return Page();
        }

        var enableResult = await userManager.SetTwoFactorEnabledAsync(user, true);
        if (!enableResult.Succeeded)
        {
            foreach (var error in enableResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        RecoveryCodes = [.. await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10) ?? []];
        return Page();
    }

    private async Task LoadSharedKeyAndUriAsync(AuthNetUser user)
    {
        var unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrWhiteSpace(unformattedKey))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
        }

        SharedKey = FormatKey(unformattedKey ?? string.Empty);
        AuthenticatorUri = BuildAuthenticatorUri(user, unformattedKey ?? string.Empty);
    }

    private string BuildAuthenticatorUri(AuthNetUser user, string unformattedKey)
    {
        var issuer = string.IsNullOrWhiteSpace(authNetOptions.ApplicationName)
            ? "AuthNet"
            : authNetOptions.ApplicationName;
        var email = user.Email ?? user.UserName ?? user.Id;

        return "otpauth://totp/"
            + WebUtility.UrlEncode(issuer)
            + ":"
            + WebUtility.UrlEncode(email)
            + "?secret="
            + WebUtility.UrlEncode(unformattedKey)
            + "&issuer="
            + WebUtility.UrlEncode(issuer)
            + "&digits=6";
    }

    private static string FormatKey(string unformattedKey)
    {
        return string.Join(" ", unformattedKey.Chunk(4).Select(chunk => new string(chunk))).ToLowerInvariant();
    }
}

public sealed class MfaSetupInput
{
    [Required]
    [Display(Name = "Authenticator code")]
    public string Code { get; set; } = string.Empty;
}
