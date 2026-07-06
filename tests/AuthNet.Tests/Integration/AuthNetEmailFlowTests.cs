using System.Net;

namespace AuthNet.Tests.Integration;

public sealed class AuthNetEmailFlowTests
{
    [Fact]
    public async Task Resend_confirmation_sends_email_for_unconfirmed_account()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("unconfirmed@example.test", emailConfirmed: false);
        var form = await host.GetFormAsync("/auth/resend-confirmation");

        var response = await host.PostFormAsync("/auth/resend-confirmation", form,
            ("Input.Email", "unconfirmed@example.test"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var message = Assert.Single(host.EmailSink.Messages);
        Assert.Equal("unconfirmed@example.test", message.To);
        Assert.Equal("Confirm your email", message.Subject);
    }

    [Theory]
    [InlineData("missing@example.test")]
    [InlineData("confirmed@example.test")]
    public async Task Resend_confirmation_does_not_enumerate_accounts(string email)
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("confirmed@example.test");
        var form = await host.GetFormAsync("/auth/resend-confirmation");

        var response = await host.PostFormAsync("/auth/resend-confirmation", form,
            ("Input.Email", email));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("If the email address belongs to an unconfirmed account", body);
        Assert.Empty(host.EmailSink.Messages);
    }

    [Fact]
    public async Task Forgot_password_sends_email_for_confirmed_account()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("reset@example.test");
        var form = await host.GetFormAsync("/auth/forgot-password");

        var response = await host.PostFormAsync("/auth/forgot-password", form,
            ("Input.Email", "reset@example.test"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var message = Assert.Single(host.EmailSink.Messages);
        Assert.Equal("reset@example.test", message.To);
        Assert.Equal("Reset your password", message.Subject);
        Assert.Contains("/auth/reset-password", message.HtmlBody);
    }

    [Theory]
    [InlineData("missing@example.test")]
    [InlineData("unconfirmed@example.test")]
    public async Task Forgot_password_does_not_enumerate_accounts(string email)
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("unconfirmed@example.test", emailConfirmed: false);
        var form = await host.GetFormAsync("/auth/forgot-password");

        var response = await host.PostFormAsync("/auth/forgot-password", form,
            ("Input.Email", email));
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("If the account exists and is confirmed", body);
        Assert.Empty(host.EmailSink.Messages);
    }
}
