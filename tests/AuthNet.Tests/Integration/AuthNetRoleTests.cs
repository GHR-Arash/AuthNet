using System.Net;
using AuthNet.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.Tests.Integration;

public sealed class AuthNetRoleTests
{
    [Fact]
    public async Task Anonymous_user_is_challenged_from_admin_roles()
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/admin/roles"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/auth/login", response.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task Non_admin_user_is_denied_from_admin_roles()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("roles.nonadmin@example.test");
        await host.SignInAsync("roles.nonadmin@example.test");

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/admin/roles"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/auth/access-denied", response.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task Admin_user_can_create_and_list_roles()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateAdminUserAsync("roles.admin@example.test");
        await host.SignInAsync("roles.admin@example.test");

        var form = await host.GetFormAsync("/auth/admin/roles/new");
        var response = await host.PostFormAsync("/auth/admin/roles/new", form,
            ("Input.Name", "Support Agent"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var role = await FindRoleByNameAsync(host, "Support Agent");
        Assert.NotNull(role);
        Assert.Equal($"/auth/admin/roles/{role.Id}", response.Headers.Location?.OriginalString);

        var listResponse = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/admin/roles"));
        var body = await listResponse.Content.ReadAsStringAsync();
        Assert.True(listResponse.StatusCode == HttpStatusCode.OK, body);
        Assert.Contains("Support Agent", body);
        Assert.Contains($"/auth/admin/roles/{role.Id}", body);
    }

    [Fact]
    public async Task Admin_role_creation_rejects_duplicate_name()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateAdminUserAsync("roles.duplicate.admin@example.test");
        await host.AddRoleAsync("Billing");
        await host.SignInAsync("roles.duplicate.admin@example.test");

        var form = await host.GetFormAsync("/auth/admin/roles/new");
        var response = await host.PostFormAsync("/auth/admin/roles/new", form,
            ("Input.Name", "Billing"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        Assert.Contains("A role with this name already exists.", body);
    }

    [Fact]
    public async Task Admin_user_can_add_and_remove_role_permissions()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateAdminUserAsync("roles.permissions.admin@example.test");
        await host.AddRoleAsync("Auditor");
        var role = await FindRoleByNameAsync(host, "Auditor");
        Assert.NotNull(role);
        await host.SignInAsync("roles.permissions.admin@example.test");

        var form = await host.GetFormAsync($"/auth/admin/roles/{role.Id}");
        var addResponse = await host.PostFormAsync($"/auth/admin/roles/{role.Id}?handler=AddPermission", form,
            ("Input.Permission", AuthNetPermissions.AuditView));

        var addBody = await addResponse.Content.ReadAsStringAsync();
        Assert.True(addResponse.StatusCode == HttpStatusCode.OK, addBody);
        Assert.Contains("Permission added.", addBody);
        await AssertRoleHasPermissionAsync(host, role.Id, AuthNetPermissions.AuditView, expected: true);

        form = await host.GetFormAsync($"/auth/admin/roles/{role.Id}");
        var removeResponse = await host.PostFormAsync(
            $"/auth/admin/roles/{role.Id}?handler=RemovePermission&permission={Uri.EscapeDataString(AuthNetPermissions.AuditView)}",
            form);

        var removeBody = await removeResponse.Content.ReadAsStringAsync();
        Assert.True(removeResponse.StatusCode == HttpStatusCode.OK, removeBody);
        Assert.Contains("Permission removed.", removeBody);
        await AssertRoleHasPermissionAsync(host, role.Id, AuthNetPermissions.AuditView, expected: false);
    }

    [Fact]
    public async Task Admin_role_detail_rejects_unknown_permission()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateAdminUserAsync("roles.invalid.admin@example.test");
        await host.AddRoleAsync("Invalid Permission Role");
        var role = await FindRoleByNameAsync(host, "Invalid Permission Role");
        Assert.NotNull(role);
        await host.SignInAsync("roles.invalid.admin@example.test");

        var form = await host.GetFormAsync($"/auth/admin/roles/{role.Id}");
        var response = await host.PostFormAsync($"/auth/admin/roles/{role.Id}?handler=AddPermission", form,
            ("Input.Permission", "authnet.unknown"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        Assert.Contains("Unknown permission.", body);
        await AssertRoleHasPermissionAsync(host, role.Id, "authnet.unknown", expected: false);
    }

    private static async Task<IdentityRole?> FindRoleByNameAsync(AuthNetTestHost host, string roleName)
    {
        using var scope = host.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        return await roleManager.FindByNameAsync(roleName);
    }

    private static async Task AssertRoleHasPermissionAsync(
        AuthNetTestHost host,
        string roleId,
        string permission,
        bool expected)
    {
        using var scope = host.Services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var role = await roleManager.FindByIdAsync(roleId);
        Assert.NotNull(role);
        var claims = await roleManager.GetClaimsAsync(role);
        Assert.Equal(expected, claims.Any(claim =>
            claim.Type == AuthNetPermissions.ClaimType &&
            claim.Value == permission));
    }
}
