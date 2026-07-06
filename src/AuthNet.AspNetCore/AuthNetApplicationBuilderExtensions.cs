using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.AspNetCore;

public static class AuthNetApplicationBuilderExtensions
{
    public static WebApplication UseAuthNet(this WebApplication app)
    {
        app.Services.GetRequiredService<AuthNetConfigurationValidator>().Validate();
        app.MapRazorPages();
        return app;
    }
}

