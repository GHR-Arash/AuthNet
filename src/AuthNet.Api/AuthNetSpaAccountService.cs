using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Claims;
using System.Text;
using AuthNet.Core;
using AuthNet.Core.Email;
using AuthNet.Persistence.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace AuthNet.Api;

internal sealed class AuthNetSpaAccountService(
    AuthNetDbContext dbContext,
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

        return await CreateProfileResponseAsync(user);
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

    public async Task<AuthNetApiResult> ResetPasswordAsync(
        AuthNetResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return AuthNetApiResult.Failure("Validation failed.", validationErrors);
        }

        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !TryDecodeCode(request.Code, out var code))
        {
            return AuthNetApiResult.Failure("Password reset failed.", [new("InvalidToken", null, "Password reset failed.")]);
        }

        var result = await userManager.ResetPasswordAsync(user, code, request.Password);
        return result.Succeeded
            ? AuthNetApiResult.Success("Password reset.")
            : IdentityFailure("Password reset failed.", result);
    }

    public async Task<AuthNetApiResult> ConfirmEmailAsync(
        AuthNetConfirmEmailRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return AuthNetApiResult.Failure("Validation failed.", validationErrors);
        }

        var user = await userManager.FindByIdAsync(request.UserId.Trim());
        if (user is null || !TryDecodeCode(request.Code, out var code))
        {
            return AuthNetApiResult.Failure("Email confirmation failed.", [new("InvalidToken", null, "Email confirmation failed.")]);
        }

        var result = await userManager.ConfirmEmailAsync(user, code);
        return result.Succeeded
            ? AuthNetApiResult.Success("Email confirmed.")
            : IdentityFailure("Email confirmation failed.", result);
    }

    public async Task<AuthNetProfileUpdateResult?> UpdateProfileAsync(
        AuthNetUpdateProfileRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user is null)
        {
            return null;
        }

        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return new AuthNetProfileUpdateResult(AuthNetApiResult.Failure("Validation failed.", validationErrors), null);
        }

        user.DisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
            ? null
            : request.DisplayName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber)
            ? null
            : request.PhoneNumber.Trim();

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return new AuthNetProfileUpdateResult(IdentityFailure("Profile update failed.", result), null);
        }

        return new AuthNetProfileUpdateResult(
            AuthNetApiResult.Success("Profile updated."),
            await CreateProfileResponseAsync(user));
    }

    public async Task<AuthNetApiResult?> ChangePasswordAsync(
        AuthNetChangePasswordRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user is null)
        {
            return null;
        }

        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return AuthNetApiResult.Failure("Validation failed.", validationErrors);
        }

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            return IdentityFailure("Password change failed.", result);
        }

        await signInManager.RefreshSignInAsync(user);
        return AuthNetApiResult.Success("Password changed.");
    }

    public async Task<AuthNetMfaStatusResponse?> GetMfaStatusAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.GetUserAsync(httpContext.User);
        return user is null
            ? null
            : await CreateMfaStatusResponseAsync(user);
    }

    public async Task<AuthNetMfaSetupStartResponse?> StartMfaSetupAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user is null)
        {
            return null;
        }

        var unformattedKey = await EnsureAuthenticatorKeyAsync(user);
        return new AuthNetMfaSetupStartResponse(
            FormatAuthenticatorKey(unformattedKey),
            BuildAuthenticatorUri(user, unformattedKey));
    }

    public async Task<AuthNetApiResponse<AuthNetMfaSetupVerifyResponse>?> VerifyMfaSetupAsync(
        AuthNetMfaSetupVerifyRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user is null)
        {
            return null;
        }

        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return Failure<AuthNetMfaSetupVerifyResponse>("Validation failed.", validationErrors);
        }

        await EnsureAuthenticatorKeyAsync(user);
        var verificationCode = NormalizeAuthenticatorCode(request.Code);
        var isValid = await userManager.VerifyTwoFactorTokenAsync(
            user,
            TokenOptions.DefaultAuthenticatorProvider,
            verificationCode);
        if (!isValid)
        {
            return Failure<AuthNetMfaSetupVerifyResponse>(
                "Authenticator code is invalid.",
                [new("InvalidAuthenticatorCode", null, "Authenticator code is invalid.")]);
        }

        var enableResult = await userManager.SetTwoFactorEnabledAsync(user, true);
        if (!enableResult.Succeeded)
        {
            return new AuthNetApiResponse<AuthNetMfaSetupVerifyResponse>(
                IdentityFailure("MFA setup failed.", enableResult),
                null);
        }

        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        return new AuthNetApiResponse<AuthNetMfaSetupVerifyResponse>(
            AuthNetApiResult.Success("Multi-factor authentication is enabled."),
            new AuthNetMfaSetupVerifyResponse(true, [.. recoveryCodes ?? []]));
    }

    public async Task<AuthNetApiResult?> DisableMfaAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user is null)
        {
            return null;
        }

        var result = await userManager.SetTwoFactorEnabledAsync(user, false);
        if (!result.Succeeded)
        {
            return IdentityFailure("MFA disable failed.", result);
        }

        await userManager.ResetAuthenticatorKeyAsync(user);
        return AuthNetApiResult.Success("Multi-factor authentication disabled.");
    }

    public async Task<AuthNetRecoveryCodesResponse?> GetRecoveryCodesAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.GetUserAsync(httpContext.User);
        return user is null
            ? null
            : new AuthNetRecoveryCodesResponse(await userManager.CountRecoveryCodesAsync(user));
    }

    public async Task<AuthNetApiResponse<AuthNetRecoveryCodesRegenerateResponse>?> RegenerateRecoveryCodesAsync(
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.GetUserAsync(httpContext.User);
        if (user is null)
        {
            return null;
        }

        if (!await userManager.GetTwoFactorEnabledAsync(user))
        {
            return Failure<AuthNetRecoveryCodesRegenerateResponse>(
                "Multi-factor authentication is not enabled.",
                [new("MfaNotEnabled", null, "Multi-factor authentication is not enabled.")]);
        }

        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        return new AuthNetApiResponse<AuthNetRecoveryCodesRegenerateResponse>(
            AuthNetApiResult.Success("Recovery codes regenerated."),
            new AuthNetRecoveryCodesRegenerateResponse([.. recoveryCodes ?? []]));
    }

    public async Task<AuthNetApiResult> LoginWithMfaAsync(
        AuthNetMfaChallengeRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return AuthNetApiResult.Failure("Validation failed.", validationErrors);
        }

        var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user is null)
        {
            return AuthNetApiResult.Failure("No two-factor sign-in is pending.", [new("NoTwoFactorChallenge", null, "No two-factor sign-in is pending.")]);
        }

        var result = await signInManager.TwoFactorAuthenticatorSignInAsync(
            NormalizeAuthenticatorCode(request.Code),
            request.RememberMe,
            rememberClient: false);

        if (result.Succeeded)
        {
            return AuthNetApiResult.Success("Signed in.");
        }

        if (result.IsLockedOut)
        {
            return AuthNetApiResult.Failure("This account is locked. Try again later.", [new("LockedOut", null, "This account is locked. Try again later.")]);
        }

        return AuthNetApiResult.Failure("Invalid authenticator code.", [new("InvalidAuthenticatorCode", null, "Invalid authenticator code.")]);
    }

    public async Task<AuthNetApiResult> LoginWithRecoveryCodeAsync(
        AuthNetRecoveryCodeLoginRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return AuthNetApiResult.Failure("Validation failed.", validationErrors);
        }

        var user = await signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user is null)
        {
            return AuthNetApiResult.Failure("No two-factor sign-in is pending.", [new("NoTwoFactorChallenge", null, "No two-factor sign-in is pending.")]);
        }

        var result = await signInManager.TwoFactorRecoveryCodeSignInAsync(
            request.RecoveryCode.Replace(" ", string.Empty));

        if (result.Succeeded)
        {
            return AuthNetApiResult.Success("Signed in.");
        }

        if (result.IsLockedOut)
        {
            return AuthNetApiResult.Failure("This account is locked. Try again later.", [new("LockedOut", null, "This account is locked. Try again later.")]);
        }

        return AuthNetApiResult.Failure("Invalid recovery code.", [new("InvalidRecoveryCode", null, "Invalid recovery code.")]);
    }

    public async Task<AuthNetExternalProvidersResponse> GetExternalProvidersAsync()
    {
        var schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
        return new AuthNetExternalProvidersResponse(
            [.. schemes
                .Where(scheme => !string.IsNullOrWhiteSpace(scheme.DisplayName))
                .Select(scheme => new AuthNetExternalProviderResponse(
                    scheme.Name,
                    scheme.DisplayName ?? scheme.Name))]);
    }

    public async Task<AuthNetExternalChallengeResult> PrepareExternalLoginChallengeAsync(
        AuthNetExternalChallengeRequest request,
        string callbackPath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return ExternalChallengeFailure("Validation failed.", validationErrors);
        }

        var provider = request.Provider.Trim();
        if (!await IsExternalProviderAsync(provider))
        {
            return ExternalChallengeFailure("External provider is not available.", [new("UnknownProvider", "provider", "External provider is not available.")]);
        }

        if (!TryNormalizeReturnUrl(request.ReturnUrl, out var returnUrl))
        {
            return ExternalChallengeFailure("Return URL must be local.", [new("InvalidReturnUrl", "returnUrl", "Return URL must be local.")]);
        }

        var redirectUrl = BuildCallbackPath(callbackPath, returnUrl);
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new AuthNetExternalChallengeResult(AuthNetApiResult.Success("External login challenge started."), provider, properties);
    }

    public async Task<AuthNetExternalChallengeResult?> PrepareExternalLinkChallengeAsync(
        AuthNetExternalChallengeRequest request,
        HttpContext httpContext,
        string callbackPath,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var currentUser = await userManager.GetUserAsync(httpContext.User);
        if (currentUser is null)
        {
            return null;
        }

        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return ExternalChallengeFailure("Validation failed.", validationErrors);
        }

        var provider = request.Provider.Trim();
        if (!await IsExternalProviderAsync(provider))
        {
            return ExternalChallengeFailure("External provider is not available.", [new("UnknownProvider", "provider", "External provider is not available.")]);
        }

        if (!TryNormalizeReturnUrl(request.ReturnUrl, out var returnUrl))
        {
            return ExternalChallengeFailure("Return URL must be local.", [new("InvalidReturnUrl", "returnUrl", "Return URL must be local.")]);
        }

        var redirectUrl = BuildCallbackPath(callbackPath, returnUrl);
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new AuthNetExternalChallengeResult(AuthNetApiResult.Success("External link challenge started."), provider, properties);
    }

    public async Task<AuthNetExternalLoginCallbackResponse> CompleteExternalLoginAsync(
        HttpContext httpContext,
        string? returnUrl,
        string? remoteError,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var safeReturnUrl = NormalizeReturnUrlOrDefault(returnUrl);
        if (remoteError is not null)
        {
            return ExternalLoginCallback("remoteError", "External provider returned an error.", safeReturnUrl);
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            return ExternalLoginCallback("missingExternalInfo", "External login information was not available.", safeReturnUrl);
        }

        var signInResult = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: false,
            bypassTwoFactor: true);

        if (signInResult.Succeeded)
        {
            var linkedUser = await userManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);
            return ExternalLoginCallback(
                "signedIn",
                "Signed in.",
                safeReturnUrl,
                info,
                linkedUser?.Id,
                linkedUser?.Email);
        }

        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
        {
            return ExternalLoginCallback("missingEmail", "The external provider did not return an email address.", safeReturnUrl, info);
        }

        if (!HasVerifiedEmail(info.Principal))
        {
            return ExternalLoginCallback("unverifiedEmail", "The external provider did not return a verified email address.", safeReturnUrl, info, email: email);
        }

        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            return ExternalLoginCallback(
                "existingLocalAccount",
                "An account already exists for this email address. Sign in with your password before linking an external login.",
                safeReturnUrl,
                info,
                existingUser.Id,
                email);
        }

        var user = new AuthNetUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            DisplayName = info.Principal.Identity?.Name
        };

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
        {
            return ExternalLoginCallback("externalLoginFailed", JoinIdentityErrors(createResult), safeReturnUrl, info, email: email);
        }

        var addLoginResult = await userManager.AddLoginAsync(user, info);
        if (!addLoginResult.Succeeded)
        {
            return ExternalLoginCallback("externalLoginFailed", JoinIdentityErrors(addLoginResult), safeReturnUrl, info, user.Id, email);
        }

        await signInManager.SignInAsync(user, isPersistent: false, info.LoginProvider);
        return ExternalLoginCallback("provisioned", "External account provisioned and signed in.", safeReturnUrl, info, user.Id, email);
    }

    public async Task<AuthNetExternalLinkCallbackResponse?> CompleteExternalLinkAsync(
        HttpContext httpContext,
        string? returnUrl,
        string? remoteError,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var currentUser = await userManager.GetUserAsync(httpContext.User);
        if (currentUser is null)
        {
            return null;
        }

        var safeReturnUrl = NormalizeReturnUrlOrDefault(returnUrl);
        if (remoteError is not null)
        {
            return ExternalLinkCallback("remoteError", "External provider returned an error.", safeReturnUrl);
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            return ExternalLinkCallback("missingExternalInfo", "External login information was not available.", safeReturnUrl);
        }

        var currentLogins = await userManager.GetLoginsAsync(currentUser);
        if (currentLogins.Any(login =>
            login.LoginProvider == info.LoginProvider &&
            login.ProviderKey == info.ProviderKey))
        {
            return ExternalLinkCallback("alreadyLinked", "External login is already linked.", safeReturnUrl, info.LoginProvider);
        }

        var linkResult = await userManager.AddLoginAsync(currentUser, info);
        if (!linkResult.Succeeded)
        {
            return ExternalLinkCallback("linkFailed", JoinIdentityErrors(linkResult), safeReturnUrl, info.LoginProvider);
        }

        await signInManager.RefreshSignInAsync(currentUser);
        return ExternalLinkCallback("linked", "External login linked.", safeReturnUrl, info.LoginProvider);
    }

    public async Task<AuthNetInvitationAcceptanceStatusResponse> GetInvitationAcceptanceStatusAsync(
        string? token,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var invitation = await FindInvitationByTokenAsync(token, cancellationToken);
        if (invitation is null)
        {
            return InvitationStatus(
                "invalidToken",
                AuthNetApiResult.Failure("Invitation is invalid.", [new("InvalidToken", "token", "Invitation is invalid.")]));
        }

        var state = await ClassifyInvitationAsync(invitation, cancellationToken);
        return state switch
        {
            "valid" => InvitationStatus(
                "valid",
                AuthNetApiResult.Success("Invitation is valid."),
                invitation),
            "expired" => InvitationStatus(
                "expired",
                AuthNetApiResult.Failure("Invitation has expired.", [new("ExpiredInvitation", "token", "Invitation has expired.")]),
                invitation),
            "alreadyAccepted" => InvitationStatus(
                "alreadyAccepted",
                AuthNetApiResult.Failure("Invitation has already been accepted.", [new("AlreadyAccepted", "token", "Invitation has already been accepted.")]),
                invitation),
            "existingUser" => InvitationStatus(
                "existingUser",
                AuthNetApiResult.Failure("A user with this email already exists.", [new("ExistingUser", "token", "A user with this email already exists.")]),
                invitation),
            _ => InvitationStatus(
                "invalidToken",
                AuthNetApiResult.Failure("Invitation is invalid.", [new("InvalidToken", "token", "Invitation is invalid.")]))
        };
    }

    public async Task<AuthNetAcceptInvitationResponse> AcceptInvitationAsync(
        AuthNetAcceptInvitationRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return AcceptInvitationResponse(
                "validationFailed",
                AuthNetApiResult.Failure("Validation failed.", validationErrors));
        }

        var invitation = await FindInvitationByTokenAsync(request.Token, cancellationToken);
        if (invitation is null)
        {
            return AcceptInvitationResponse(
                "invalidToken",
                AuthNetApiResult.Failure("Invitation is invalid.", [new("InvalidToken", "token", "Invitation is invalid.")]));
        }

        var state = await ClassifyInvitationAsync(invitation, cancellationToken);
        if (state != "valid")
        {
            return state switch
            {
                "expired" => AcceptInvitationResponse(
                    "expired",
                    AuthNetApiResult.Failure("Invitation has expired.", [new("ExpiredInvitation", "token", "Invitation has expired.")]),
                    invitation.Email),
                "alreadyAccepted" => AcceptInvitationResponse(
                    "alreadyAccepted",
                    AuthNetApiResult.Failure("Invitation has already been accepted.", [new("AlreadyAccepted", "token", "Invitation has already been accepted.")]),
                    invitation.Email),
                "existingUser" => AcceptInvitationResponse(
                    "existingUser",
                    AuthNetApiResult.Failure("A user with this email already exists.", [new("ExistingUser", "token", "A user with this email already exists.")]),
                    invitation.Email),
                _ => AcceptInvitationResponse(
                    "invalidToken",
                    AuthNetApiResult.Failure("Invitation is invalid.", [new("InvalidToken", "token", "Invitation is invalid.")]))
            };
        }

        var user = new AuthNetUser
        {
            UserName = request.UserName.Trim(),
            Email = invitation.Email,
            EmailConfirmed = true,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
                ? null
                : request.DisplayName.Trim()
        };

        var createResult = await userManager.CreateAsync(user, request.Password);
        if (!createResult.Succeeded)
        {
            return AcceptInvitationResponse(
                "validationFailed",
                IdentityFailure("Invitation acceptance failed.", createResult),
                invitation.Email);
        }

        invitation.AcceptedAtUtc = DateTimeOffset.UtcNow;
        invitation.AcceptedByUserId = user.Id;
        await dbContext.SaveChangesAsync(cancellationToken);

        await signInManager.SignInAsync(user, isPersistent: false);
        return AcceptInvitationResponse(
            "accepted",
            AuthNetApiResult.Success("Invitation accepted."),
            invitation.Email,
            user.Id);
    }

    private async Task<AuthNetUser?> FindUserAsync(string identifier)
    {
        var trimmedIdentifier = identifier.Trim();
        return await userManager.FindByEmailAsync(trimmedIdentifier)
            ?? await userManager.FindByNameAsync(trimmedIdentifier);
    }

    private async Task<AuthNetInvitation?> FindInvitationByTokenAsync(
        string? token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var tokenHash = AuthNetInvitationToken.Hash(token);
        return await dbContext.Invitations.SingleOrDefaultAsync(
            invitation => invitation.TokenHash == tokenHash,
            cancellationToken);
    }

    private async Task<string> ClassifyInvitationAsync(
        AuthNetInvitation invitation,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var now = DateTimeOffset.UtcNow;
        if (invitation.IsAccepted)
        {
            return "alreadyAccepted";
        }

        if (invitation.IsExpired(now))
        {
            return "expired";
        }

        return await userManager.FindByEmailAsync(invitation.Email) is null
            ? "valid"
            : "existingUser";
    }

    private static AuthNetInvitationAcceptanceStatusResponse InvitationStatus(
        string status,
        AuthNetApiResult result,
        AuthNetInvitation? invitation = null)
    {
        return new AuthNetInvitationAcceptanceStatusResponse(
            result,
            status,
            invitation?.Email,
            invitation?.ExpiresAtUtc);
    }

    private static AuthNetAcceptInvitationResponse AcceptInvitationResponse(
        string status,
        AuthNetApiResult result,
        string? email = null,
        string? userId = null)
    {
        return new AuthNetAcceptInvitationResponse(result, status, email, userId);
    }

    private static AuthNetApiResult IdentityFailure(string message, IdentityResult result)
    {
        return AuthNetApiResult.Failure(
            message,
            [.. result.Errors.Select(error => new AuthNetApiError(error.Code, null, error.Description))]);
    }

    private async Task<AuthNetProfileResponse> CreateProfileResponseAsync(AuthNetUser user)
    {
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

    private async Task<AuthNetMfaStatusResponse> CreateMfaStatusResponseAsync(AuthNetUser user)
    {
        return new AuthNetMfaStatusResponse(
            await userManager.GetTwoFactorEnabledAsync(user),
            !string.IsNullOrWhiteSpace(await userManager.GetAuthenticatorKeyAsync(user)),
            await userManager.CountRecoveryCodesAsync(user));
    }

    private async Task<string> EnsureAuthenticatorKeyAsync(AuthNetUser user)
    {
        var unformattedKey = await userManager.GetAuthenticatorKeyAsync(user);
        if (!string.IsNullOrWhiteSpace(unformattedKey))
        {
            return unformattedKey;
        }

        await userManager.ResetAuthenticatorKeyAsync(user);
        return await userManager.GetAuthenticatorKeyAsync(user) ?? string.Empty;
    }

    private string BuildAuthenticatorUri(AuthNetUser user, string unformattedKey)
    {
        var issuer = string.IsNullOrWhiteSpace(authNetOptions.ApplicationName)
            ? "AuthNet"
            : authNetOptions.ApplicationName;
        var email = user.Email ?? user.UserName ?? user.Id;

        return "otpauth://totp/"
            + WebUtility.UrlEncode(issuer)
            + ":"
            + WebUtility.UrlEncode(email)
            + "?secret="
            + WebUtility.UrlEncode(unformattedKey)
            + "&issuer="
            + WebUtility.UrlEncode(issuer)
            + "&digits=6";
    }

    private static AuthNetApiResponse<T> Failure<T>(string message, IReadOnlyList<AuthNetApiError> errors)
    {
        return new AuthNetApiResponse<T>(AuthNetApiResult.Failure(message, errors), default);
    }

    private async Task<bool> IsExternalProviderAsync(string provider)
    {
        var schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
        return schemes.Any(scheme => string.Equals(scheme.Name, provider, StringComparison.Ordinal));
    }

    private static AuthNetExternalChallengeResult ExternalChallengeFailure(
        string message,
        IReadOnlyList<AuthNetApiError> errors)
    {
        return new AuthNetExternalChallengeResult(AuthNetApiResult.Failure(message, errors), null, null);
    }

    private static string BuildCallbackPath(string callbackPath, string returnUrl)
    {
        return QueryHelpers.AddQueryString(callbackPath, "returnUrl", returnUrl);
    }

    private static string NormalizeReturnUrlOrDefault(string? returnUrl)
    {
        return TryNormalizeReturnUrl(returnUrl, out var safeReturnUrl)
            ? safeReturnUrl
            : "/";
    }

    private static bool TryNormalizeReturnUrl(string? returnUrl, out string safeReturnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            safeReturnUrl = "/";
            return true;
        }

        var trimmed = returnUrl.Trim();
        if (trimmed.StartsWith("/", StringComparison.Ordinal) &&
            !trimmed.StartsWith("//", StringComparison.Ordinal) &&
            !trimmed.Contains("://", StringComparison.Ordinal))
        {
            safeReturnUrl = trimmed;
            return true;
        }

        safeReturnUrl = "/";
        return false;
    }

    private static AuthNetExternalLoginCallbackResponse ExternalLoginCallback(
        string status,
        string message,
        string returnUrl,
        ExternalLoginInfo? info = null,
        string? userId = null,
        string? email = null)
    {
        return new AuthNetExternalLoginCallbackResponse(
            status,
            message,
            returnUrl,
            info?.LoginProvider,
            email ?? info?.Principal.FindFirstValue(ClaimTypes.Email),
            userId);
    }

    private static AuthNetExternalLinkCallbackResponse ExternalLinkCallback(
        string status,
        string message,
        string returnUrl,
        string? provider = null)
    {
        return new AuthNetExternalLinkCallbackResponse(status, message, returnUrl, provider);
    }

    private static bool HasVerifiedEmail(ClaimsPrincipal principal)
    {
        var verifiedClaim = principal.FindFirst("email_verified")?.Value;
        return string.Equals(verifiedClaim, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(verifiedClaim, "1", StringComparison.OrdinalIgnoreCase);
    }

    private static string JoinIdentityErrors(IdentityResult result)
    {
        return string.Join(" ", result.Errors.Select(error => error.Description));
    }

    private static string NormalizeAuthenticatorCode(string code)
    {
        return code.Replace(" ", string.Empty).Replace("-", string.Empty);
    }

    private static string FormatAuthenticatorKey(string unformattedKey)
    {
        return string.Join(" ", unformattedKey.Chunk(4).Select(chunk => new string(chunk))).ToLowerInvariant();
    }

    private static bool TryDecodeCode(string encodedCode, out string code)
    {
        try
        {
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedCode));
            return true;
        }
        catch (FormatException)
        {
            code = string.Empty;
            return false;
        }
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
