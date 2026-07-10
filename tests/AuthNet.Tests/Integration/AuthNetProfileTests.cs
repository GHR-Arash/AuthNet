using System.Net;
using AuthNet.Persistence.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.Tests.Integration;

public sealed class AuthNetProfileTests
{
    [Fact]
    public async Task Authenticated_user_can_view_profile()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("profile@example.test");
        await host.SignInAsync("profile@example.test");

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/profile"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Authenticated_user_can_update_profile_fields()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("profile.update@example.test");
        await host.SignInAsync("profile.update@example.test");
        var form = await host.GetFormAsync("/auth/profile");

        var response = await host.PostFormAsync("/auth/profile", form,
            ("Input.Email", "profile.update@example.test"),
            ("Input.DisplayName", "Updated User"),
            ("Input.PhoneNumber", "+15555550100"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByEmailAsync("profile.update@example.test");
        Assert.NotNull(user);
        Assert.Equal("Updated User", user.DisplayName);
        Assert.Equal("+15555550100", user.PhoneNumber);
    }

    [Fact]
    public async Task Email_change_sends_confirmation_and_does_not_immediately_mutate_user()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("old.email@example.test");
        await host.SignInAsync("old.email@example.test");
        var form = await host.GetFormAsync("/auth/profile");

        var response = await host.PostFormAsync("/auth/profile", form,
            ("Input.Email", "new.email@example.test"),
            ("Input.DisplayName", "Email Change"),
            ("Input.PhoneNumber", "+15555550101"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var message = Assert.Single(host.EmailSink.Messages);
        Assert.Equal("new.email@example.test", message.To);
        Assert.Equal("Confirm your new email", message.Subject);

        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        Assert.NotNull(await userManager.FindByEmailAsync("old.email@example.test"));
        Assert.Null(await userManager.FindByEmailAsync("new.email@example.test"));
    }

    [Fact]
    public async Task Email_change_confirmation_updates_email_and_username()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("before.change@example.test");
        await host.SignInAsync("before.change@example.test");
        var form = await host.GetFormAsync("/auth/profile");

        await host.PostFormAsync("/auth/profile", form,
            ("Input.Email", "after.change@example.test"),
            ("Input.DisplayName", "Email Change"),
            ("Input.PhoneNumber", "+15555550102"));

        var message = Assert.Single(host.EmailSink.Messages);
        var confirmationUrl = AuthNetTestHtml.GetFirstLinkContaining(message.HtmlBody, "clicking here");
        Assert.NotNull(confirmationUrl);

        var response = await host.Client.GetAsync(confirmationUrl);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByEmailAsync("after.change@example.test");
        Assert.NotNull(user);
        Assert.Equal("after.change@example.test", user.UserName);
    }
}
