using System.Net;
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.Tests.Integration;

public sealed class AuthNetExternalLoginTests
{
    [Fact]
    public async Task Existing_local_account_is_not_linked_by_email_alone()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("existing.external@example.test");
        await host.SetExternalLoginAsync("existing.external@example.test", "provider-existing", verified: true);

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/external-login?handler=Callback"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("An account already exists for this email address", body);
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        Assert.Empty(await userManager.GetLoginsAsync(user));
    }

    [Fact]
    public async Task New_external_account_requires_verified_email()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.SetExternalLoginAsync("unverified.external@example.test", "provider-unverified", verified: false);

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/external-login?handler=Callback"));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("verified email address", body);
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        Assert.Null(await userManager.FindByEmailAsync("unverified.external@example.test"));
    }

    [Fact]
    public async Task Verified_external_account_is_provisioned_and_linked()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.SetExternalLoginAsync("verified.external@example.test", "provider-verified", verified: true);

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/external-login?handler=Callback"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByEmailAsync("verified.external@example.test");
        Assert.NotNull(user);
        Assert.True(user.EmailConfirmed);
        var login = Assert.Single(await userManager.GetLoginsAsync(user));
        Assert.Equal("TestProvider", login.LoginProvider);
        Assert.Equal("provider-verified", login.ProviderKey);
    }

    [Fact]
    public async Task Authenticated_user_can_link_external_provider()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("link.external@example.test");
        await host.SignInAsync("link.external@example.test");
        await host.SetExternalLoginAsync("different.external@example.test", "provider-link", verified: true);

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/external-login?handler=Callback&returnUrl=/auth/profile"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/auth/profile", response.Headers.Location?.OriginalString);
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var login = Assert.Single(await userManager.GetLoginsAsync(user));
        Assert.Equal("TestProvider", login.LoginProvider);
        Assert.Equal("provider-link", login.ProviderKey);
    }
}
