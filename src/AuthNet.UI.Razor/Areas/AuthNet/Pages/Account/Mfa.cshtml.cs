using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthNetRazor.Areas.AuthNet.Pages.Account;

[Authorize]
public sealed class MfaModel(UserManager<AuthNetUser> userManager) : PageModel
{
    public bool IsMfaEnabled { get; private set; }

    public bool HasAuthenticator { get; private set; }

    public int RecoveryCodesLeft { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        IsMfaEnabled = await userManager.GetTwoFactorEnabledAsync(user);
        HasAuthenticator = !string.IsNullOrWhiteSpace(await userManager.GetAuthenticatorKeyAsync(user));
        RecoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user);

        return Page();
    }
}
