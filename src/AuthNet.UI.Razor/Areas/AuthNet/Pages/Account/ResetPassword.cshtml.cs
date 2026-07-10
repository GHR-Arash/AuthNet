using System.ComponentModel.DataAnnotations;
using System.Text;
using AuthNet.Persistence.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace AuthNetRazor.Areas.AuthNet.Pages.Account;

public sealed class ResetPasswordModel(UserManager<AuthNetUser> userManager) : PageModel
{
    [BindProperty]
    public ResetPasswordInput Input { get; set; } = new();

    public bool Succeeded { get; private set; }

    public IActionResult OnGet(string? code)
    {
        if (code is null)
        {
            return BadRequest();
        }

        Input.Code = code;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await userManager.FindByEmailAsync(Input.Email);
        if (user is null)
        {
            Succeeded = true;
            return Page();
        }

        var code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(Input.Code));
        var result = await userManager.ResetPasswordAsync(user, code, Input.Password);
        if (result.Succeeded)
        {
            Succeeded = true;
            return Page();
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return Page();
    }
}

public sealed class ResetPasswordInput
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;
}
