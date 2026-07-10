using System.ComponentModel.DataAnnotations;
using System.Text;
using AuthNet.Core.Email;
using AuthNet.Persistence.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace AuthNetRazor.Areas.AuthNet.Pages.Account;

public sealed class ForgotPasswordModel(
    UserManager<AuthNetUser> userManager,
    IAuthNetEmailSender emailSender)
    : PageModel
{
    [BindProperty]
    public ForgotPasswordInput Input { get; set; } = new();

    public bool Sent { get; private set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await userManager.FindByEmailAsync(Input.Email);
        if (user is not null && await userManager.IsEmailConfirmedAsync(user))
        {
            var code = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Page(
                "/Account/ResetPassword",
                pageHandler: null,
                values: new { area = "AuthNet", code = encodedCode },
                protocol: Request.Scheme)!;

            await emailSender.SendAsync(new AuthNetEmailMessage(
                Input.Email,
                "Reset your password",
                $"Reset your password by <a href=\"{callbackUrl}\">clicking here</a>.",
                $"Reset your password: {callbackUrl}"),
                HttpContext.RequestAborted);
        }

        Sent = true;
        return Page();
    }
}

public sealed class ForgotPasswordInput
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
