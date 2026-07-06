using System.Net;

namespace AuthNet.Tests.Integration;

public sealed class AuthNetEndpointMappingTests
{
    [Fact]
    public async Task Legacy_use_authnet_wrapper_still_maps_account_routes()
    {
        await using var host = await AuthNetTestHost.CreateAsync(useLegacyUseAuthNet: true);

        var response = await host.Client.GetAsync("/auth/login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
