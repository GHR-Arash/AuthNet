using Microsoft.Extensions.Configuration;

namespace AuthNet.AspNetCore;

internal sealed class AuthNetStartupOptions
{
    public bool ApplyMigrations { get; set; }

    public InitialAdministratorOptions? InitialAdministrator { get; set; }
}

internal sealed class InitialAdministratorOptions
{
    public bool Enabled { get; set; } = true;

    public string? UserName { get; set; }

    public string? Email { get; set; }

    public string? Password { get; set; }

    public static InitialAdministratorOptions? FromConfiguration(IConfiguration section)
    {
        var options = new InitialAdministratorOptions();
        section.Bind(options);

        var enabledValue = section["Enabled"];
        var hasConfiguredValue =
            !string.IsNullOrWhiteSpace(options.UserName) ||
            !string.IsNullOrWhiteSpace(options.Email) ||
            !string.IsNullOrWhiteSpace(options.Password);

        if (!string.IsNullOrWhiteSpace(enabledValue) && !options.Enabled)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(enabledValue) && !hasConfiguredValue)
        {
            return null;
        }

        return options;
    }
}
