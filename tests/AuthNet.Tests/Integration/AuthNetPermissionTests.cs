using System.Net;
using AuthNet.Core;

namespace AuthNet.Tests.Integration;

public sealed class AuthNetPermissionTests
{
    [Fact]
    public async Task Anonymous_user_is_challenged_from_permission_protected_route()
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/test/roles-manage"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/auth/login", response.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task Non_admin_user_without_permission_is_denied()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("permission.denied@example.test");
        await host.SignInAsync("permission.denied@example.test");

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/test/roles-manage"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/auth/access-denied", response.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task User_with_role_permission_claim_is_allowed()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("permission.allowed@example.test");
        await host.AddRoleAsync("Role Managers", AuthNetPermissions.RolesManage);
        await host.AddUserToRoleAsync(user.Id, "Role Managers");
        await host.SignInAsync("permission.allowed@example.test");

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/test/roles-manage"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected OK but got {(int)response.StatusCode} {response.StatusCode} at {response.Headers.Location?.PathAndQuery}. Body: {body}");
        Assert.Equal("roles-manage", body);
    }

    [Fact]
    public async Task Administrator_is_allowed_without_permission_claim()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateAdminUserAsync("permission.admin@example.test");
        await host.SignInAsync("permission.admin@example.test");

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/test/roles-manage"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(
            response.StatusCode == HttpStatusCode.OK,
            $"Expected OK but got {(int)response.StatusCode} {response.StatusCode} at {response.Headers.Location?.PathAndQuery}. Body: {body}");
        Assert.Equal("roles-manage", body);
    }

    [Fact]
    public async Task User_with_users_view_permission_can_open_user_list()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("permission.users.view@example.test");
        await host.AddRoleAsync("User Viewers", AuthNetPermissions.UsersView);
        await host.AddUserToRoleAsync(user.Id, "User Viewers");
        await host.SignInAsync("permission.users.view@example.test");

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/admin/users"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        Assert.Contains("Users", body);
    }

    [Fact]
    public async Task User_with_users_view_permission_cannot_create_users()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("permission.users.readonly@example.test");
        await host.AddRoleAsync("Readonly User Viewers", AuthNetPermissions.UsersView);
        await host.AddUserToRoleAsync(user.Id, "Readonly User Viewers");
        await host.SignInAsync("permission.users.readonly@example.test");

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/admin/users/new"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/auth/access-denied", response.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task Manage_permission_satisfies_matching_view_policy()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("permission.roles.manage@example.test");
        await host.AddRoleAsync("Role Managers", AuthNetPermissions.RolesManage);
        await host.AddUserToRoleAsync(user.Id, "Role Managers");
        await host.SignInAsync("permission.roles.manage@example.test");

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/admin/roles"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        Assert.Contains("Roles", body);
    }
}
