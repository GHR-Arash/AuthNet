using System.Net;
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.Tests.Integration;

public sealed class AuthNetInvitationTests
{
    [Fact]
    public async Task Non_admin_user_is_denied_from_admin_invitations()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("user@example.test");
        await host.SignInAsync("user@example.test");

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/admin/invitations"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/auth/access-denied", response.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task Admin_user_can_create_invitation_and_send_email()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateAdminUserAsync("admin.invite@example.test");
        await host.SignInAsync("admin.invite@example.test");
        var form = await host.GetFormAsync("/auth/admin/invitations/new");

        var response = await host.PostFormAsync("/auth/admin/invitations/new", form,
            ("Input.Email", "invited@example.test"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/auth/admin/invitations", response.Headers.Location?.OriginalString);

        var message = Assert.Single(host.EmailSink.Messages);
        Assert.Equal("invited@example.test", message.To);
        Assert.Equal("You're invited to create an account", message.Subject);
        Assert.Contains("/auth/invitations/accept", message.HtmlBody);

        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthNetDbContext>();
        var invitation = await dbContext.Invitations.SingleAsync();
        Assert.Equal("invited@example.test", invitation.Email);
        Assert.Equal("INVITED@EXAMPLE.TEST", invitation.NormalizedEmail);
        Assert.NotEmpty(invitation.TokenHash);
        Assert.DoesNotContain(invitation.TokenHash, message.HtmlBody);
        Assert.Null(invitation.AcceptedAtUtc);
    }

    [Fact]
    public async Task Admin_user_cannot_invite_existing_user()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateAdminUserAsync("admin.existing@example.test");
        await host.CreateUserAsync("existing.invite@example.test");
        await host.SignInAsync("admin.existing@example.test");
        var form = await host.GetFormAsync("/auth/admin/invitations/new");

        var response = await host.PostFormAsync("/auth/admin/invitations/new", form,
            ("Input.Email", "existing.invite@example.test"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("A user with this email already exists.", body);
        Assert.Empty(host.EmailSink.Messages);
    }

    [Fact]
    public async Task Admin_user_cannot_create_duplicate_pending_invitation()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateAdminUserAsync("admin.duplicate@example.test");
        await host.SignInAsync("admin.duplicate@example.test");

        var form = await host.GetFormAsync("/auth/admin/invitations/new");
        await host.PostFormAsync("/auth/admin/invitations/new", form,
            ("Input.Email", "duplicate.invite@example.test"));

        form = await host.GetFormAsync("/auth/admin/invitations/new");
        var response = await host.PostFormAsync("/auth/admin/invitations/new", form,
            ("Input.Email", "duplicate.invite@example.test"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("A pending invitation already exists for this email.", body);
        Assert.Single(host.EmailSink.Messages);
    }

    [Fact]
    public async Task Invited_user_can_accept_invitation_and_is_signed_in()
    {
        await using var host = await AuthNetTestHost.CreateAsync(options =>
            options.EnablePublicRegistration = false);
        await host.CreateAdminUserAsync("admin.accept@example.test");
        await host.SignInAsync("admin.accept@example.test");
        var acceptPath = await CreateInvitationAndGetAcceptPathAsync(host, "accept.invite@example.test");

        host.ClearCookies();
        var form = await host.GetFormAsync(acceptPath);
        var response = await host.PostFormAsync(acceptPath, form,
            ("Input.Token", GetTokenFromPath(acceptPath)),
            ("Input.UserName", "accept.invite"),
            ("Input.DisplayName", "Accepted Invite"),
            ("Input.Password", "Password1!"),
            ("Input.ConfirmPassword", "Password1!"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/auth/profile", response.Headers.Location?.OriginalString);

        var profileResponse = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/profile"));
        var profileBody = await profileResponse.Content.ReadAsStringAsync();
        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);
        Assert.Contains("accept.invite@example.test", profileBody);

        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByEmailAsync("accept.invite@example.test");
        Assert.NotNull(user);
        Assert.Equal("accept.invite", user.UserName);
        Assert.Equal("Accepted Invite", user.DisplayName);
        Assert.True(await userManager.IsEmailConfirmedAsync(user));

        var dbContext = scope.ServiceProvider.GetRequiredService<AuthNetDbContext>();
        var invitation = await dbContext.Invitations.SingleAsync();
        Assert.NotNull(invitation.AcceptedAtUtc);
        Assert.Equal(user.Id, invitation.AcceptedByUserId);
    }

    [Fact]
    public async Task Accepted_invitation_cannot_be_reused()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateAdminUserAsync("admin.reuse@example.test");
        await host.SignInAsync("admin.reuse@example.test");
        var acceptPath = await CreateInvitationAndGetAcceptPathAsync(host, "reuse.invite@example.test");

        host.ClearCookies();
        var form = await host.GetFormAsync(acceptPath);
        await host.PostFormAsync(acceptPath, form,
            ("Input.Token", GetTokenFromPath(acceptPath)),
            ("Input.UserName", "reuse.invite"),
            ("Input.DisplayName", ""),
            ("Input.Password", "Password1!"),
            ("Input.ConfirmPassword", "Password1!"));

        host.ClearCookies();
        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, acceptPath));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("This invitation is invalid or no longer available.", body);
    }

    [Fact]
    public async Task Expired_invitation_cannot_be_accepted()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var token = await CreateStoredInvitationAsync(
            host,
            "expired.invite@example.test",
            DateTimeOffset.UtcNow.AddDays(-2),
            DateTimeOffset.UtcNow.AddDays(-1));

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/auth/invitations/accept?token={token}"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("This invitation is invalid or no longer available.", body);
    }

    [Fact]
    public async Task Invalid_invitation_token_fails_safely()
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/invitations/accept?token=not-real"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("This invitation is invalid or no longer available.", body);
    }

    private static async Task<string> CreateInvitationAndGetAcceptPathAsync(AuthNetTestHost host, string email)
    {
        var form = await host.GetFormAsync("/auth/admin/invitations/new");
        var response = await host.PostFormAsync("/auth/admin/invitations/new", form,
            ("Input.Email", email));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        var message = host.EmailSink.Messages.Last();
        var acceptUrl = AuthNetTestHtml.GetFirstLinkContaining(message.HtmlBody, "clicking here");
        Assert.NotNull(acceptUrl);
        return new Uri(acceptUrl).PathAndQuery;
    }

    private static async Task<string> CreateStoredInvitationAsync(
        AuthNetTestHost host,
        string email,
        DateTimeOffset createdAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        var token = AuthNetInvitationToken.Generate();
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthNetDbContext>();
        dbContext.Invitations.Add(new AuthNetInvitation
        {
            Email = email,
            NormalizedEmail = userManager.NormalizeEmail(email),
            TokenHash = AuthNetInvitationToken.Hash(token),
            CreatedAtUtc = createdAtUtc,
            ExpiresAtUtc = expiresAtUtc
        });
        await dbContext.SaveChangesAsync();
        return token;
    }

    private static string GetTokenFromPath(string path)
    {
        var query = new Uri("https://localhost" + path).Query.TrimStart('?');
        foreach (var segment in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = segment.Split('=', 2);
            if (parts.Length == 2 && parts[0] == "token")
            {
                return Uri.UnescapeDataString(parts[1]);
            }
        }

        throw new InvalidOperationException("Invitation path did not contain a token.");
    }
}
