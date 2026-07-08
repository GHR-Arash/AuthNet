using AuthNet.Core.Email;
using AuthNet.SampleHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace AuthNet.Tests;

public sealed class SampleHostEmailSenderTests
{
    [Fact]
    public void Development_email_sender_remains_default_when_enabled()
    {
        var services = new ServiceCollection();
        var configuration = Configuration(
            ("AuthNet:UseInMemoryDatabase", "true"),
            ("AuthNet:UseDevelopmentEmailSender", "true"));
        var environment = new TestHostEnvironment(Environments.Development);

        SampleHostAuthNetPersistence.AddAuthNet(services, configuration, environment);

        using var provider = services.BuildServiceProvider();
        var emailSender = provider.GetRequiredService<IAuthNetEmailSender>();
        Assert.Equal("DevelopmentAuthNetEmailSender", emailSender.GetType().Name);
    }

    [Fact]
    public void Smtp_sender_registers_when_development_sender_is_disabled()
    {
        var services = new ServiceCollection();
        var configuration = Configuration(
            ("AuthNet:UseInMemoryDatabase", "true"),
            ("AuthNet:UseDevelopmentEmailSender", "false"),
            ("AuthNet:Email:Smtp:Enabled", "true"),
            ("AuthNet:Email:Smtp:Host", "smtp.example.test"),
            ("AuthNet:Email:Smtp:Port", "2525"),
            ("AuthNet:Email:Smtp:UserName", "smtp-user"),
            ("AuthNet:Email:Smtp:Password", "super-secret"),
            ("AuthNet:Email:Smtp:FromEmail", "no-reply@example.test"),
            ("AuthNet:Email:Smtp:FromName", "AuthNet Sample"),
            ("AuthNet:Email:Smtp:EnableSsl", "false"));
        var environment = new TestHostEnvironment(Environments.Development);

        SampleHostAuthNetPersistence.AddAuthNet(services, configuration, environment);

        using var provider = services.BuildServiceProvider();
        var emailSender = provider.GetRequiredService<IAuthNetEmailSender>();
        var smtpOptions = provider.GetRequiredService<SampleHostSmtpEmailOptions>();

        Assert.IsType<SampleHostSmtpEmailSender>(emailSender);
        Assert.Equal("smtp.example.test", smtpOptions.Host);
        Assert.Equal(2525, smtpOptions.Port);
        Assert.Equal("smtp-user", smtpOptions.UserName);
        Assert.Equal("super-secret", smtpOptions.Password);
        Assert.Equal("no-reply@example.test", smtpOptions.FromEmail);
        Assert.Equal("AuthNet Sample", smtpOptions.FromName);
        Assert.False(smtpOptions.EnableSsl);
    }

    [Fact]
    public void Smtp_sender_requires_explicit_enablement_when_development_sender_is_disabled()
    {
        var services = new ServiceCollection();
        var configuration = Configuration(
            ("AuthNet:UseDevelopmentEmailSender", "false"));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            SampleHostAuthNetPersistence.ConfigureEmailSender(services, configuration));

        Assert.Contains("Enabled=true", exception.Message);
    }

    [Fact]
    public void Smtp_validation_errors_do_not_leak_password_values()
    {
        var services = new ServiceCollection();
        var configuration = Configuration(
            ("AuthNet:UseDevelopmentEmailSender", "false"),
            ("AuthNet:Email:Smtp:Enabled", "true"),
            ("AuthNet:Email:Smtp:Password", "super-secret"));

        var exception = Assert.Throws<InvalidOperationException>(() =>
            SampleHostAuthNetPersistence.ConfigureEmailSender(services, configuration));

        Assert.DoesNotContain("super-secret", exception.Message, StringComparison.Ordinal);
    }

    private static IConfiguration Configuration(params (string Key, string Value)[] values)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(values.Select(value =>
                new KeyValuePair<string, string?>(value.Key, value.Value)))
            .Build();
    }

    private sealed class TestHostEnvironment(string environmentName) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = environmentName;

        public string ApplicationName { get; set; } = "AuthNet.Tests";

        public string ContentRootPath { get; set; } = Directory.GetCurrentDirectory();

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
