using System.Net;

namespace AuthNet.Tests.Integration;

public sealed class AuthNetRouteTests
{
    [Theory]
    [InlineData("/auth/login")]
    [InlineData("/auth/register")]
    [InlineData("/auth/forgot-password")]
    [InlineData("/auth/resend-confirmation")]
    [InlineData("/auth/confirm-email")]
    [InlineData("/auth/access-denied")]
    [InlineData("/auth/external-login")]
    [InlineData("/auth/invitations/accept")]
    public async Task Public_account_routes_render(string route)
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var response = await host.Client.GetAsync(route);

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
    }

    [Theory]
    [InlineData("/auth/profile")]
    [InlineData("/auth/change-password")]
    [InlineData("/auth/mfa")]
    [InlineData("/auth/mfa/setup")]
    [InlineData("/auth/mfa/recovery-codes")]
    [InlineData("/auth/mfa/disable")]
    public async Task Authenticated_account_routes_challenge_anonymous_users(string route)
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var response = await host.Client.GetAsync(route);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/auth/login?ReturnUrl=", response.Headers.Location?.OriginalString);
    }

    [Theory]
    [InlineData("/auth/login/mfa")]
    [InlineData("/auth/login/recovery-code")]
    public async Task Mfa_login_routes_without_pending_sign_in_redirect_to_login(string route)
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var response = await host.Client.GetAsync(route);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/auth/login", response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Host_owned_routes_continue_to_work_when_authnet_is_mapped()
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var response = await host.Client.GetAsync("/host-owned");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("host-owned", await response.Content.ReadAsStringAsync());
    }

    [Theory]
    [InlineData("/auth/admin/invitations")]
    [InlineData("/auth/admin/invitations/new")]
    public async Task Admin_invitation_routes_challenge_anonymous_users(string route)
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var response = await host.Client.GetAsync(route);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/auth/login?ReturnUrl=", response.Headers.Location?.OriginalString);
    }
}
