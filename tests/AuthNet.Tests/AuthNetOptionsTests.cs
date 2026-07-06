using AuthNet.AspNetCore;
using AuthNet.Core;
using AuthNet.Core.Email;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

namespace AuthNet.Tests;

public sealed class AuthNetOptionsTests
{
    [Fact]
    public void Defaults_keep_public_registration_disabled()
    {
        var options = new AuthNetOptions();

        Assert.False(options.EnablePublicRegistration);
        Assert.True(options.RequireConfirmedEmail);
        Assert.Equal("/auth", options.NormalizedAccountRoutePrefix);
    }

    [Theory]
    [InlineData("auth", "/auth")]
    [InlineData("/auth", "/auth")]
    [InlineData(" /identity/ ", "/identity")]
    [InlineData("", "/auth")]
    public void NormalizedAccountRoutePrefix_returns_single_leading_slash(string configured, string expected)
    {
        var options = new AuthNetOptions
        {
            AccountRoutePrefix = configured
        };

        Assert.Equal(expected, options.NormalizedAccountRoutePrefix);
    }

    [Fact]
    public void Production_configuration_requires_email_sender()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment("Production"));
        var provider = services.BuildServiceProvider();
        var validator = new AuthNetConfigurationValidator(
            provider.GetRequiredService<IHostEnvironment>(),
            new AuthNetOptions(),
            provider);

        var exception = Assert.Throws<AuthNetConfigurationException>(() => validator.Validate());

        Assert.Contains("IAuthNetEmailSender", exception.Message);
    }

    [Fact]
    public void Production_configuration_rejects_development_email_sender()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment("Production"));
        services.AddSingleton<IAuthNetEmailSender>(
            new DevelopmentAuthNetEmailSender(
                new DevelopmentEmailStore(),
                NullLogger<DevelopmentAuthNetEmailSender>.Instance));
        var provider = services.BuildServiceProvider();
        var validator = new AuthNetConfigurationValidator(
            provider.GetRequiredService<IHostEnvironment>(),
            new AuthNetOptions { UseDevelopmentEmailSender = true },
            provider);

        var exception = Assert.Throws<AuthNetConfigurationException>(() => validator.Validate());

        Assert.Contains("development email sender", exception.Message);
    }

    [Fact]
    public async Task Development_email_sender_records_messages()
    {
        var store = new DevelopmentEmailStore();
        var sender = new DevelopmentAuthNetEmailSender(
            store,
            NullLogger<DevelopmentAuthNetEmailSender>.Instance);

        await sender.SendAsync(new AuthNetEmailMessage(
            "user@example.test",
            "Subject",
            "<p>Hello</p>"));

        var message = Assert.Single(store.Messages);
        Assert.Equal("user@example.test", message.To);
        Assert.Equal("Subject", message.Subject);
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "AuthNet.Tests";

        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
