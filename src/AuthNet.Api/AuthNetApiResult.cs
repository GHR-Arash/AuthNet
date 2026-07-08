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
