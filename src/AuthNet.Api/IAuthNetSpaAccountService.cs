using Microsoft.AspNetCore.Http;

namespace AuthNet.Api;

public interface IAuthNetSpaAccountService
{
    Task<AuthNetSessionResponse> GetSessionAsync(HttpContext httpContext, CancellationToken cancellationToken);

    Task<AuthNetProfileResponse?> GetProfileAsync(HttpContext httpContext, CancellationToken cancellationToken);

    Task<AuthNetApiResult> LoginAsync(AuthNetLoginRequest request, CancellationToken cancellationToken);

    Task<AuthNetApiResult> LogoutAsync();

    Task<AuthNetApiResult> RegisterAsync(AuthNetRegisterRequest request, HttpRequest httpRequest, CancellationToken cancellationToken);

    Task<AuthNetApiResult> ForgotPasswordAsync(AuthNetForgotPasswordRequest request, HttpRequest httpRequest, CancellationToken cancellationToken);

    Task<AuthNetApiResult> ResetPasswordAsync(AuthNetResetPasswordRequest request, CancellationToken cancellationToken);

    Task<AuthNetApiResult> ResendConfirmationAsync(AuthNetResendConfirmationRequest request, HttpRequest httpRequest, CancellationToken cancellationToken);

    Task<AuthNetApiResult> ConfirmEmailAsync(AuthNetConfirmEmailRequest request, CancellationToken cancellationToken);

    Task<AuthNetProfileUpdateResult?> UpdateProfileAsync(AuthNetUpdateProfileRequest request, HttpContext httpContext, CancellationToken cancellationToken);

    Task<AuthNetApiResult?> ChangePasswordAsync(AuthNetChangePasswordRequest request, HttpContext httpContext, CancellationToken cancellationToken);
}
