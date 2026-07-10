using System.ComponentModel.DataAnnotations;
using AuthNet.Persistence.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthNetRazor.Areas.AuthNet.Pages.Account;

public sealed class LoginWithRecoveryCodeModel(SignInManager<AuthNetUser> signInManager) : PageModel
{
    [BindProperty]
    public LoginWithRecoveryCodeInput Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

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

        var recoveryCode = Input.RecoveryCode.Replace(" ", string.Empty);
        var result = await signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

        if (result.Succeeded)
        {
            return LocalRedirect(ReturnUrl ?? "~/");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account is locked. Try again later.");
            return Page();
        }

        ModelState.AddModelError(string.Empty, "Invalid recovery code.");
        return Page();
    }
}

public sealed class LoginWithRecoveryCodeInput
{
    [Required]
    [Display(Name = "Recovery code")]
    public string RecoveryCode { get; set; } = string.Empty;
}
