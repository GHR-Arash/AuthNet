using Microsoft.AspNetCore.Authentication;

namespace AuthNet.Api;

/// <summary>Represents a standard AuthNet API operation result.</summary>
public sealed record AuthNetApiResult(
    bool Succeeded,
    string Message,
    IReadOnlyList<AuthNetApiError> Errors)
{
    public static AuthNetApiResult Success(string message)
    {
        return new AuthNetApiResult(true, message, []);
    }

    public static AuthNetApiResult Failure(string message, IReadOnlyList<AuthNetApiError> errors)
    {
        return new AuthNetApiResult(false, message, errors);
    }
}

/// <summary>Represents a field-addressable AuthNet API error.</summary>
public sealed record AuthNetApiError(
    string Code,
    string? Field,
    string Description);

/// <summary>Represents a profile update operation result and updated profile when successful.</summary>
public sealed record AuthNetProfileUpdateResult(
    AuthNetApiResult Result,
    AuthNetProfileResponse? Profile);

/// <summary>Represents an AuthNet API operation result with a typed value when successful.</summary>
public sealed record AuthNetApiResponse<T>(
    AuthNetApiResult Result,
    T? Value);

/// <summary>Represents the current browser session state.</summary>
public sealed record AuthNetSessionResponse(
    bool IsAuthenticated,
    string? UserId,
    string? Email,
    string? UserName,
    string? DisplayName,
    IReadOnlyList<string> Roles);

/// <summary>Represents the current user's profile.</summary>
public sealed record AuthNetProfileResponse(
    string UserId,
    string Email,
    string UserName,
    string? DisplayName,
    string? PhoneNumber,
    bool EmailConfirmed,
    bool MfaEnabled,
    IReadOnlyList<string> Roles);

/// <summary>Represents the current user's MFA state.</summary>
public sealed record AuthNetMfaStatusResponse(
    bool IsMfaEnabled,
    bool HasAuthenticator,
    int RecoveryCodesLeft);

/// <summary>Represents authenticator-app setup data for the current user.</summary>
public sealed record AuthNetMfaSetupStartResponse(
    string SharedKey,
    string AuthenticatorUri);

/// <summary>Represents successful authenticator-app MFA setup.</summary>
public sealed record AuthNetMfaSetupVerifyResponse(
    bool IsMfaEnabled,
    IReadOnlyList<string> RecoveryCodes);

/// <summary>Represents the current user's recovery-code state.</summary>
public sealed record AuthNetRecoveryCodesResponse(
    int RecoveryCodesLeft);

/// <summary>Represents newly generated recovery codes.</summary>
public sealed record AuthNetRecoveryCodesRegenerateResponse(
    IReadOnlyList<string> RecoveryCodes);

/// <summary>Represents an available external login provider.</summary>
public sealed record AuthNetExternalProviderResponse(
    string Name,
    string DisplayName);

/// <summary>Represents available external login providers.</summary>
public sealed record AuthNetExternalProvidersResponse(
    IReadOnlyList<AuthNetExternalProviderResponse> Providers);

/// <summary>Represents an external login callback outcome.</summary>
public sealed record AuthNetExternalLoginCallbackResponse(
    string Status,
    string Message,
    string ReturnUrl,
    string? Provider,
    string? Email,
    string? UserId);

/// <summary>Represents an external login link callback outcome.</summary>
public sealed record AuthNetExternalLinkCallbackResponse(
    string Status,
    string Message,
    string ReturnUrl,
    string? Provider);

/// <summary>Represents an account invitation token inspection outcome.</summary>
public sealed record AuthNetInvitationAcceptanceStatusResponse(
    AuthNetApiResult Result,
    string Status,
    string? Email,
    DateTimeOffset? ExpiresAtUtc);

/// <summary>Represents an account invitation acceptance outcome.</summary>
public sealed record AuthNetAcceptInvitationResponse(
    AuthNetApiResult Result,
    string Status,
    string? Email,
    string? UserId);

/// <summary>Represents a prepared external authentication challenge.</summary>
public sealed record AuthNetExternalChallengeResult(
    AuthNetApiResult Result,
    string? Provider,
    AuthenticationProperties? Properties);
