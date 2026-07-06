using System.Text;
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace AuthNetRazor.Areas.AuthNet.Pages.Account;

public sealed class ConfirmEmailModel(UserManager<AuthNetUser> userManager) : PageModel
{
    public bool Succeeded { get; private set; }

    public string Message { get; private set; } = "Unable to confirm email.";

    public async Task OnGetAsync(string? userId, string? code)
    {
        if (userId is null || code is null)
        {
            return;
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return;
        }

        var decodedCode = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
        var result = await userManager.ConfirmEmailAsync(user, decodedCode);
        Succeeded = result.Succeeded;
        Message = result.Succeeded ? "Email confirmed. You can now sign in." : "Email confirmation failed.";
    }
}
