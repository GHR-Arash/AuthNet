using System.ComponentModel.DataAnnotations;
using AuthNet.Persistence.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace AuthNetRazor.Areas.AuthNet.Pages.Account;

public sealed class AcceptInvitationModel(
    AuthNetDbContext dbContext,
    UserManager<AuthNetUser> userManager,
    SignInManager<AuthNetUser> signInManager) : PageModel
{
    [BindProperty]
    public AcceptInvitationInput Input { get; set; } = new();

    public string InvitedEmail { get; private set; } = string.Empty;

    public bool InvalidInvitation { get; private set; }

    public async Task<IActionResult> OnGetAsync(string? token)
    {
        var invitation = await FindUsableInvitationAsync(token);
        if (invitation is null)
        {
            InvalidInvitation = true;
            return Page();
        }

        Input.Token = token ?? string.Empty;
        InvitedEmail = invitation.Email;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var invitation = await FindUsableInvitationAsync(Input.Token);
        if (invitation is null)
        {
            InvalidInvitation = true;
            return Page();
        }

        InvitedEmail = invitation.Email;
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (await userManager.FindByEmailAsync(invitation.Email) is not null)
        {
            InvalidInvitation = true;
            return Page();
        }

        var user = new AuthNetUser
        {
            UserName = Input.UserName.Trim(),
            Email = invitation.Email,
            EmailConfirmed = true,
            DisplayName = Input.DisplayName
        };

        var result = await userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }

        invitation.AcceptedAtUtc = DateTimeOffset.UtcNow;
        invitation.AcceptedByUserId = user.Id;
        await dbContext.SaveChangesAsync(HttpContext.RequestAborted);

        await signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToPage("./Profile");
    }

    private async Task<AuthNetInvitation?> FindUsableInvitationAsync(string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var tokenHash = AuthNetInvitationToken.Hash(token);
        var now = DateTimeOffset.UtcNow;
        var invitation = await dbContext.Invitations.SingleOrDefaultAsync(
            item => item.TokenHash == tokenHash,
            HttpContext.RequestAborted);

        if (invitation is null || !invitation.IsPending(now))
        {
            return null;
        }

        return invitation;
    }
}

public sealed class AcceptInvitationInput
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, MaxLength(256)]
    public string UserName { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? DisplayName { get; set; }

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required, DataType(DataType.Password), Compare(nameof(Password))]
    public string ConfirmPassword { get; set; } = string.Empty;
}
