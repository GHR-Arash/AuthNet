using System.Net;
using System.Net.Http.Json;
using AuthNet.Api;
using AuthNet.Persistence.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.Tests.Integration;

public sealed class AuthNetSpaAccountWorkflowTests
{
    [Fact]
    public async Task Confirm_email_completes_from_api_confirmation_link()
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var registerResponse = await host.PostJsonAsync("/auth/api/register", new AuthNetRegisterRequest
        {
            Email = "confirm.workflow@example.test",
            Password = "Password1!"
        });
        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        var message = Assert.Single(host.EmailSink.Messages);
        var query = GetFirstEmailLinkQuery(message.HtmlBody);

        var response = await host.PostJsonAsync("/auth/api/confirm-email", new AuthNetConfirmEmailRequest
        {
            UserId = query["userId"].ToString(),
            Code = query["code"].ToString()
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(result);
        Assert.True(result.Succeeded);

        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByEmailAsync("confirm.workflow@example.test");
        Assert.NotNull(user);
        Assert.True(await userManager.IsEmailConfirmedAsync(user));
    }

    [Fact]
    public async Task Confirm_email_rejects_invalid_token()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("invalid.confirm@example.test", emailConfirmed: false);

        var response = await host.PostJsonAsync("/auth/api/confirm-email", new AuthNetConfirmEmailRequest
        {
            UserId = user.Id,
            Code = "not-a-valid-code"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == "InvalidToken");
    }

    [Fact]
    public async Task Reset_password_completes_from_api_reset_link()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("reset.workflow@example.test");

        var forgotResponse = await host.PostJsonAsync("/auth/api/forgot-password", new AuthNetForgotPasswordRequest
        {
            Email = "reset.workflow@example.test"
        });
        Assert.Equal(HttpStatusCode.OK, forgotResponse.StatusCode);

        var message = Assert.Single(host.EmailSink.Messages);
        var query = GetFirstEmailLinkQuery(message.HtmlBody);

        var response = await host.PostJsonAsync("/auth/api/reset-password", new AuthNetResetPasswordRequest
        {
            Email = "reset.workflow@example.test",
            Code = query["code"].ToString(),
            Password = "NewPassword1!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(result);
        Assert.True(result.Succeeded);

        var oldPasswordLogin = await host.PostJsonAsync("/auth/api/login", new AuthNetLoginRequest
        {
            Identifier = "reset.workflow@example.test",
            Password = "Password1!"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, oldPasswordLogin.StatusCode);

        var newPasswordLogin = await host.PostJsonAsync("/auth/api/login", new AuthNetLoginRequest
        {
            Identifier = "reset.workflow@example.test",
            Password = "NewPassword1!"
        });
        Assert.Equal(HttpStatusCode.OK, newPasswordLogin.StatusCode);
    }

    [Fact]
    public async Task Reset_password_reports_invalid_or_weak_password_errors()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("weak.reset@example.test");

        var invalidTokenResponse = await host.PostJsonAsync("/auth/api/reset-password", new AuthNetResetPasswordRequest
        {
            Email = "weak.reset@example.test",
            Code = "not-a-valid-code",
            Password = "NewPassword1!"
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidTokenResponse.StatusCode);

        var forgotResponse = await host.PostJsonAsync("/auth/api/forgot-password", new AuthNetForgotPasswordRequest
        {
            Email = "weak.reset@example.test"
        });
        Assert.Equal(HttpStatusCode.OK, forgotResponse.StatusCode);
        var query = GetFirstEmailLinkQuery(Assert.Single(host.EmailSink.Messages).HtmlBody);

        var weakPasswordResponse = await host.PostJsonAsync("/auth/api/reset-password", new AuthNetResetPasswordRequest
        {
            Email = "weak.reset@example.test",
            Code = query["code"].ToString(),
            Password = "weak"
        });

        Assert.Equal(HttpStatusCode.BadRequest, weakPasswordResponse.StatusCode);
        var result = await weakPasswordResponse.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code.StartsWith("Password", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Update_profile_requires_authentication_and_returns_updated_profile()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("profile.workflow@example.test", displayName: "Original Name");

        var anonymousResponse = await PutJsonAsync(host, "/auth/api/profile", new AuthNetUpdateProfileRequest
        {
            DisplayName = "Anonymous"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        var loginResponse = await host.PostJsonAsync("/auth/api/login", new AuthNetLoginRequest
        {
            Identifier = "profile.workflow@example.test",
            Password = "Password1!"
        });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var response = await PutJsonAsync(host, "/auth/api/profile", new AuthNetUpdateProfileRequest
        {
            DisplayName = "Updated Name",
            PhoneNumber = "+15551234567"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<AuthNetProfileResponse>();
        Assert.NotNull(profile);
        Assert.Equal("Updated Name", profile.DisplayName);
        Assert.Equal("+15551234567", profile.PhoneNumber);

        var getProfileResponse = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/api/profile"));
        Assert.Equal(HttpStatusCode.OK, getProfileResponse.StatusCode);
        var loadedProfile = await getProfileResponse.Content.ReadFromJsonAsync<AuthNetProfileResponse>();
        Assert.NotNull(loadedProfile);
        Assert.Equal("Updated Name", loadedProfile.DisplayName);
        Assert.Equal("+15551234567", loadedProfile.PhoneNumber);
    }

    [Fact]
    public async Task Change_password_requires_authentication_and_updates_local_password()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("change.workflow@example.test");

        var anonymousResponse = await host.PostJsonAsync("/auth/api/change-password", new AuthNetChangePasswordRequest
        {
            CurrentPassword = "Password1!",
            NewPassword = "NewPassword1!"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        var loginResponse = await host.PostJsonAsync("/auth/api/login", new AuthNetLoginRequest
        {
            Identifier = "change.workflow@example.test",
            Password = "Password1!"
        });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        var response = await host.PostJsonAsync("/auth/api/change-password", new AuthNetChangePasswordRequest
        {
            CurrentPassword = "Password1!",
            NewPassword = "NewPassword1!"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(result);
        Assert.True(result.Succeeded);

        host.ClearCookies();
        var oldPasswordLogin = await host.PostJsonAsync("/auth/api/login", new AuthNetLoginRequest
        {
            Identifier = "change.workflow@example.test",
            Password = "Password1!"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, oldPasswordLogin.StatusCode);

        var newPasswordLogin = await host.PostJsonAsync("/auth/api/login", new AuthNetLoginRequest
        {
            Identifier = "change.workflow@example.test",
            Password = "NewPassword1!"
        });
        Assert.Equal(HttpStatusCode.OK, newPasswordLogin.StatusCode);
    }

    [Fact]
    public async Task Change_password_reports_wrong_current_password()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("wrong.current@example.test");
        await host.PostJsonAsync("/auth/api/login", new AuthNetLoginRequest
        {
            Identifier = "wrong.current@example.test",
            Password = "Password1!"
        });

        var response = await host.PostJsonAsync("/auth/api/change-password", new AuthNetChangePasswordRequest
        {
            CurrentPassword = "wrong-password",
            NewPassword = "NewPassword1!"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == "PasswordMismatch");
    }

    [Fact]
    public async Task Change_password_reports_weak_new_password()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("weak.change@example.test");
        await host.PostJsonAsync("/auth/api/login", new AuthNetLoginRequest
        {
            Identifier = "weak.change@example.test",
            Password = "Password1!"
        });

        var response = await host.PostJsonAsync("/auth/api/change-password", new AuthNetChangePasswordRequest
        {
            CurrentPassword = "Password1!",
            NewPassword = "weak"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code.StartsWith("Password", StringComparison.Ordinal));
    }

    private static async Task<HttpResponseMessage> PutJsonAsync<TRequest>(
        AuthNetTestHost host,
        string path,
        TRequest body)
    {
        return await host.SendAsync(new HttpRequestMessage(HttpMethod.Put, path)
        {
            Content = JsonContent.Create(body)
        });
    }

    private static IDictionary<string, Microsoft.Extensions.Primitives.StringValues> GetFirstEmailLinkQuery(string html)
    {
        var link = AuthNetTestHtml.GetFirstLinkContaining(html, "clicking here");
        Assert.NotNull(link);
        return QueryHelpers.ParseQuery(new Uri(link).Query);
    }
}
