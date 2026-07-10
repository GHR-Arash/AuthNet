using AuthNet.Core;
using AuthNet.Persistence.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AuthNetRazor.Areas.AuthNet.Pages.Admin.Audit;

[Authorize(Policy = AuthNetPermissions.AuditView)]
public sealed class IndexModel(AuthNetDbContext dbContext) : PageModel
{
    private const int PageSize = 100;

    [BindProperty(SupportsGet = true)]
    public string? Action { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Actor { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Target { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? From { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? To { get; set; }

    public IReadOnlyList<AuditEventListItem> Events { get; private set; } = [];

    public async Task OnGetAsync()
    {
        var query = dbContext.AuditEvents.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(Action))
        {
            var action = Action.Trim();
            query = query.Where(item => item.Action.Contains(action));
        }

        if (!string.IsNullOrWhiteSpace(Actor))
        {
            var actor = Actor.Trim();
            query = query.Where(item =>
                (item.ActorEmail != null && item.ActorEmail.Contains(actor)) ||
                (item.ActorUserId != null && item.ActorUserId.Contains(actor)));
        }

        if (!string.IsNullOrWhiteSpace(Target))
        {
            var target = Target.Trim();
            query = query.Where(item =>
                (item.TargetEmail != null && item.TargetEmail.Contains(target)) ||
                (item.TargetUserId != null && item.TargetUserId.Contains(target)));
        }

        if (DateTimeOffset.TryParse(From, out var from))
        {
            query = query.Where(item => item.CreatedAtUtc >= from);
        }

        if (DateTimeOffset.TryParse(To, out var to))
        {
            query = query.Where(item => item.CreatedAtUtc <= to);
        }

        Events = await query
            .OrderByDescending(item => item.CreatedAtUtc)
            .Take(PageSize)
            .Select(item => new AuditEventListItem(
                item.CreatedAtUtc,
                item.Action,
                item.Outcome,
                item.ActorEmail ?? item.ActorUserId ?? string.Empty,
                item.TargetEmail ?? item.TargetUserId ?? string.Empty,
                item.Metadata))
            .ToListAsync();
    }
}

public sealed record AuditEventListItem(
    DateTimeOffset CreatedAtUtc,
    string Action,
    string Outcome,
    string Actor,
    string Target,
    string? Metadata);
