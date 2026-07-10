using System.Net;
using System.Net.Http.Json;
using AuthNet.Api;
using AuthNet.Persistence.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.Tests.Integration;

public sealed class AuthNetSpaInvitationApiTests
{
    [Fact]
    public async Task Invitation_status_reports_valid_pending_invitation()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var token = await CreateStoredInvitationAsync(host, "status.invite@example.test");

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/auth/api/invitations/accept?token={token}"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetInvitationAcceptanceStatusResponse>();
        Assert.NotNull(result);
        Assert.True(result.Result.Succeeded);
        Assert.Equal("valid", result.Status);
        Assert.Equal("status.invite@example.test", result.Email);
        Assert.NotNull(result.ExpiresAtUtc);
    }

    [Fact]
    public async Task Invitation_status_rejects_invalid_expired_accepted_and_existing_user_states()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var expiredToken = await CreateStoredInvitationAsync(
            host,
            "expired.api.invite@example.test",
            DateTimeOffset.UtcNow.AddDays(-2),
            DateTimeOffset.UtcNow.AddDays(-1));
        var acceptedToken = await CreateStoredInvitationAsync(
            host,
            "accepted.api.invite@example.test",
            acceptedByUserId: "accepted-user");
        var existingUserToken = await CreateStoredInvitationAsync(host, "existing.api.invite@example.test");
        await host.CreateUserAsync("existing.api.invite@example.test");

        await AssertInvitationStatusAsync(host, "not-real", "invalidToken", "InvalidToken");
        await AssertInvitationStatusAsync(host, expiredToken, "expired", "ExpiredInvitation");
        await AssertInvitationStatusAsync(host, acceptedToken, "alreadyAccepted", "AlreadyAccepted");
        await AssertInvitationStatusAsync(host, existingUserToken, "existingUser", "ExistingUser");
    }

    [Fact]
    public async Task Invitation_acceptance_creates_user_confirms_email_marks_invitation_and_signs_in()
    {
        await using var host = await AuthNetTestHost.CreateAsync(options =>
            options.EnablePublicRegistration = false);
        var token = await CreateStoredInvitationAsync(host, "accept.api.invite@example.test");

        var response = await host.PostJsonAsync("/auth/api/invitations/accept", new AuthNetAcceptInvitationRequest
        {
            Token = token,
            UserName = "accept.api.invite",
            DisplayName = "Accepted API Invite",
            Password = "Password1!",
            ConfirmPassword = "Password1!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetAcceptInvitationResponse>();
        Assert.NotNull(result);
        Assert.True(result.Result.Succeeded);
        Assert.Equal("accepted", result.Status);
        Assert.Equal("accept.api.invite@example.test", result.Email);
        Assert.False(string.IsNullOrWhiteSpace(result.UserId));

        var sessionResponse = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/api/session"));
        var session = await sessionResponse.Content.ReadFromJsonAsync<AuthNetSessionResponse>();
        Assert.Equal(HttpStatusCode.OK, sessionResponse.StatusCode);
        Assert.NotNull(session);
        Assert.True(session.IsAuthenticated);
        Assert.Equal("accept.api.invite@example.test", session.Email);

        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByEmailAsync("accept.api.invite@example.test");
        Assert.NotNull(user);
        Assert.Equal("accept.api.invite", user.UserName);
        Assert.Equal("Accepted API Invite", user.DisplayName);
        Assert.True(await userManager.IsEmailConfirmedAsync(user));

        var dbContext = scope.ServiceProvider.GetRequiredService<AuthNetDbContext>();
        var invitation = await dbContext.Invitations.SingleAsync();
        Assert.NotNull(invitation.AcceptedAtUtc);
        Assert.Equal(user.Id, invitation.AcceptedByUserId);
    }

    [Fact]
    public async Task Accepted_invitation_cannot_be_reused_through_api()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var token = await CreateStoredInvitationAsync(host, "reuse.api.invite@example.test");

        var firstResponse = await host.PostJsonAsync("/auth/api/invitations/accept", new AuthNetAcceptInvitationRequest
        {
            Token = token,
            UserName = "reuse.api.invite",
            Password = "Password1!",
            ConfirmPassword = "Password1!"
        });
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        host.ClearCookies();
        var secondResponse = await host.PostJsonAsync("/auth/api/invitations/accept", new AuthNetAcceptInvitationRequest
        {
            Token = token,
            UserName = "reuse.api.invite.second",
            Password = "Password1!",
            ConfirmPassword = "Password1!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, secondResponse.StatusCode);
        var result = await secondResponse.Content.ReadFromJsonAsync<AuthNetAcceptInvitationResponse>();
        Assert.NotNull(result);
        Assert.False(result.Result.Succeeded);
        Assert.Equal("alreadyAccepted", result.Status);
        Assert.Contains(result.Result.Errors, error => error.Code == "AlreadyAccepted");
    }

    [Fact]
    public async Task Invitation_acceptance_reports_validation_and_token_state_failures()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var token = await CreateStoredInvitationAsync(host, "validation.api.invite@example.test");
        var expiredToken = await CreateStoredInvitationAsync(
            host,
            "expired.accept.api.invite@example.test",
            DateTimeOffset.UtcNow.AddDays(-2),
            DateTimeOffset.UtcNow.AddDays(-1));

        var weakPasswordResponse = await host.PostJsonAsync("/auth/api/invitations/accept", new AuthNetAcceptInvitationRequest
        {
            Token = token,
            UserName = "validation.api.invite",
            Password = "weak",
            ConfirmPassword = "weak"
        });
        await AssertAcceptFailureAsync(weakPasswordResponse, "validationFailed", code => code.StartsWith("Password", StringComparison.Ordinal));

        var mismatchResponse = await host.PostJsonAsync("/auth/api/invitations/accept", new AuthNetAcceptInvitationRequest
        {
            Token = token,
            UserName = "validation.api.invite",
            Password = "Password1!",
            ConfirmPassword = "Password2!"
        });
        await AssertAcceptFailureAsync(mismatchResponse, "validationFailed", code => code == "Validation");

        var invalidTokenResponse = await host.PostJsonAsync("/auth/api/invitations/accept", new AuthNetAcceptInvitationRequest
        {
            Token = "not-real",
            UserName = "invalid.api.invite",
            Password = "Password1!",
            ConfirmPassword = "Password1!"
        });
        await AssertAcceptFailureAsync(invalidTokenResponse, "invalidToken", code => code == "InvalidToken");

        var expiredResponse = await host.PostJsonAsync("/auth/api/invitations/accept", new AuthNetAcceptInvitationRequest
        {
            Token = expiredToken,
            UserName = "expired.api.invite",
            Password = "Password1!",
            ConfirmPassword = "Password1!"
        });
        await AssertAcceptFailureAsync(expiredResponse, "expired", code => code == "ExpiredInvitation");
    }

    [Fact]
    public async Task Invitation_acceptance_rejects_existing_user_without_mutating_invitation()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var token = await CreateStoredInvitationAsync(host, "existing.accept.api@example.test");
        await host.CreateUserAsync("existing.accept.api@example.test");

        var response = await host.PostJsonAsync("/auth/api/invitations/accept", new AuthNetAcceptInvitationRequest
        {
            Token = token,
            UserName = "existing.accept.api",
            Password = "Password1!",
            ConfirmPassword = "Password1!"
        });

        await AssertAcceptFailureAsync(response, "existingUser", code => code == "ExistingUser");

        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthNetDbContext>();
        var invitation = await dbContext.Invitations.SingleAsync();
        Assert.Null(invitation.AcceptedAtUtc);
        Assert.Null(invitation.AcceptedByUserId);
    }

    private static async Task AssertInvitationStatusAsync(
        AuthNetTestHost host,
        string token,
        string expectedStatus,
        string expectedErrorCode)
    {
        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/auth/api/invitations/accept?token={token}"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetInvitationAcceptanceStatusResponse>();
        Assert.NotNull(result);
        Assert.False(result.Result.Succeeded);
        Assert.Equal(expectedStatus, result.Status);
        Assert.Contains(result.Result.Errors, error => error.Code == expectedErrorCode);
    }

    private static async Task AssertAcceptFailureAsync(
        HttpResponseMessage response,
        string expectedStatus,
        Func<string, bool> matchesErrorCode)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetAcceptInvitationResponse>();
        Assert.NotNull(result);
        Assert.False(result.Result.Succeeded);
        Assert.Equal(expectedStatus, result.Status);
        Assert.Contains(result.Result.Errors, error => matchesErrorCode(error.Code));
    }

    private static async Task<string> CreateStoredInvitationAsync(
        AuthNetTestHost host,
        string email,
        DateTimeOffset? createdAtUtc = null,
        DateTimeOffset? expiresAtUtc = null,
        string? acceptedByUserId = null)
    {
        var token = AuthNetInvitationToken.Generate();
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthNetDbContext>();
        var now = DateTimeOffset.UtcNow;
        dbContext.Invitations.Add(new AuthNetInvitation
        {
            Email = email,
            NormalizedEmail = userManager.NormalizeEmail(email),
            TokenHash = AuthNetInvitationToken.Hash(token),
            CreatedAtUtc = createdAtUtc ?? now,
            ExpiresAtUtc = expiresAtUtc ?? now.AddDays(7),
            AcceptedAtUtc = acceptedByUserId is null ? null : now,
            AcceptedByUserId = acceptedByUserId
        });
        await dbContext.SaveChangesAsync();
        return token;
    }
}
