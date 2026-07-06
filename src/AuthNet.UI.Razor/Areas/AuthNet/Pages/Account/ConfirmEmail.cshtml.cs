using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthNetRazor.Areas.AuthNet.Pages.Account;

public sealed class ConfirmEmailModel(UserManager<AuthNetUser> userManager) : PageModel
{
    public bool Succeeded { get; private set; }

    public string Message { get; private set; } = "Unable to confirm email.";

    public async Task OnGetAsync(string? userId, string? code, string? changedEmail = null)
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

        if (!AccountEmailMessages.TryDecodeToken(code, out var decodedCode))
        {
            Message = "Email confirmation failed.";
            return;
        }

        var result = string.IsNullOrWhiteSpace(changedEmail)
            ? await userManager.ConfirmEmailAsync(user, decodedCode)
            : await userManager.ChangeEmailAsync(user, changedEmail, decodedCode);

        if (result.Succeeded && !string.IsNullOrWhiteSpace(changedEmail))
        {
            result = await userManager.SetUserNameAsync(user, changedEmail);
        }

        Succeeded = result.Succeeded;
        Message = result.Succeeded
            ? "Email confirmed. You can now sign in."
            : "Email confirmation failed.";
    }
}
