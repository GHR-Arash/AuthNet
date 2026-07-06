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
    public async Task Authenticated_account_routes_challenge_anonymous_users(string route)
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var response = await host.Client.GetAsync(route);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.EndsWith("/auth/login?ReturnUrl=%2Fauth%2F" + route.Split('/').Last(), response.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Host_owned_routes_continue_to_work_when_authnet_is_mapped()
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var response = await host.Client.GetAsync("/host-owned");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("host-owned", await response.Content.ReadAsStringAsync());
    }
}
