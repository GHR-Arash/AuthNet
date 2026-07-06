using System.ComponentModel.DataAnnotations;
using System.Text;
using AuthNet.Core;
using AuthNet.Core.Email;
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace AuthNetRazor.Areas.AuthNet.Pages.Account;

public sealed class RegisterModel(
    UserManager<AuthNetUser> userManager,
    IAuthNetEmailSender emailSender,
    AuthNetOptions authNetOptions)
    : PageModel
{
    [BindProperty]
    public RegisterInput Input { get; set; } = new();

    public IActionResult OnGet()
    {
        return authNetOptions.EnablePublicRegistration ? Page() : NotFound();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!authNetOptions.EnablePublicRegistration)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new AuthNetUser
        {
            UserName = Input.Email,
            Email = Input.Email,
            DisplayName = Input.DisplayName
        };

        var result = await userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedCode = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
        var callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new { area = "AuthNet", userId = user.Id, code = encodedCode },
            protocol: Request.Scheme)!;

        await emailSender.SendAsync(new AuthNetEmailMessage(
            Input.Email,
            "Confirm your email",
            $"Confirm your account by <a href=\"{callbackUrl}\">clicking here</a>.",
            $"Confirm your account: {callbackUrl}"),
            HttpContext.RequestAborted);

        return RedirectToPage("./Login", new { statusMessage = "Account created. Check email to confirm your account." });
    }
}

public sealed class RegisterInput
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? DisplayName { get; set; }

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
