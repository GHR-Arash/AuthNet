using System.ComponentModel.DataAnnotations;
using AuthNet.Core;
using AuthNet.Core.Email;
using AuthNet.Persistence.Postgres;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AuthNet.Api;

internal sealed class AuthNetSpaAccountService(
    UserManager<AuthNetUser> userManager,
    SignInManager<AuthNetUser> signInManager,
    IAuthNetEmailSender emailSender,
    AuthNetOptions authNetOptions) : IAuthNetSpaAccountService
{
    public async Task<AuthNetSessionResponse> GetSessionAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user is null)
        {
            return new AuthNetSessionResponse(false, null, null, null, null, []);
        }

        return new AuthNetSessionResponse(
            true,
            user.Id,
            user.Email,
            user.UserName,
            user.DisplayName,
            [.. await userManager.GetRolesAsync(user)]);
    }

    public async Task<AuthNetProfileResponse?> GetProfileAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user is null)
        {
            return null;
        }

        return new AuthNetProfileResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.UserName ?? string.Empty,
            user.DisplayName,
            user.PhoneNumber,
            await userManager.IsEmailConfirmedAsync(user),
            await userManager.GetTwoFactorEnabledAsync(user),
            [.. await userManager.GetRolesAsync(user)]);
    }

    public async Task<AuthNetApiResult> LoginAsync(
        AuthNetLoginRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return AuthNetApiResult.Failure("Validation failed.", validationErrors);
        }

        var user = await FindUserAsync(request.Identifier);
        if (user is null)
        {
            return AuthNetApiResult.Failure("Invalid sign-in attempt.", [new("InvalidCredentials", null, "Invalid sign-in attempt.")]);
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            request.Password,
            request.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            return AuthNetApiResult.Success("Signed in.");
        }

        if (result.RequiresTwoFactor)
        {
            return AuthNetApiResult.Failure("Two-factor authentication is required.", [new("RequiresTwoFactor", null, "Two-factor authentication is required.")]);
        }

        if (result.IsLockedOut)
        {
            return AuthNetApiResult.Failure("This account is locked. Try again later.", [new("LockedOut", null, "This account is locked. Try again later.")]);
        }

        if (result.IsNotAllowed)
        {
            return AuthNetApiResult.Failure("This account is not allowed to sign in yet.", [new("NotAllowed", null, "This account is not allowed to sign in yet.")]);
        }

        return AuthNetApiResult.Failure("Invalid sign-in attempt.", [new("InvalidCredentials", null, "Invalid sign-in attempt.")]);
    }

    public async Task<AuthNetApiResult> LogoutAsync()
    {
        await signInManager.SignOutAsync();
        return AuthNetApiResult.Success("Signed out.");
    }

    public async Task<AuthNetApiResult> RegisterAsync(
        AuthNetRegisterRequest request,
        HttpRequest httpRequest,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!authNetOptions.EnablePublicRegistration)
        {
            return AuthNetApiResult.Failure("Registration is not available.", [new("RegistrationDisabled", null, "Registration is not available.")]);
        }

        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return AuthNetApiResult.Failure("Validation failed.", validationErrors);
        }

        var email = request.Email.Trim();
        var user = new AuthNetUser
        {
            UserName = email,
            Email = email,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
                ? null
                : request.DisplayName.Trim()
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return IdentityFailure("Registration failed.", result);
        }

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var callbackUrl = AuthNetApiEmailMessages.BuildConfirmEmailUrl(
            httpRequest,
            authNetOptions.NormalizedAccountRoutePrefix,
            user.Id,
            code);
        await emailSender.SendAsync(
            AuthNetApiEmailMessages.CreateConfirmEmailMessage(email, callbackUrl),
            cancellationToken);

        return AuthNetApiResult.Success("Account created. Check email to confirm your account.");
    }

    public async Task<AuthNetApiResult> ForgotPasswordAsync(
        AuthNetForgotPasswordRequest request,
        HttpRequest httpRequest,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return AuthNetApiResult.Failure("Validation failed.", validationErrors);
        }

        var email = request.Email.Trim();
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null && await userManager.IsEmailConfirmedAsync(user))
        {
            var code = await userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = AuthNetApiEmailMessages.BuildResetPasswordUrl(
                httpRequest,
                authNetOptions.NormalizedAccountRoutePrefix,
                code);
            await emailSender.SendAsync(
                AuthNetApiEmailMessages.CreateResetPasswordMessage(email, callbackUrl),
                cancellationToken);
        }

        return AuthNetApiResult.Success("If the account exists, password reset instructions were sent.");
    }

    public async Task<AuthNetApiResult> ResendConfirmationAsync(
        AuthNetResendConfirmationRequest request,
        HttpRequest httpRequest,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return AuthNetApiResult.Failure("Validation failed.", validationErrors);
        }

        var email = request.Email.Trim();
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null && !await userManager.IsEmailConfirmedAsync(user))
        {
            var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var callbackUrl = AuthNetApiEmailMessages.BuildConfirmEmailUrl(
                httpRequest,
                authNetOptions.NormalizedAccountRoutePrefix,
                user.Id,
                code);
            await emailSender.SendAsync(
                AuthNetApiEmailMessages.CreateConfirmEmailMessage(email, callbackUrl),
                cancellationToken);
        }

        return AuthNetApiResult.Success("If the account exists and needs confirmation, a confirmation email was sent.");
    }

    private async Task<AuthNetUser?> FindUserAsync(string identifier)
    {
        var trimmedIdentifier = identifier.Trim();
        return await userManager.FindByEmailAsync(trimmedIdentifier)
            ?? await userManager.FindByNameAsync(trimmedIdentifier);
    }

    private static AuthNetApiResult IdentityFailure(string message, IdentityResult result)
    {
        return AuthNetApiResult.Failure(
            message,
            [.. result.Errors.Select(error => new AuthNetApiError(error.Code, null, error.Description))]);
    }

    private static IReadOnlyList<AuthNetApiError> Validate<T>(T request)
        where T : notnull
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(request);
        if (Validator.TryValidateObject(request, context, results, validateAllProperties: true))
        {
            return [];
        }

        return [.. results.SelectMany(result =>
        {
            var members = result.MemberNames.Any()
                ? result.MemberNames
                : [string.Empty];
            return members.Select(member => new AuthNetApiError(
                "Validation",
                string.IsNullOrWhiteSpace(member) ? null : ToCamelCase(member),
                result.ErrorMessage ?? "Validation failed."));
        })];
    }

    private static string ToCamelCase(string value)
    {
        return string.IsNullOrEmpty(value)
            ? value
            : char.ToLowerInvariant(value[0]) + value[1..];
    }
}
