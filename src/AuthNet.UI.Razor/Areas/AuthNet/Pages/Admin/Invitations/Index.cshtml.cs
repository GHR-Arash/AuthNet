using AuthNet.Core;
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AuthNetRazor.Areas.AuthNet.Pages.Admin.Invitations;

[Authorize(Policy = AuthNetPermissions.InvitationsManage)]
public sealed class IndexModel(AuthNetDbContext dbContext) : PageModel
{
    private const int PageSize = 20;

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public IReadOnlyList<InvitationListItem> Invitations { get; private set; } = [];

    public bool HasPreviousPage => PageNumber > 1;

    public bool HasNextPage { get; private set; }

    public async Task OnGetAsync()
    {
        if (PageNumber < 1)
        {
            PageNumber = 1;
        }

        var now = DateTimeOffset.UtcNow;
        var invitations = await dbContext.Invitations
            .AsNoTracking()
            .OrderByDescending(invitation => invitation.CreatedAtUtc)
            .Skip((PageNumber - 1) * PageSize)
            .Take(PageSize + 1)
            .ToListAsync();

        HasNextPage = invitations.Count > PageSize;
        Invitations = [.. invitations.Take(PageSize).Select(invitation => new InvitationListItem(
            invitation.Email,
            invitation.CreatedAtUtc,
            invitation.ExpiresAtUtc,
            invitation.AcceptedAtUtc,
            GetStatus(invitation, now)))];
    }

    private static string GetStatus(AuthNetInvitation invitation, DateTimeOffset now)
    {
        if (invitation.IsAccepted)
        {
            return "Accepted";
        }

        return invitation.IsExpired(now) ? "Expired" : "Pending";
    }
}

public sealed record InvitationListItem(
    string Email,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc,
    DateTimeOffset? AcceptedAtUtc,
    string Status);
