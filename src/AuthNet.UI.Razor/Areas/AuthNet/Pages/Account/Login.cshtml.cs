using System.ComponentModel.DataAnnotations;
using AuthNet.Core;
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthNetRazor.Areas.AuthNet.Pages.Account;

public sealed class LoginModel(
    SignInManager<AuthNetUser> signInManager,
    AuthNetOptions authNetOptions)
    : PageModel
{
    [BindProperty]
    public LoginInput Input { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusMessage { get; set; }

    public bool EnablePublicRegistration => authNetOptions.EnablePublicRegistration;

    public IReadOnlyList<AuthenticationScheme> ExternalSchemes { get; private set; } = [];

    public async Task OnGetAsync()
    {
        ExternalSchemes = [.. await signInManager.GetExternalAuthenticationSchemesAsync()];
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ExternalSchemes = [.. await signInManager.GetExternalAuthenticationSchemesAsync()];

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(
            Input.Email,
            Input.Password,
            Input.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return LocalRedirect(ReturnUrl ?? "~/");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "This account is locked. Try again later.");
            return Page();
        }

        if (result.IsNotAllowed)
        {
            ModelState.AddModelError(string.Empty, "This account is not allowed to sign in yet.");
            return Page();
        }

        ModelState.AddModelError(string.Empty, "Invalid sign-in attempt.");
        return Page();
    }
}

public sealed class LoginInput
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember me")]
    public bool RememberMe { get; set; }
}
