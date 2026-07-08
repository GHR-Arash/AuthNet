using System.Net;
using System.Net.Http.Json;
using AuthNet.Api;
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.Tests.Integration;

public sealed class AuthNetSpaExternalLoginApiTests
{
    [Fact]
    public async Task External_providers_lists_configured_openid_connect_provider()
    {
        await using var host = await AuthNetTestHost.CreateAsync(options =>
        {
            options.OpenIdConnect.Enabled = true;
            options.OpenIdConnect.Authority = "https://identity.example.test";
            options.OpenIdConnect.ClientId = "authnet-spa";
            options.OpenIdConnect.DisplayName = "Company SSO";
        });

        var response = await host.Client.GetAsync("/auth/api/external-providers");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var providers = await response.Content.ReadFromJsonAsync<AuthNetExternalProvidersResponse>();
        Assert.NotNull(providers);
        var provider = Assert.Single(providers.Providers);
        Assert.Equal("AuthNetOidc", provider.Name);
        Assert.Equal("Company SSO", provider.DisplayName);
    }

    [Fact]
    public async Task External_login_challenge_rejects_unknown_provider_and_nonlocal_return_url()
    {
        await using var host = await AuthNetTestHost.CreateAsync(options =>
        {
            options.OpenIdConnect.Enabled = true;
            options.OpenIdConnect.Authority = "https://identity.example.test";
            options.OpenIdConnect.ClientId = "authnet-spa";
        });

        var unknownProvider = await host.PostJsonAsync("/auth/api/external-login/challenge", new AuthNetExternalChallengeRequest
        {
            Provider = "MissingProvider",
            ReturnUrl = "/spa"
        });
        var externalReturnUrl = await host.PostJsonAsync("/auth/api/external-login/challenge", new AuthNetExternalChallengeRequest
        {
            Provider = "AuthNetOidc",
            ReturnUrl = "https://evil.example.test"
        });

        Assert.Equal(HttpStatusCode.BadRequest, unknownProvider.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, externalReturnUrl.StatusCode);
        await AssertApiErrorAsync(unknownProvider, "UnknownProvider");
        await AssertApiErrorAsync(externalReturnUrl, "InvalidReturnUrl");
    }

    [Fact]
    public async Task External_login_callback_provisions_verified_external_account()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.SetExternalLoginAsync("verified.api@example.test", "provider-verified", verified: true);

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/api/external-login/callback?returnUrl=/spa-done"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetExternalLoginCallbackResponse>();
        Assert.NotNull(result);
        Assert.Equal("provisioned", result.Status);
        Assert.Equal("/spa-done", result.ReturnUrl);
        Assert.Equal("TestProvider", result.Provider);
        Assert.Equal("verified.api@example.test", result.Email);
        Assert.False(string.IsNullOrWhiteSpace(result.UserId));

        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByEmailAsync("verified.api@example.test");
        Assert.NotNull(user);
        Assert.True(user.EmailConfirmed);
        var login = Assert.Single(await userManager.GetLoginsAsync(user));
        Assert.Equal("TestProvider", login.LoginProvider);
        Assert.Equal("provider-verified", login.ProviderKey);
    }

    [Fact]
    public async Task External_login_callback_rejects_existing_local_account_by_email()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("existing.api@example.test");
        await host.SetExternalLoginAsync("existing.api@example.test", "provider-existing", verified: true);

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/api/external-login/callback"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetExternalLoginCallbackResponse>();
        Assert.NotNull(result);
        Assert.Equal("existingLocalAccount", result.Status);
        Assert.Equal("existing.api@example.test", result.Email);
        Assert.Equal(user.Id, result.UserId);

        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        Assert.Empty(await userManager.GetLoginsAsync(user));
    }

    [Fact]
    public async Task External_login_callback_requires_verified_email_and_email_claim()
    {
        await using var unverifiedHost = await AuthNetTestHost.CreateAsync();
        await unverifiedHost.SetExternalLoginAsync("unverified.api@example.test", "provider-unverified", verified: false);

        var unverifiedResponse = await unverifiedHost.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/api/external-login/callback"));

        Assert.Equal(HttpStatusCode.OK, unverifiedResponse.StatusCode);
        var unverified = await unverifiedResponse.Content.ReadFromJsonAsync<AuthNetExternalLoginCallbackResponse>();
        Assert.NotNull(unverified);
        Assert.Equal("unverifiedEmail", unverified.Status);

        await using var missingEmailHost = await AuthNetTestHost.CreateAsync();
        await missingEmailHost.SetExternalLoginAsync("missing.api@example.test", "provider-missing-email", verified: true, includeEmail: false);

        var missingEmailResponse = await missingEmailHost.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/api/external-login/callback"));

        Assert.Equal(HttpStatusCode.OK, missingEmailResponse.StatusCode);
        var missingEmail = await missingEmailResponse.Content.ReadFromJsonAsync<AuthNetExternalLoginCallbackResponse>();
        Assert.NotNull(missingEmail);
        Assert.Equal("missingEmail", missingEmail.Status);

        await AssertUserMissingAsync(unverifiedHost, "unverified.api@example.test");
        await AssertUserMissingAsync(missingEmailHost, "missing.api@example.test");
    }

    [Fact]
    public async Task External_login_callback_signs_in_existing_linked_external_account()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("linked.api@example.test");
        await AddExternalLoginAsync(host, user, "provider-linked");
        await host.SetExternalLoginAsync("linked.api@example.test", "provider-linked", verified: true);

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/api/external-login/callback"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetExternalLoginCallbackResponse>();
        Assert.NotNull(result);
        Assert.Equal("signedIn", result.Status);
        Assert.Equal(user.Id, result.UserId);

        var sessionResponse = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/api/session"));
        var session = await sessionResponse.Content.ReadFromJsonAsync<AuthNetSessionResponse>();
        Assert.NotNull(session);
        Assert.True(session.IsAuthenticated);
        Assert.Equal("linked.api@example.test", session.Email);
    }

    [Fact]
    public async Task External_link_callback_requires_authenticated_user_and_links_provider()
    {
        await using var anonymousHost = await AuthNetTestHost.CreateAsync();
        await anonymousHost.SetExternalLoginAsync("anonymous.link@example.test", "provider-anonymous-link", verified: true);

        var anonymousResponse = await anonymousHost.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/api/external-login/link/callback"));

        Assert.Equal(HttpStatusCode.Unauthorized, anonymousResponse.StatusCode);

        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("link.api@example.test");
        await host.SignInAsync("link.api@example.test");
        await host.SetExternalLoginAsync("different.link.api@example.test", "provider-link", verified: true);

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/api/external-login/link/callback?returnUrl=/auth/profile"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetExternalLinkCallbackResponse>();
        Assert.NotNull(result);
        Assert.Equal("linked", result.Status);
        Assert.Equal("/auth/profile", result.ReturnUrl);
        Assert.Equal("TestProvider", result.Provider);

        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var login = Assert.Single(await userManager.GetLoginsAsync(user));
        Assert.Equal("TestProvider", login.LoginProvider);
        Assert.Equal("provider-link", login.ProviderKey);
    }

    [Fact]
    public async Task External_link_callback_is_idempotent_for_already_linked_provider()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("linked.current@example.test");
        await AddExternalLoginAsync(host, user, "provider-current");
        await host.SignInAsync("linked.current@example.test");
        await host.SetExternalLoginAsync("linked.current@example.test", "provider-current", verified: true);

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/api/external-login/link/callback"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetExternalLinkCallbackResponse>();
        Assert.NotNull(result);
        Assert.Equal("alreadyLinked", result.Status);
    }

    private static async Task AssertApiErrorAsync(HttpResponseMessage response, string code)
    {
        var result = await response.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Code == code);
    }

    private static async Task AssertUserMissingAsync(AuthNetTestHost host, string email)
    {
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        Assert.Null(await userManager.FindByEmailAsync(email));
    }

    private static async Task AddExternalLoginAsync(AuthNetTestHost host, AuthNetUser user, string providerKey)
    {
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var persistedUser = await userManager.FindByIdAsync(user.Id);
        Assert.NotNull(persistedUser);
        var result = await userManager.AddLoginAsync(
            persistedUser,
            new UserLoginInfo("TestProvider", providerKey, "TestProvider"));
        Assert.True(result.Succeeded);
    }
}
