using Microsoft.Extensions.Configuration;

namespace AuthNet.AspNetCore;

public sealed class AuthNetStartupBuilder
{
    internal AuthNetStartupOptions Options { get; } = new();

    public AuthNetStartupBuilder ApplyMigrations(bool enabled = true)
    {
        Options.ApplyMigrations = enabled;
        return this;
    }

    public AuthNetStartupBuilder InitialAdministrator(
        string username,
        string password,
        string email)
    {
        Options.InitialAdministrator = new InitialAdministratorOptions
        {
            UserName = username,
            Password = password,
            Email = email
        };

        return this;
    }

    public AuthNetStartupBuilder InitialAdministrator(IConfiguration configurationSection)
    {
        var initialAdministrator = InitialAdministratorOptions.FromConfiguration(configurationSection);
        if (initialAdministrator is not null)
        {
            Options.InitialAdministrator = initialAdministrator;
        }

        return this;
    }
}
