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

/// <summary>Payload for resending an email confirmation message.</summary>
public sealed record AuthNetResendConfirmationRequest
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;
}
