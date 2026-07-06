namespace AuthNet.Core;

public sealed class AuthNetOptions
{
    public bool EnablePublicRegistration { get; set; }

    public bool RequireConfirmedEmail { get; set; } = true;

    public bool UseDevelopmentEmailSender { get; set; }

    public string ApplicationName { get; set; } = "AuthNet";

    public string AccountRoutePrefix { get; set; } = "/auth";

    public string? LayoutPath { get; set; }

    public string? BrandLogoUrl { get; set; }

    public string? PostgresConnectionString { get; set; }

    public AuthNetPasswordOptions Password { get; } = new();

    public AuthNetLockoutOptions Lockout { get; } = new();

    public AuthNetCookieOptions Cookie { get; } = new();

    public AuthNetOpenIdConnectOptions OpenIdConnect { get; } = new();

    public string NormalizedAccountRoutePrefix
    {
        get
        {
            var prefix = string.IsNullOrWhiteSpace(AccountRoutePrefix)
                ? "/auth"
                : AccountRoutePrefix.Trim();

            return "/" + prefix.Trim('/');
        }
    }
}

public sealed class AuthNetPasswordOptions
{
    public int RequiredLength { get; set; } = 8;

    public bool RequireDigit { get; set; } = true;

    public bool RequireLowercase { get; set; } = true;

    public bool RequireUppercase { get; set; } = true;

    public bool RequireNonAlphanumeric { get; set; } = false;
}

public sealed class AuthNetLockoutOptions
{
    public int MaxFailedAccessAttempts { get; set; } = 5;

    public TimeSpan DefaultLockoutTimeSpan { get; set; } = TimeSpan.FromMinutes(15);
}

public sealed class AuthNetCookieOptions
{
    public TimeSpan ExpireTimeSpan { get; set; } = TimeSpan.FromHours(8);

    public bool SlidingExpiration { get; set; } = true;
}

public sealed class AuthNetOpenIdConnectOptions
{
    public bool Enabled { get; set; }

    public string Scheme { get; set; } = "AuthNetOidc";

    public string DisplayName { get; set; } = "OpenID Connect";

    public string? Authority { get; set; }

    public string? ClientId { get; set; }

    public string? ClientSecret { get; set; }

    public string CallbackPath { get; set; } = "/signin-authnet-oidc";
}

