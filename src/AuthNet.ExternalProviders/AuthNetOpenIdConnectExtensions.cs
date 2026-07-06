using AuthNet.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace AuthNet.ExternalProviders;

public static class AuthNetOpenIdConnectExtensions
{
    public static AuthenticationBuilder AddAuthNetOpenIdConnect(
        this AuthenticationBuilder builder,
        AuthNetOpenIdConnectOptions options)
    {
        if (!options.Enabled)
        {
            return builder;
        }

        if (string.IsNullOrWhiteSpace(options.Authority))
        {
            throw new AuthNetConfigurationException("AuthNet OpenID Connect is enabled but Authority is not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.ClientId))
        {
            throw new AuthNetConfigurationException("AuthNet OpenID Connect is enabled but ClientId is not configured.");
        }

        return builder.AddOpenIdConnect(options.Scheme, options.DisplayName, oidc =>
        {
            oidc.Authority = options.Authority;
            oidc.ClientId = options.ClientId;
            oidc.ClientSecret = options.ClientSecret;
            oidc.CallbackPath = options.CallbackPath;
            oidc.ResponseType = OpenIdConnectResponseType.Code;
            oidc.SaveTokens = true;
            oidc.GetClaimsFromUserInfoEndpoint = true;
            oidc.Scope.Add("email");
            oidc.Scope.Add("profile");
        });
    }
}
