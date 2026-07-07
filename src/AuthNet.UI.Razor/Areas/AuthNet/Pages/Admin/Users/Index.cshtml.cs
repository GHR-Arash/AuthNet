using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AuthNetRazor.Areas.AuthNet.Pages.Admin.Users;

[Authorize(Roles = "Administrator")]
public sealed class IndexModel(UserManager<AuthNetUser> userManager) : PageModel
{
    private const int PageSize = 20;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public IReadOnlyList<AdminUserListItem> Users { get; private set; } = [];

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage { get; private set; }

    public async Task OnGetAsync()
    {
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        var query = userManager.Users.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(Search))
        {
            var search = Search.Trim();
            query = query.Where(user =>
                (user.Email != null && user.Email.Contains(search)) ||
                (user.DisplayName != null && user.DisplayName.Contains(search)));
        }

        var users = await query
            .OrderBy(user => user.Email)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize + 1)
            .ToListAsync();

        HasNextPage = users.Count > PageSize;
        Users = [.. users.Take(PageSize).Select(user => new AdminUserListItem(
            user.Id,
            user.Email ?? string.Empty,
            user.DisplayName,
            user.EmailConfirmed,
            user.LockoutEnd is not null && user.LockoutEnd > DateTimeOffset.UtcNow))];
    }
}

public sealed record AdminUserListItem(
    string Id,
    string Email,
    string? DisplayName,
    bool EmailConfirmed,
    bool IsLockedOut);
