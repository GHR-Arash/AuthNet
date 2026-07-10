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

    [Obsolete("Use UseAuthNet(Action<AuthNetStartupBuilder>) for startup tasks or MapAuthNet() for endpoint-only mapping.")]
    public static WebApplication UseAuthNet(this WebApplication app)
    {
        app.MapAuthNet();
        return app;
    }

    public static async Task<WebApplication> UseAuthNet(
        this WebApplication app,
        Action<AuthNetStartupBuilder> configure)
    {
        var builder = new AuthNetStartupBuilder();
        configure(builder);

        app.Services.GetRequiredService<AuthNetConfigurationValidator>().Validate();
        await app.Services.GetRequiredService<AuthNetStartupRunner>().RunAsync(builder.Options);
        app.MapAuthNet();

        return app;
    }
}
