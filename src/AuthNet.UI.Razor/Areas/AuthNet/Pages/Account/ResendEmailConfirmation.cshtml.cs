using System.ComponentModel.DataAnnotations;
using AuthNet.Core.Email;
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthNetRazor.Areas.AuthNet.Pages.Account;

public sealed class ResendEmailConfirmationModel(
    UserManager<AuthNetUser> userManager,
    IAuthNetEmailSender emailSender) : PageModel
{
    [BindProperty]
    public ResendEmailConfirmationInput Input { get; set; } = new();

    public bool Sent { get; private set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await userManager.FindByEmailAsync(Input.Email);
        if (user is not null && !await userManager.IsEmailConfirmedAsync(user))
        {
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = AccountEmailMessages.BuildConfirmEmailUrl(this, user.Id, code);

            await emailSender.SendAsync(
                AccountEmailMessages.CreateConfirmEmailMessage(Input.Email, callbackUrl),
                HttpContext.RequestAborted);
        }

        Sent = true;
        return Page();
    }
}

public sealed class ResendEmailConfirmationInput
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
