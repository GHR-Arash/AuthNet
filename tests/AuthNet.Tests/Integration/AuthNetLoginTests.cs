using System.Net;
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.Tests.Integration;

public sealed class AuthNetLoginTests
{
    [Fact]
    public async Task User_can_sign_in_with_user_name_when_email_is_different()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        using (var scope = host.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
            var result = await userManager.CreateAsync(new AuthNetUser
            {
                UserName = "admin",
                Email = "admin@admin.com",
                EmailConfirmed = true,
                LockoutEnabled = true
            }, "Password1!");
            Assert.True(result.Succeeded);
        }

        var form = await host.GetFormAsync("/auth/login");
        var response = await host.PostFormAsync("/auth/login", form,
            ("Input.Email", "admin"),
            ("Input.Password", "Password1!"),
            ("Input.RememberMe", "false"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
    }
}
