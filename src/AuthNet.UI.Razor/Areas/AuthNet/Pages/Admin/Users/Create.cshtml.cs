using System.ComponentModel.DataAnnotations;
using AuthNet.Core;
using AuthNet.Persistence.Postgres;
using AuthNetRazor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthNetRazor.Areas.AuthNet.Pages.Admin.Users;

[Authorize(Policy = AuthNetPermissions.UsersManage)]
public sealed class CreateModel(
    UserManager<AuthNetUser> userManager,
    RoleManager<IdentityRole> roleManager,
    IAuthNetAuditWriter auditWriter) : PageModel
{
    private const string AdministratorRoleName = "Administrator";

    [BindProperty]
    public InputModel Input { get; set; } = new()
    {
        EmailConfirmed = true
    };

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var userName = Input.UserName.Trim();
        var email = Input.Email.Trim();
        var displayName = string.IsNullOrWhiteSpace(Input.DisplayName)
            ? null
            : Input.DisplayName.Trim();

        if (await userManager.FindByNameAsync(userName) is not null)
        {
            ModelState.AddModelError("Input.UserName", "A user with this username already exists.");
        }

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            ModelState.AddModelError("Input.Email", "A user with this email already exists.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = new AuthNetUser
        {
            UserName = userName,
            Email = email,
            DisplayName = displayName,
            EmailConfirmed = Input.EmailConfirmed,
            LockoutEnabled = true
        };

        var createResult = await userManager.CreateAsync(user, Input.Password);
        if (!createResult.Succeeded)
        {
            AddErrors(createResult);
            return Page();
        }

        if (Input.GrantAdministrator)
        {
            var roleResult = await EnsureAdministratorRoleAsync();
            if (!roleResult.Succeeded)
            {
                AddErrors(roleResult);
                return Page();
            }

            var addRoleResult = await userManager.AddToRoleAsync(user, AdministratorRoleName);
            if (!addRoleResult.Succeeded)
            {
                AddErrors(addRoleResult);
                return Page();
            }
        }

        await auditWriter.RecordAsync(
            User,
            "UserCreated",
            user,
            metadata: $"EmailConfirmed={Input.EmailConfirmed};GrantAdministrator={Input.GrantAdministrator}",
            cancellationToken: HttpContext.RequestAborted);

        return RedirectToPage("./Detail", new { id = user.Id });
    }

    private async Task<IdentityResult> EnsureAdministratorRoleAsync()
    {
        if (await roleManager.RoleExistsAsync(AdministratorRoleName))
        {
            return IdentityResult.Success;
        }

        return await roleManager.CreateAsync(new IdentityRole(AdministratorRoleName));
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    public sealed class InputModel
    {
        [Required]
        [Display(Name = "Username")]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Display name")]
        public string? DisplayName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name = "Confirm password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [Display(Name = "Mark email confirmed")]
        public bool EmailConfirmed { get; set; }

        [Display(Name = "Grant administrator access")]
        public bool GrantAdministrator { get; set; }
    }
}
