using AuthNet.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AuthNetRazor.Areas.AuthNet.Pages.Admin.Roles;

[Authorize(Policy = AuthNetPermissions.RolesView)]
public sealed class IndexModel(RoleManager<IdentityRole> roleManager) : PageModel
{
    public IReadOnlyList<RoleListItem> Roles { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Roles = await roleManager.Roles
            .AsNoTracking()
            .OrderBy(role => role.Name)
            .Select(role => new RoleListItem(
                role.Id,
                role.Name ?? string.Empty))
            .ToListAsync();
    }
}

public sealed record RoleListItem(string Id, string Name);
