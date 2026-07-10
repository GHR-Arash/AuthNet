using System.Net;
using AuthNet.Persistence.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.Tests.Integration;

public sealed class AuthNetRegistrationTests
{
    [Fact]
    public async Task Registration_is_not_available_when_public_registration_is_disabled()
    {
        await using var host = await AuthNetTestHost.CreateAsync(options =>
            options.EnablePublicRegistration = false);

        var response = await host.Client.GetAsync("/auth/register");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Enabled_registration_sends_confirmation_email()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var form = await host.GetFormAsync("/auth/register");

        var response = await host.PostFormAsync("/auth/register", form,
            ("Input.Email", "new.user@example.test"),
            ("Input.DisplayName", "New User"),
            ("Input.Password", "Password1!"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var message = Assert.Single(host.EmailSink.Messages);
        Assert.Equal("new.user@example.test", message.To);
        Assert.Equal("Confirm your email", message.Subject);
        Assert.Contains("/auth/confirm-email", message.HtmlBody);
    }

    [Fact]
    public async Task Confirmation_link_confirms_registered_user()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var form = await host.GetFormAsync("/auth/register");

        await host.PostFormAsync("/auth/register", form,
            ("Input.Email", "confirm.me@example.test"),
            ("Input.DisplayName", "Confirm Me"),
            ("Input.Password", "Password1!"));

        var message = Assert.Single(host.EmailSink.Messages);
        var confirmationUrl = AuthNetTestHtml.GetFirstLinkContaining(message.HtmlBody, "clicking here");
        Assert.NotNull(confirmationUrl);

        var confirmResponse = await host.Client.GetAsync(confirmationUrl);

        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByEmailAsync("confirm.me@example.test");
        Assert.NotNull(user);
        Assert.True(await userManager.IsEmailConfirmedAsync(user));
    }

    [Fact]
    public async Task Invalid_confirmation_token_fails_safely()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("invalid.confirm@example.test", emailConfirmed: false);

        var response = await host.Client.GetAsync($"/auth/confirm-email?userId={user.Id}&code=not-a-token");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Email confirmation failed.", body);
    }
}
