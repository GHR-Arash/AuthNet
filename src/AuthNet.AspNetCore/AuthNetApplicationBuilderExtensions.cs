using AuthNet.Api;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;

namespace AuthNet.AspNetCore;

public static class AuthNetApplicationBuilderExtensions
{
    public static IEndpointConventionBuilder MapAuthNet(this IEndpointRouteBuilder endpoints)
    {
        endpoints.ServiceProvider.GetRequiredService<AuthNetConfigurationValidator>().Validate();
        endpoints.MapAuthNetApi();
        return endpoints.MapRazorPages();
    }

    [Obsolete("Use MapAuthNet() after UseAuthentication() and UseAuthorization().")]
    public static WebApplication UseAuthNet(this WebApplication app)
    {
        app.MapAuthNet();
        return app;
    }
}
