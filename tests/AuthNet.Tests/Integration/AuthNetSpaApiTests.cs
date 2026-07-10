using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AuthNet.Api;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.Tests.Integration;

public sealed class AuthNetSpaApiTests
{
    [Fact]
    public async Task Session_returns_anonymous_state_before_login()
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var session = await host.Client.GetFromJsonAsync<AuthNetSessionResponse>("/auth/api/session");

        Assert.NotNull(session);
        Assert.False(session.IsAuthenticated);
        Assert.Null(session.Email);
        Assert.Empty(session.Roles);
    }

    [Fact]
    public async Task Login_session_profile_and_logout_work_with_cookie_session()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("spa.user@example.test", displayName: "SPA User");
        await host.AddRoleAsync("Editor");
        await host.AddUserToRoleAsync(user.Id, "Editor");

        var loginResponse = await host.PostJsonAsync("/auth/api/login", new AuthNetLoginRequest
        {
            Identifier = "spa.user@example.test",
            Password = "Password1!",
            RememberMe = false
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var login = await loginResponse.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(login);
        Assert.True(login.Succeeded);

        var sessionResponse = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/api/session"));
        var session = await sessionResponse.Content.ReadFromJsonAsync<AuthNetSessionResponse>();
        Assert.NotNull(session);
        Assert.True(session.IsAuthenticated);
        Assert.Equal("spa.user@example.test", session.Email);
        Assert.Equal("SPA User", session.DisplayName);
        Assert.Contains("Editor", session.Roles);

        var profileResponse = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/api/profile"));
        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);
        var profile = await profileResponse.Content.ReadFromJsonAsync<AuthNetProfileResponse>();
        Assert.NotNull(profile);
        Assert.Equal("spa.user@example.test", profile.Email);
        Assert.Equal("SPA User", profile.DisplayName);
        Assert.True(profile.EmailConfirmed);
        Assert.Contains("Editor", profile.Roles);

        var logoutResponse = await host.PostJsonAsync("/auth/api/logout", new { });
        Assert.Equal(HttpStatusCode.OK, logoutResponse.StatusCode);

        var signedOutSessionResponse = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/api/session"));
        var signedOutSession = await signedOutSessionResponse.Content.ReadFromJsonAsync<AuthNetSessionResponse>();
        Assert.NotNull(signedOutSession);
        Assert.False(signedOutSession.IsAuthenticated);
    }

    [Fact]
    public async Task Invalid_login_returns_json_error()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("invalid.spa@example.test");

        var response = await host.PostJsonAsync("/auth/api/login", new AuthNetLoginRequest
        {
            Identifier = "invalid.spa@example.test",
            Password = "wrong-password"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == "InvalidCredentials");
    }

    [Fact]
    public async Task Login_for_unconfirmed_email_returns_not_allowed_error()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("unconfirmed.spa@example.test", emailConfirmed: false);

        var response = await host.PostJsonAsync("/auth/api/login", new AuthNetLoginRequest
        {
            Identifier = "unconfirmed.spa@example.test",
            Password = "Password1!"
        });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == "NotAllowed");
    }

    [Fact]
    public async Task Profile_requires_authenticated_cookie_session()
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var response = await host.Client.GetAsync("/auth/api/profile");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Register_creates_user_and_sends_confirmation_email()
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var response = await host.PostJsonAsync("/auth/api/register", new AuthNetRegisterRequest
        {
            Email = "registered.spa@example.test",
            DisplayName = "Registered SPA",
            Password = "Password1!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(result);
        Assert.True(result.Succeeded);

        var message = Assert.Single(host.EmailSink.Messages);
        Assert.Equal("registered.spa@example.test", message.To);
        Assert.Equal("Confirm your email", message.Subject);
        Assert.Contains("/auth/confirm-email", message.HtmlBody);

        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNet.Persistence.EntityFrameworkCore.AuthNetUser>>();
        var user = await userManager.FindByEmailAsync("registered.spa@example.test");
        Assert.NotNull(user);
        Assert.Equal("Registered SPA", user.DisplayName);
    }

    [Fact]
    public async Task Register_reports_disabled_public_registration()
    {
        await using var host = await AuthNetTestHost.CreateAsync(options =>
            options.EnablePublicRegistration = false);

        var response = await host.PostJsonAsync("/auth/api/register", new AuthNetRegisterRequest
        {
            Email = "closed@example.test",
            Password = "Password1!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == "RegistrationDisabled");
    }

    [Fact]
    public async Task Register_reports_duplicate_email_errors()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("duplicate.spa@example.test");

        var response = await host.PostJsonAsync("/auth/api/register", new AuthNetRegisterRequest
        {
            Email = "duplicate.spa@example.test",
            Password = "Password1!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code is "DuplicateUserName" or "DuplicateEmail");
    }

    [Fact]
    public async Task Forgot_password_does_not_enumerate_accounts()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("known.reset@example.test");

        var knownResponse = await host.PostJsonAsync("/auth/api/forgot-password", new AuthNetForgotPasswordRequest
        {
            Email = "known.reset@example.test"
        });
        var unknownResponse = await host.PostJsonAsync("/auth/api/forgot-password", new AuthNetForgotPasswordRequest
        {
            Email = "unknown.reset@example.test"
        });

        Assert.Equal(HttpStatusCode.OK, knownResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, unknownResponse.StatusCode);
        var known = await knownResponse.Content.ReadFromJsonAsync<AuthNetApiResult>();
        var unknown = await unknownResponse.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(known);
        Assert.NotNull(unknown);
        Assert.True(known.Succeeded);
        Assert.True(unknown.Succeeded);
        var message = Assert.Single(host.EmailSink.Messages);
        Assert.Equal("known.reset@example.test", message.To);
        Assert.Equal("Reset your password", message.Subject);
        Assert.Contains("/auth/reset-password", message.HtmlBody);
    }

    [Fact]
    public async Task Resend_confirmation_sends_only_for_unconfirmed_accounts()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("needs.confirmation@example.test", emailConfirmed: false);
        await host.CreateUserAsync("already.confirmed@example.test", emailConfirmed: true);

        var unconfirmedResponse = await host.PostJsonAsync("/auth/api/resend-confirmation", new AuthNetResendConfirmationRequest
        {
            Email = "needs.confirmation@example.test"
        });
        var confirmedResponse = await host.PostJsonAsync("/auth/api/resend-confirmation", new AuthNetResendConfirmationRequest
        {
            Email = "already.confirmed@example.test"
        });

        Assert.Equal(HttpStatusCode.OK, unconfirmedResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, confirmedResponse.StatusCode);
        var message = Assert.Single(host.EmailSink.Messages);
        Assert.Equal("needs.confirmation@example.test", message.To);
        Assert.Equal("Confirm your email", message.Subject);
    }

    [Fact]
    public async Task Validation_errors_use_camel_case_fields()
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var response = await host.PostJsonAsync("/auth/api/login", new AuthNetLoginRequest
        {
            Identifier = string.Empty,
            Password = string.Empty
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);
        var fields = document.RootElement
            .GetProperty("errors")
            .EnumerateArray()
            .Select(error => error.GetProperty("field").GetString())
            .ToArray();
        Assert.Contains("identifier", fields);
        Assert.Contains("password", fields);
    }
}
