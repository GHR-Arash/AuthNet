using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using AuthNet.AspNetCore;
using AuthNet.Core;
using AuthNet.Core.Email;
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AuthNet.Tests.Integration;

internal sealed class AuthNetTestHost : IAsyncDisposable
{
    private readonly WebApplication app;
    private readonly Dictionary<string, string> cookies = new(StringComparer.Ordinal);

    private AuthNetTestHost(WebApplication app, HttpClient client, TestEmailSink emailSink)
    {
        this.app = app;
        Client = client;
        EmailSink = emailSink;
    }

    public HttpClient Client { get; }

    public IServiceProvider Services => app.Services;

    public TestEmailSink EmailSink { get; }

    public static async Task<AuthNetTestHost> CreateAsync(
        Action<AuthNetOptions>? configure = null,
        bool useLegacyUseAuthNet = false,
        Action<AuthNetStartupBuilder>? configureStartup = null)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            EnvironmentName = Environments.Development
        });

        builder.WebHost.UseTestServer();
        builder.Logging.ClearProviders();

        var databaseName = $"AuthNet.Tests.{Guid.NewGuid():N}";
        var emailSink = new TestEmailSink();

        builder.Services.AddDataProtection()
            .UseEphemeralDataProtectionProvider();
        builder.Services.AddSingleton(emailSink);
        builder.Services.AddSingleton<IAuthNetEmailSender, TestEmailSender>();
        builder.Services.AddRazorPages();
        builder.Services.AddAuthNet(
            options =>
            {
                options.EnablePublicRegistration = true;
                options.ApplicationName = "AuthNet Test";
                configure?.Invoke(options);
            },
            db => db.UseInMemoryDatabase(databaseName));

        var app = builder.Build();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGet("/host-owned", () => Results.Text("host-owned"));
        app.MapGet("/test/roles-manage", () => Results.Text("roles-manage"))
            .RequireAuthorization(AuthNetPermissions.RolesManage);
        app.MapGet("/test/external-cookie", async (
            HttpContext context,
            string email,
            string providerKey,
            bool verified,
            bool includeEmail) =>
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, providerKey),
                new("email_verified", verified ? "true" : "false")
            };
            if (includeEmail)
            {
                claims.Add(new Claim(ClaimTypes.Email, email));
            }

            var identity = new ClaimsIdentity(claims, IdentityConstants.ExternalScheme);
            var properties = new AuthenticationProperties();
            properties.Items["LoginProvider"] = "TestProvider";

            await context.SignInAsync(
                IdentityConstants.ExternalScheme,
                new ClaimsPrincipal(identity),
                properties);

            return Results.Ok();
        });
        if (configureStartup is not null)
        {
            await app.UseAuthNet(configureStartup);
        }
        else if (useLegacyUseAuthNet)
        {
#pragma warning disable CS0618
            app.UseAuthNet();
#pragma warning restore CS0618
        }
        else
        {
            app.MapAuthNet();
        }

        await app.StartAsync();

        var client = app.GetTestClient();
        client.BaseAddress = new Uri("https://localhost");

        return new AuthNetTestHost(app, client, emailSink);
    }

    public async Task<AuthNetUser> CreateUserAsync(
        string email,
        string password = "Password1!",
        bool emailConfirmed = true,
        string? displayName = null)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = new AuthNetUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = emailConfirmed,
            LockoutEnabled = true,
            DisplayName = displayName
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
        }

        return user;
    }

    public async Task<AuthNetUser> CreateAdminUserAsync(string email, string password = "Password1!")
    {
        using var scope = Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        if (!await roleManager.RoleExistsAsync("Administrator"))
        {
            var roleResult = await roleManager.CreateAsync(new IdentityRole("Administrator"));
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(" ", roleResult.Errors.Select(error => error.Description)));
            }
        }

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = new AuthNetUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            LockoutEnabled = true
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", createResult.Errors.Select(error => error.Description)));
        }

        var result = await userManager.AddToRoleAsync(user, "Administrator");
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
        }

        return user;
    }

    public async Task AddRoleAsync(string roleName, params string[] permissions)
    {
        using var scope = Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var role = await roleManager.FindByNameAsync(roleName);
        if (role is null)
        {
            var createResult = await roleManager.CreateAsync(new IdentityRole(roleName));
            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join(" ", createResult.Errors.Select(error => error.Description)));
            }

            role = await roleManager.FindByNameAsync(roleName);
        }

        if (role is null)
        {
            throw new InvalidOperationException($"Role '{roleName}' could not be loaded.");
        }

        var existingClaims = await roleManager.GetClaimsAsync(role);
        foreach (var permission in permissions)
        {
            if (existingClaims.Any(claim =>
                claim.Type == AuthNetPermissions.ClaimType &&
                claim.Value == permission))
            {
                continue;
            }

            var result = await roleManager.AddClaimAsync(
                role,
                new Claim(AuthNetPermissions.ClaimType, permission));
            if (!result.Succeeded)
            {
                throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
            }
        }
    }

    public async Task AddUserToRoleAsync(string userId, string roleName)
    {
        using var scope = Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            throw new InvalidOperationException($"User '{userId}' could not be loaded.");
        }

        var result = await userManager.AddToRoleAsync(user, roleName);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join(" ", result.Errors.Select(error => error.Description)));
        }
    }

    public async Task<HttpResponseMessage> SignInAsync(string email, string password = "Password1!")
    {
        var form = await GetFormAsync("/auth/login");

        return await PostFormAsync("/auth/login", form,
            ("Input.Email", email),
            ("Input.Password", password),
            ("Input.RememberMe", "false"));
    }

    public void ClearCookies()
    {
        cookies.Clear();
    }

    public async Task SetExternalLoginAsync(
        string email,
        string providerKey = "external-key",
        bool verified = true,
        bool includeEmail = true)
    {
        var path = $"/test/external-cookie?email={Uri.EscapeDataString(email)}&providerKey={Uri.EscapeDataString(providerKey)}&verified={verified}&includeEmail={includeEmail}";
        var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, path));
        response.EnsureSuccessStatusCode();
    }

    public async Task<AuthNetTestForm> GetFormAsync(string path)
    {
        var response = await SendAsync(new HttpRequestMessage(HttpMethod.Get, path));
        var html = await response.Content.ReadAsStringAsync();
        return new AuthNetTestForm(
            AuthNetTestHtml.GetRequestVerificationToken(html));
    }

    public async Task<HttpResponseMessage> PostFormAsync(
        string path,
        AuthNetTestForm form,
        params (string Name, string Value)[] fields)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = AuthNetTestHtml.Form(
                [.. fields, ("__RequestVerificationToken", form.RequestVerificationToken)])
        };

        return await SendAsync(request);
    }

    public async Task<HttpResponseMessage> PostJsonAsync<TRequest>(string path, TRequest body)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(body)
        };

        return await SendAsync(request);
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        if (cookies.Count > 0)
        {
            request.Headers.Add("Cookie", string.Join("; ", cookies.Select(cookie => cookie.Key + "=" + cookie.Value)));
        }

        var response = await Client.SendAsync(request);
        foreach (var cookie in AuthNetTestHtml.GetCookies(response))
        {
            cookies[cookie.Key] = cookie.Value;
        }

        return response;
    }

    public async ValueTask DisposeAsync()
    {
        Client.Dispose();
        await app.DisposeAsync();
    }
}

internal sealed record AuthNetTestForm(string RequestVerificationToken);
