using System.ComponentModel.DataAnnotations;

namespace AuthNet.Api;

/// <summary>Payload for signing in with a local password credential.</summary>
public sealed record AuthNetLoginRequest
{
    [Required]
    public string Identifier { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;

    public bool RememberMe { get; init; }
}

/// <summary>Payload for registering a local account.</summary>
public sealed record AuthNetRegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [MaxLength(200)]
    public string? DisplayName { get; init; }

    [Required]
    public string Password { get; init; } = string.Empty;
}

/// <summary>Payload for starting password recovery.</summary>
public sealed record AuthNetForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;
}

/// <summary>Payload for completing password recovery.</summary>
public sealed record AuthNetResetPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Code { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

/// <summary>Payload for resending an email confirmation message.</summary>
public sealed record AuthNetResendConfirmationRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;
}

/// <summary>Payload for completing email confirmation.</summary>
public sealed record AuthNetConfirmEmailRequest
{
    [Required]
    public string UserId { get; init; } = string.Empty;

    [Required]
    public string Code { get; init; } = string.Empty;
}

/// <summary>Payload for updating the current user's profile.</summary>
public sealed record AuthNetUpdateProfileRequest
{
    [MaxLength(200)]
    public string? DisplayName { get; init; }

    [Phone]
    public string? PhoneNumber { get; init; }
}

/// <summary>Payload for changing the current user's password.</summary>
public sealed record AuthNetChangePasswordRequest
{
    [Required]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required]
    public string NewPassword { get; init; } = string.Empty;
}
