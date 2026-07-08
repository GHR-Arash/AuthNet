using System.Net;
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.Tests.Integration;

public sealed class AuthNetAdminUserTests
{
    [Fact]
    public async Task Anonymous_user_is_challenged_from_admin_users()
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/admin/users"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/auth/login", response.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task Non_admin_user_is_denied_from_admin_users()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("user@example.test");
        await host.SignInAsync("user@example.test");

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/admin/users"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/auth/access-denied", response.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task Admin_user_can_list_and_search_users()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateAdminUserAsync("admin@example.test");
        var alpha = await host.CreateUserAsync("alpha@example.test", displayName: "Alpha Person");
        await host.CreateUserAsync("beta@example.test", displayName: "Beta Person");
        await host.SignInAsync("admin@example.test");

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/admin/users?search=Alpha"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        Assert.Contains("alpha@example.test", body);
        Assert.Contains("Alpha Person", body);
        Assert.Contains($"/auth/admin/users/{alpha.Id}", body);
        Assert.DoesNotContain("beta@example.test", body);
    }

    [Fact]
    public async Task Admin_user_can_view_user_detail()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateAdminUserAsync("admin.detail@example.test");
        var user = await host.CreateUserAsync(
            "detail@example.test",
            emailConfirmed: false,
            displayName: "Detail Person");
        await host.SignInAsync("admin.detail@example.test");

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, $"/auth/admin/users/{user.Id}"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        Assert.Contains("detail@example.test", body);
        Assert.Contains("Detail Person", body);
        Assert.Contains("Email confirmed", body);
        Assert.Contains("External logins", body);
        Assert.Contains("Administrator access", body);
        Assert.Contains("Not granted", body);
    }

    [Fact]
    public async Task Missing_admin_user_detail_returns_not_found()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateAdminUserAsync("admin.missing@example.test");
        await host.SignInAsync("admin.missing@example.test");

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/admin/users/missing-user"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Admin_user_can_confirm_and_unconfirm_email()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateAdminUserAsync("admin.email@example.test");
        var user = await host.CreateUserAsync("email.state@example.test", emailConfirmed: false);
        await host.SignInAsync("admin.email@example.test");

        var form = await host.GetFormAsync($"/auth/admin/users/{user.Id}");
        var confirmResponse = await host.PostFormAsync($"/auth/admin/users/{user.Id}?handler=ConfirmEmail", form);

        Assert.Equal(HttpStatusCode.OK, confirmResponse.StatusCode);
        await AssertEmailConfirmedAsync(host, user.Id, expected: true);

        form = await host.GetFormAsync($"/auth/admin/users/{user.Id}");
        var unconfirmResponse = await host.PostFormAsync($"/auth/admin/users/{user.Id}?handler=UnconfirmEmail", form);

        Assert.Equal(HttpStatusCode.OK, unconfirmResponse.StatusCode);
        await AssertEmailConfirmedAsync(host, user.Id, expected: false);
    }

    [Fact]
    public async Task Admin_user_can_lock_unlock_and_reset_access_failures()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateAdminUserAsync("admin.lock@example.test");
        var user = await host.CreateUserAsync("lock.state@example.test");
        await AddAccessFailureAsync(host, user.Id);
        await host.SignInAsync("admin.lock@example.test");

        var form = await host.GetFormAsync($"/auth/admin/users/{user.Id}");
        var lockResponse = await host.PostFormAsync($"/auth/admin/users/{user.Id}?handler=Lock", form);

        Assert.Equal(HttpStatusCode.OK, lockResponse.StatusCode);
        await AssertLockedOutAsync(host, user.Id, expected: true);

        form = await host.GetFormAsync($"/auth/admin/users/{user.Id}");
        var resetResponse = await host.PostFormAsync($"/auth/admin/users/{user.Id}?handler=ResetAccessFailedCount", form);

        Assert.Equal(HttpStatusCode.OK, resetResponse.StatusCode);
        await AssertAccessFailedCountAsync(host, user.Id, expected: 0);

        form = await host.GetFormAsync($"/auth/admin/users/{user.Id}");
        var unlockResponse = await host.PostFormAsync($"/auth/admin/users/{user.Id}?handler=Unlock", form);

        Assert.Equal(HttpStatusCode.OK, unlockResponse.StatusCode);
        await AssertLockedOutAsync(host, user.Id, expected: false);
    }

    [Fact]
    public async Task Non_admin_user_cannot_post_admin_actions()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var target = await host.CreateUserAsync("target@example.test", emailConfirmed: false);
        await host.CreateUserAsync("nonadmin@example.test");
        await host.SignInAsync("nonadmin@example.test");

        var response = await host.PostFormAsync(
            $"/auth/admin/users/{target.Id}?handler=ConfirmEmail",
            new AuthNetTestForm("not-a-real-token"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/auth/access-denied", response.Headers.Location?.PathAndQuery);
        await AssertEmailConfirmedAsync(host, target.Id, expected: false);
    }

    [Fact]
    public async Task Admin_user_can_grant_administrator_access()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateAdminUserAsync("admin.grant@example.test");
        var target = await host.CreateUserAsync("grant.target@example.test");
        await host.SignInAsync("admin.grant@example.test");

        var form = await host.GetFormAsync($"/auth/admin/users/{target.Id}");
        var response = await host.PostFormAsync($"/auth/admin/users/{target.Id}?handler=GrantAdministrator", form);

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        Assert.Contains("Administrator access granted.", body);
        Assert.Contains("Granted", body);
        await AssertInRoleAsync(host, target.Id, "Administrator", expected: true);
    }

    [Fact]
    public async Task Admin_user_can_remove_administrator_access_when_another_admin_remains()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateAdminUserAsync("admin.revoke.actor@example.test");
        var target = await host.CreateAdminUserAsync("admin.revoke.target@example.test");
        await host.SignInAsync("admin.revoke.actor@example.test");

        var form = await host.GetFormAsync($"/auth/admin/users/{target.Id}");
        var response = await host.PostFormAsync($"/auth/admin/users/{target.Id}?handler=RemoveAdministrator", form);

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        Assert.Contains("Administrator access removed.", body);
        Assert.Contains("Not granted", body);
        await AssertInRoleAsync(host, target.Id, "Administrator", expected: false);
    }

    [Fact]
    public async Task Admin_user_cannot_remove_last_administrator()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var admin = await host.CreateAdminUserAsync("admin.last@example.test");
        await host.SignInAsync("admin.last@example.test");

        var form = await host.GetFormAsync($"/auth/admin/users/{admin.Id}");
        var response = await host.PostFormAsync($"/auth/admin/users/{admin.Id}?handler=RemoveAdministrator", form);

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        Assert.Contains("Cannot remove administrator access from the last administrator.", body);
        await AssertInRoleAsync(host, admin.Id, "Administrator", expected: true);
    }

    [Fact]
    public async Task Non_admin_user_cannot_post_admin_role_assignment_actions()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var target = await host.CreateUserAsync("role.target@example.test");
        await host.CreateUserAsync("role.nonadmin@example.test");
        await host.SignInAsync("role.nonadmin@example.test");

        var grantResponse = await host.PostFormAsync(
            $"/auth/admin/users/{target.Id}?handler=GrantAdministrator",
            new AuthNetTestForm("not-a-real-token"));

        Assert.Equal(HttpStatusCode.Redirect, grantResponse.StatusCode);
        Assert.StartsWith("/auth/access-denied", grantResponse.Headers.Location?.PathAndQuery);
        await AssertInRoleAsync(host, target.Id, "Administrator", expected: false);

        var removeResponse = await host.PostFormAsync(
            $"/auth/admin/users/{target.Id}?handler=RemoveAdministrator",
            new AuthNetTestForm("not-a-real-token"));

        Assert.Equal(HttpStatusCode.Redirect, removeResponse.StatusCode);
        Assert.StartsWith("/auth/access-denied", removeResponse.Headers.Location?.PathAndQuery);
        await AssertInRoleAsync(host, target.Id, "Administrator", expected: false);
    }

    private static async Task AssertEmailConfirmedAsync(AuthNetTestHost host, string userId, bool expected)
    {
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByIdAsync(userId);
        Assert.NotNull(user);
        Assert.Equal(expected, user.EmailConfirmed);
    }

    private static async Task AssertLockedOutAsync(AuthNetTestHost host, string userId, bool expected)
    {
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByIdAsync(userId);
        Assert.NotNull(user);
        Assert.Equal(expected, await userManager.IsLockedOutAsync(user));
    }

    private static async Task AssertAccessFailedCountAsync(AuthNetTestHost host, string userId, int expected)
    {
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByIdAsync(userId);
        Assert.NotNull(user);
        Assert.Equal(expected, await userManager.GetAccessFailedCountAsync(user));
    }

    private static async Task AssertInRoleAsync(AuthNetTestHost host, string userId, string roleName, bool expected)
    {
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByIdAsync(userId);
        Assert.NotNull(user);
        Assert.Equal(expected, await userManager.IsInRoleAsync(user, roleName));
    }

    private static async Task AddAccessFailureAsync(AuthNetTestHost host, string userId)
    {
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByIdAsync(userId);
        Assert.NotNull(user);
        var result = await userManager.AccessFailedAsync(user);
        Assert.True(result.Succeeded);
    }
}
