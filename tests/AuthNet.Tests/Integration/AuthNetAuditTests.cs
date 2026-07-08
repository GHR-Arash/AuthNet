using System.Net;
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.Tests.Integration;

public sealed class AuthNetAuditTests
{
    [Fact]
    public async Task Anonymous_user_is_challenged_from_admin_audit()
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/admin/audit"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/auth/login", response.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task Non_admin_user_is_denied_from_admin_audit()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("audit.nonadmin@example.test");
        await host.SignInAsync("audit.nonadmin@example.test");

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/admin/audit"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/auth/access-denied", response.Headers.Location?.PathAndQuery);
    }

    [Fact]
    public async Task Direct_user_creation_records_audit_event()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var admin = await host.CreateAdminUserAsync("audit.create.actor@example.test");
        await host.SignInAsync(admin.Email!);

        var form = await host.GetFormAsync("/auth/admin/users/new");
        var response = await host.PostFormAsync("/auth/admin/users/new", form,
            ("Input.UserName", "audit.created"),
            ("Input.Email", "audit.created@example.test"),
            ("Input.DisplayName", "Audit Created"),
            ("Input.Password", "Password1!"),
            ("Input.ConfirmPassword", "Password1!"),
            ("Input.EmailConfirmed", "true"),
            ("Input.GrantAdministrator", "true"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var created = await FindUserByEmailAsync(host, "audit.created@example.test");
        Assert.NotNull(created);
        var auditEvent = await SingleAuditEventAsync(host, "UserCreated");
        Assert.Equal(admin.Id, auditEvent.ActorUserId);
        Assert.Equal(admin.Email, auditEvent.ActorEmail);
        Assert.Equal(created.Id, auditEvent.TargetUserId);
        Assert.Equal(created.Email, auditEvent.TargetEmail);
        Assert.Equal("Succeeded", auditEvent.Outcome);
        Assert.Contains("GrantAdministrator=True", auditEvent.Metadata);
        Assert.DoesNotContain("Password1!", auditEvent.Metadata);
    }

    [Fact]
    public async Task Invitation_creation_records_audit_event_without_token()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var admin = await host.CreateAdminUserAsync("audit.invite.actor@example.test");
        await host.SignInAsync(admin.Email!);

        var form = await host.GetFormAsync("/auth/admin/invitations/new");
        var response = await host.PostFormAsync("/auth/admin/invitations/new", form,
            ("Input.Email", "audit.invited@example.test"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var auditEvent = await SingleAuditEventAsync(host, "InvitationCreated");
        Assert.Equal(admin.Id, auditEvent.ActorUserId);
        Assert.Equal("audit.invited@example.test", auditEvent.TargetEmail);
        Assert.Contains("InvitationId=", auditEvent.Metadata);
        Assert.DoesNotContain("/auth/invitations/accept", auditEvent.Metadata);
        Assert.DoesNotContain("token=", auditEvent.Metadata, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Admin_user_detail_actions_record_audit_events()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var admin = await host.CreateAdminUserAsync("audit.detail.actor@example.test");
        var target = await host.CreateUserAsync("audit.detail.target@example.test", emailConfirmed: false);
        await host.SignInAsync(admin.Email!);

        var form = await host.GetFormAsync($"/auth/admin/users/{target.Id}");
        await AssertPostOkAsync(host, $"/auth/admin/users/{target.Id}?handler=ConfirmEmail", form);
        form = await host.GetFormAsync($"/auth/admin/users/{target.Id}");
        await AssertPostOkAsync(host, $"/auth/admin/users/{target.Id}?handler=UnconfirmEmail", form);
        form = await host.GetFormAsync($"/auth/admin/users/{target.Id}");
        await AssertPostOkAsync(host, $"/auth/admin/users/{target.Id}?handler=Lock", form);
        form = await host.GetFormAsync($"/auth/admin/users/{target.Id}");
        await AssertPostOkAsync(host, $"/auth/admin/users/{target.Id}?handler=Unlock", form);
        await AddAccessFailureAsync(host, target.Id);
        form = await host.GetFormAsync($"/auth/admin/users/{target.Id}");
        await AssertPostOkAsync(host, $"/auth/admin/users/{target.Id}?handler=ResetAccessFailedCount", form);
        form = await host.GetFormAsync($"/auth/admin/users/{target.Id}");
        await AssertPostOkAsync(host, $"/auth/admin/users/{target.Id}?handler=GrantAdministrator", form);
        form = await host.GetFormAsync($"/auth/admin/users/{target.Id}");
        await AssertPostOkAsync(host, $"/auth/admin/users/{target.Id}?handler=RemoveAdministrator", form);

        var actions = await AuditActionsAsync(host);
        Assert.Contains("UserEmailConfirmed", actions);
        Assert.Contains("UserEmailUnconfirmed", actions);
        Assert.Contains("UserLocked", actions);
        Assert.Contains("UserUnlocked", actions);
        Assert.Contains("UserAccessFailuresReset", actions);
        Assert.Contains("AdministratorGranted", actions);
        Assert.Contains("AdministratorRemoved", actions);

        var events = await AuditEventsAsync(host);
        Assert.All(events, auditEvent =>
        {
            Assert.Equal(admin.Id, auditEvent.ActorUserId);
            Assert.Equal(target.Id, auditEvent.TargetUserId);
            Assert.Equal(target.Email, auditEvent.TargetEmail);
        });
    }

    [Fact]
    public async Task Admin_can_filter_audit_events()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var admin = await host.CreateAdminUserAsync("audit.filter.actor@example.test");
        await host.SignInAsync(admin.Email!);

        var form = await host.GetFormAsync("/auth/admin/users/new");
        await host.PostFormAsync("/auth/admin/users/new", form,
            ("Input.UserName", "audit.filter.created"),
            ("Input.Email", "audit.filter.created@example.test"),
            ("Input.DisplayName", "Filter Created"),
            ("Input.Password", "Password1!"),
            ("Input.ConfirmPassword", "Password1!"),
            ("Input.EmailConfirmed", "true"),
            ("Input.GrantAdministrator", "false"));

        form = await host.GetFormAsync("/auth/admin/invitations/new");
        await host.PostFormAsync("/auth/admin/invitations/new", form,
            ("Input.Email", "audit.filter.invited@example.test"));

        var response = await host.SendAsync(new HttpRequestMessage(
            HttpMethod.Get,
            "/auth/admin/audit?action=InvitationCreated&actor=filter.actor&target=filter.invited"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        Assert.Contains("InvitationCreated", body);
        Assert.Contains("audit.filter.actor@example.test", body);
        Assert.Contains("audit.filter.invited@example.test", body);
        Assert.DoesNotContain("UserCreated", body);
        Assert.DoesNotContain("audit.filter.created@example.test", body);
    }

    private static async Task AssertPostOkAsync(AuthNetTestHost host, string path, AuthNetTestForm form)
    {
        var response = await host.PostFormAsync(path, form);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
    }

    private static async Task<AuthNetAuditEvent> SingleAuditEventAsync(AuthNetTestHost host, string action)
    {
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthNetDbContext>();
        return await dbContext.AuditEvents.SingleAsync(item => item.Action == action);
    }

    private static async Task<IReadOnlyList<AuthNetAuditEvent>> AuditEventsAsync(AuthNetTestHost host)
    {
        using var scope = host.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthNetDbContext>();
        return await dbContext.AuditEvents.OrderBy(item => item.CreatedAtUtc).ToListAsync();
    }

    private static async Task<IReadOnlyList<string>> AuditActionsAsync(AuthNetTestHost host)
    {
        var events = await AuditEventsAsync(host);
        return [.. events.Select(item => item.Action)];
    }

    private static async Task<AuthNetUser?> FindUserByEmailAsync(AuthNetTestHost host, string email)
    {
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        return await userManager.FindByEmailAsync(email);
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
