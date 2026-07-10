using System.ComponentModel.DataAnnotations;
using AuthNet.Persistence.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthNetRazor.Areas.AuthNet.Pages.Account;

public sealed class LoginWithMfaModel(SignInManager<AuthNetUser> signInManager) : PageModel
{
    [BindProperty]
    public LoginWithMfaInput Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool RememberMe { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        return user is null ? RedirectToPage("./Login") : Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var verificationCode = Input.Code.Replace(" ", string.Empty).Replace("-", string.Empty);
        var result = await signInManager.TwoFactorAuthenticatorSignInAsync(
            verificationCode,
            RememberMe,
            rememberClient: false);

        if (result.Succeeded)
        {
            return LocalRedirect(ReturnUrl ?? "~/");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account is locked. Try again later.");
            return Page();
        }

        ModelState.AddModelError(string.Empty, "Invalid authenticator code.");
        return Page();
    }
}

public sealed class LoginWithMfaInput
{
    [Required]
    [Display(Name = "Authenticator code")]
    public string Code { get; set; } = string.Empty;
}
