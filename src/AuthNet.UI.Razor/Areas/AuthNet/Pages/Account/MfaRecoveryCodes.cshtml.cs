using AuthNet.Persistence.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthNetRazor.Areas.AuthNet.Pages.Account;

[Authorize]
public sealed class MfaRecoveryCodesModel(UserManager<AuthNetUser> userManager) : PageModel
{
    public int RecoveryCodesLeft { get; private set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Challenge();
        }

        RecoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user);
        return Page();
    }
}
