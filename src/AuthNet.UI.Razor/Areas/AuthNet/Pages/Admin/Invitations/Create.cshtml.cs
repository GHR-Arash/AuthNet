using System.ComponentModel.DataAnnotations;
using AuthNet.Core;
using AuthNet.Core.Email;
using AuthNet.Persistence.Postgres;
using AuthNetRazor;
using AuthNetRazor.Areas.AuthNet.Pages.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AuthNetRazor.Areas.AuthNet.Pages.Admin.Invitations;

[Authorize(Policy = AuthNetPermissions.InvitationsManage)]
public sealed class CreateModel(
    AuthNetDbContext dbContext,
    UserManager<AuthNetUser> userManager,
    IAuthNetEmailSender emailSender,
    AuthNetOptions authNetOptions,
    IAuthNetAuditWriter auditWriter) : PageModel
{
    [BindProperty]
    public InvitationInput Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var email = Input.Email.Trim();
        var normalizedEmail = userManager.NormalizeEmail(email);
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            ModelState.AddModelError("Input.Email", "A user with this email already exists.");
            return Page();
        }

        var now = DateTimeOffset.UtcNow;
        var pendingExists = await dbContext.Invitations.AnyAsync(invitation =>
            invitation.NormalizedEmail == normalizedEmail &&
            invitation.AcceptedAtUtc == null &&
            invitation.ExpiresAtUtc > now);
        if (pendingExists)
        {
            ModelState.AddModelError("Input.Email", "A pending invitation already exists for this email.");
            return Page();
        }

        var token = AuthNetInvitationToken.Generate();
        var invitation = new AuthNetInvitation
        {
            Email = email,
            NormalizedEmail = normalizedEmail,
            TokenHash = AuthNetInvitationToken.Hash(token),
            CreatedAtUtc = now,
            ExpiresAtUtc = now.Add(authNetOptions.Invitations.Expiration),
            CreatedByUserId = userManager.GetUserId(User)
        };

        dbContext.Invitations.Add(invitation);
        await dbContext.SaveChangesAsync(HttpContext.RequestAborted);

        var acceptUrl = AccountEmailMessages.BuildAcceptInvitationUrl(this, token);
        await emailSender.SendAsync(
            AccountEmailMessages.CreateInvitationMessage(email, acceptUrl),
            HttpContext.RequestAborted);

        await auditWriter.RecordAsync(
            User,
            "InvitationCreated",
            targetEmail: email,
            metadata: $"InvitationId={invitation.Id};ExpiresAtUtc={invitation.ExpiresAtUtc:O}",
            cancellationToken: HttpContext.RequestAborted);

        return RedirectToPage("./Index");
    }
}

public sealed class InvitationInput
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
