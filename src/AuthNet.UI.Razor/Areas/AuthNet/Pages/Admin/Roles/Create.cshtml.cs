using System.ComponentModel.DataAnnotations;
using AuthNet.Core;
using AuthNetRazor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AuthNetRazor.Areas.AuthNet.Pages.Admin.Roles;

[Authorize(Policy = AuthNetPermissions.RolesManage)]
public sealed class CreateModel(
    RoleManager<IdentityRole> roleManager,
    IAuthNetAuditWriter auditWriter) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var roleName = Input.Name.Trim();
        if (await roleManager.RoleExistsAsync(roleName))
        {
            ModelState.AddModelError("Input.Name", "A role with this name already exists.");
            return Page();
        }

        var role = new IdentityRole(roleName);
        var result = await roleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            AddErrors(result);
            return Page();
        }

        await auditWriter.RecordAsync(
            User,
            "RoleCreated",
            metadata: $"RoleId={role.Id};Role={role.Name}",
            cancellationToken: HttpContext.RequestAborted);

        return RedirectToPage("./Detail", new { id = role.Id });
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
        [Display(Name = "Role name")]
        public string Name { get; set; } = string.Empty;
    }
}
