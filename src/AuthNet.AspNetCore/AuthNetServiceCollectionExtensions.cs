using AuthNet.Api;
using AuthNet.Core;
using AuthNet.Core.Email;
using AuthNet.ExternalProviders;
using AuthNet.Persistence.Postgres;
using AuthNetRazor;
using AuthNetRazor.Areas.AuthNet.Pages.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace AuthNet.AspNetCore;

public static class AuthNetServiceCollectionExtensions
{
    public static IServiceCollection AddAuthNet(
        this IServiceCollection services,
        Action<AuthNetOptions>? configure = null,
        Action<DbContextOptionsBuilder>? configureDbContext = null)
    {
        var options = new AuthNetOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddOptions<AuthNetOptions>().Configure(configure ?? (_ => { }));
        services.AddSingleton<IValidateOptions<AuthNetOptions>, AuthNetOptionsValidator>();

        if (configureDbContext is null && string.IsNullOrWhiteSpace(options.PostgresConnectionString))
        {
            throw new AuthNetConfigurationException("AuthNet requires PostgresConnectionString for MVP slice 1.");
        }

        services.AddDbContext<AuthNetDbContext>(db =>
        {
            if (configureDbContext is not null)
            {
                configureDbContext(db);
                return;
            }

            db.UseNpgsql(options.PostgresConnectionString);
        });

        services
            .AddIdentityCore<AuthNetUser>(identity =>
            {
                identity.SignIn.RequireConfirmedEmail = options.RequireConfirmedEmail;
                identity.User.RequireUniqueEmail = true;
                identity.Password.RequiredLength = options.Password.RequiredLength;
                identity.Password.RequireDigit = options.Password.RequireDigit;
                identity.Password.RequireLowercase = options.Password.RequireLowercase;
                identity.Password.RequireUppercase = options.Password.RequireUppercase;
                identity.Password.RequireNonAlphanumeric = options.Password.RequireNonAlphanumeric;
                identity.Lockout.MaxFailedAccessAttempts = options.Lockout.MaxFailedAccessAttempts;
                identity.Lockout.DefaultLockoutTimeSpan = options.Lockout.DefaultLockoutTimeSpan;
            })
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<AuthNetDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services
            .AddAuthentication(authenticationOptions =>
            {
                authenticationOptions.DefaultScheme = IdentityConstants.ApplicationScheme;
                authenticationOptions.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddIdentityCookies();

        services.AddAuthentication().AddAuthNetOpenIdConnect(options.OpenIdConnect);

        services.ConfigureApplicationCookie(cookie =>
        {
            cookie.LoginPath = options.NormalizedAccountRoutePrefix + "/login";
            cookie.LogoutPath = options.NormalizedAccountRoutePrefix + "/logout";
            cookie.AccessDeniedPath = options.NormalizedAccountRoutePrefix + "/access-denied";
            cookie.ExpireTimeSpan = options.Cookie.ExpireTimeSpan;
            cookie.SlidingExpiration = options.Cookie.SlidingExpiration;
        });

        if (options.UseDevelopmentEmailSender)
        {
            services.TryAddSingleton<DevelopmentEmailStore>();
            services.TryAddSingleton<IAuthNetEmailSender, DevelopmentAuthNetEmailSender>();
        }

        services.TryAddScoped<IAuthNetAuditWriter, AuthNetAuditWriter>();
        services.AddSingleton<IAuthorizationHandler, AuthNetPermissionAuthorizationHandler>();
        services.TryAddSingleton<AuthNetConfigurationValidator>();
        services.AddAuthNetApi();
        services.AddAuthorization(authorization =>
        {
            foreach (var permission in AuthNetPermissions.All)
            {
                authorization.AddPolicy(
                    permission.Value,
                    policy => policy.Requirements.Add(new AuthNetPermissionRequirement(permission.Value)));
            }
        });

        services
            .AddRazorPages(razor =>
            {
                var prefix = options.NormalizedAccountRoutePrefix.Trim('/');
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Account/Login", $"{prefix}/login");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Account/Register", $"{prefix}/register");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Account/Logout", $"{prefix}/logout");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Account/ForgotPassword", $"{prefix}/forgot-password");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Account/ResetPassword", $"{prefix}/reset-password");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Account/ConfirmEmail", $"{prefix}/confirm-email");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Account/ResendEmailConfirmation", $"{prefix}/resend-confirmation");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Account/Profile", $"{prefix}/profile");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Account/ChangePassword", $"{prefix}/change-password");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Account/AccessDenied", $"{prefix}/access-denied");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Account/ExternalLogin", $"{prefix}/external-login");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Account/Mfa", $"{prefix}/mfa");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Account/MfaSetup", $"{prefix}/mfa/setup");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Account/MfaRecoveryCodes", $"{prefix}/mfa/recovery-codes");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Account/MfaDisable", $"{prefix}/mfa/disable");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Account/LoginWithMfa", $"{prefix}/login/mfa");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Account/LoginWithRecoveryCode", $"{prefix}/login/recovery-code");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Admin/Users/Index", $"{prefix}/admin/users");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Admin/Users/Create", $"{prefix}/admin/users/new");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Admin/Users/Detail", $"{prefix}/admin/users/{{id}}");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Admin/Roles/Index", $"{prefix}/admin/roles");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Admin/Roles/Create", $"{prefix}/admin/roles/new");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Admin/Roles/Detail", $"{prefix}/admin/roles/{{id}}");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Admin/Audit/Index", $"{prefix}/admin/audit");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Admin/Invitations/Index", $"{prefix}/admin/invitations");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Admin/Invitations/Create", $"{prefix}/admin/invitations/new");
                razor.Conventions.AddAreaPageRoute("AuthNet", "/Account/AcceptInvitation", $"{prefix}/invitations/accept");
            })
            .AddApplicationPart(typeof(LoginModel).Assembly);

        return services;
    }
}

internal sealed class AuthNetOptionsValidator : IValidateOptions<AuthNetOptions>
{
    public ValidateOptionsResult Validate(string? name, AuthNetOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.AccountRoutePrefix))
        {
            return ValidateOptionsResult.Fail("AuthNet AccountRoutePrefix is required.");
        }

        if (options.Password.RequiredLength < 6)
        {
            return ValidateOptionsResult.Fail("AuthNet password RequiredLength must be at least 6.");
        }

        return ValidateOptionsResult.Success;
    }
}

internal sealed class AuthNetConfigurationValidator(
    IHostEnvironment environment,
    AuthNetOptions options,
    IServiceProvider serviceProvider)
{
    public void Validate()
    {
        if (environment.IsProduction() && options.UseDevelopmentEmailSender)
        {
            throw new AuthNetConfigurationException("AuthNet production configuration cannot use the development email sender.");
        }

        if (environment.IsProduction() && serviceProvider.GetService<IAuthNetEmailSender>() is null)
        {
            throw new AuthNetConfigurationException("AuthNet production configuration requires an IAuthNetEmailSender implementation.");
        }
    }
}
