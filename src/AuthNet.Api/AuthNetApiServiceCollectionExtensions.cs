using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AuthNet.Api;

public static class AuthNetApiServiceCollectionExtensions
{
    public static IServiceCollection AddAuthNetApi(this IServiceCollection services)
    {
        services.TryAddScoped<IAuthNetSpaAccountService, AuthNetSpaAccountService>();
        return services;
    }
}
