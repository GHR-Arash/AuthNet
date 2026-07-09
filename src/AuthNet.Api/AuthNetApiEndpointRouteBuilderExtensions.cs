using AuthNet.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.Api;

public static class AuthNetApiEndpointRouteBuilderExtensions
{
    public static RouteGroupBuilder MapAuthNetApi(this IEndpointRouteBuilder endpoints)
    {
        var options = endpoints.ServiceProvider.GetRequiredService<AuthNetOptions>();
        var group = endpoints
            .MapGroup(options.NormalizedAccountRoutePrefix + "/api")
            .WithTags("AuthNet SPA");

        group.MapGet("/openapi.json", () =>
        {
            return TypedResults.Json(AuthNetOpenApiDocumentBuilder.Build(options));
        })
            .WithName("AuthNetApiOpenApi")
            .WithSummary("Get the OpenAPI document for AuthNet SPA JSON endpoints.")
            .Produces(StatusCodes.Status200OK, contentType: "application/json");

        group.MapGet("/session", async Task<Ok<AuthNetSessionResponse>> (
            HttpContext httpContext,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            return TypedResults.Ok(await accountService.GetSessionAsync(httpContext, cancellationToken));
        })
            .WithName("AuthNetApiSession")
            .WithSummary("Get the current AuthNet browser session.")
            .Produces<AuthNetSessionResponse>(StatusCodes.Status200OK);

        group.MapGet("/profile", async Task<Results<Ok<AuthNetProfileResponse>, UnauthorizedHttpResult>> (
            HttpContext httpContext,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var profile = await accountService.GetProfileAsync(httpContext, cancellationToken);
            return profile is null
                ? TypedResults.Unauthorized()
                : TypedResults.Ok(profile);
        })
            .WithName("AuthNetApiProfile")
            .WithSummary("Get the current authenticated user's profile.")
            .Produces<AuthNetProfileResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/profile", async Task<IResult> (
            AuthNetUpdateProfileRequest request,
            HttpContext httpContext,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.UpdateProfileAsync(request, httpContext, cancellationToken);
            if (result is null)
            {
                return TypedResults.Unauthorized();
            }

            return result.Result.Succeeded
                ? TypedResults.Ok(result.Profile)
                : TypedResults.BadRequest(result.Result);
        })
            .WithName("AuthNetApiUpdateProfile")
            .WithSummary("Update the current authenticated user's profile.")
            .Produces<AuthNetProfileResponse>(StatusCodes.Status200OK)
            .Produces<AuthNetApiResult>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/login", async Task<IResult> (
            AuthNetLoginRequest request,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.LoginAsync(request, cancellationToken);
            return ToLoginResult(result);
        })
            .WithName("AuthNetApiLogin")
            .WithSummary("Sign in with a local password credential.")
            .Produces<AuthNetApiResult>(StatusCodes.Status200OK)
            .Produces<AuthNetApiResult>(StatusCodes.Status400BadRequest)
            .Produces<AuthNetApiResult>(StatusCodes.Status401Unauthorized)
            .Produces<AuthNetApiResult>(StatusCodes.Status409Conflict);

        group.MapPost("/logout", async Task<Ok<AuthNetApiResult>> (
            IAuthNetSpaAccountService accountService) =>
        {
            return TypedResults.Ok(await accountService.LogoutAsync());
        })
            .WithName("AuthNetApiLogout")
            .WithSummary("Sign out of the current AuthNet browser session.")
            .Produces<AuthNetApiResult>(StatusCodes.Status200OK);

        group.MapPost("/register", async Task<IResult> (
            AuthNetRegisterRequest request,
            HttpContext httpContext,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.RegisterAsync(request, httpContext.Request, cancellationToken);
            return ToWriteResult(result);
        })
            .WithName("AuthNetApiRegister")
            .WithSummary("Register a local AuthNet account.")
            .Produces<AuthNetApiResult>(StatusCodes.Status200OK)
            .Produces<AuthNetApiResult>(StatusCodes.Status400BadRequest);

        group.MapPost("/forgot-password", async Task<IResult> (
            AuthNetForgotPasswordRequest request,
            HttpContext httpContext,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.ForgotPasswordAsync(request, httpContext.Request, cancellationToken);
            return ToWriteResult(result);
        })
            .WithName("AuthNetApiForgotPassword")
            .WithSummary("Send password reset instructions when the account can receive them.")
            .Produces<AuthNetApiResult>(StatusCodes.Status200OK)
            .Produces<AuthNetApiResult>(StatusCodes.Status400BadRequest);

        group.MapPost("/reset-password", async Task<IResult> (
            AuthNetResetPasswordRequest request,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.ResetPasswordAsync(request, cancellationToken);
            return ToWriteResult(result);
        })
            .WithName("AuthNetApiResetPassword")
            .WithSummary("Complete password recovery with a reset code.")
            .Produces<AuthNetApiResult>(StatusCodes.Status200OK)
            .Produces<AuthNetApiResult>(StatusCodes.Status400BadRequest);

        group.MapPost("/resend-confirmation", async Task<IResult> (
            AuthNetResendConfirmationRequest request,
            HttpContext httpContext,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.ResendConfirmationAsync(request, httpContext.Request, cancellationToken);
            return ToWriteResult(result);
        })
            .WithName("AuthNetApiResendConfirmation")
            .WithSummary("Send email confirmation instructions when the account needs them.")
            .Produces<AuthNetApiResult>(StatusCodes.Status200OK)
            .Produces<AuthNetApiResult>(StatusCodes.Status400BadRequest);

        group.MapPost("/confirm-email", async Task<IResult> (
            AuthNetConfirmEmailRequest request,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.ConfirmEmailAsync(request, cancellationToken);
            return ToWriteResult(result);
        })
            .WithName("AuthNetApiConfirmEmail")
            .WithSummary("Complete email confirmation with a confirmation code.")
            .Produces<AuthNetApiResult>(StatusCodes.Status200OK)
            .Produces<AuthNetApiResult>(StatusCodes.Status400BadRequest);

        group.MapPost("/change-password", async Task<IResult> (
            AuthNetChangePasswordRequest request,
            HttpContext httpContext,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.ChangePasswordAsync(request, httpContext, cancellationToken);
            return result is null
                ? TypedResults.Unauthorized()
                : ToWriteResult(result);
        })
            .WithName("AuthNetApiChangePassword")
            .WithSummary("Change the current authenticated user's password.")
            .Produces<AuthNetApiResult>(StatusCodes.Status200OK)
            .Produces<AuthNetApiResult>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/mfa", async Task<IResult> (
            HttpContext httpContext,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.GetMfaStatusAsync(httpContext, cancellationToken);
            return result is null
                ? TypedResults.Unauthorized()
                : TypedResults.Ok(result);
        })
            .WithName("AuthNetApiMfaStatus")
            .WithSummary("Get the current authenticated user's MFA state.")
            .Produces<AuthNetMfaStatusResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/mfa/setup/start", async Task<IResult> (
            HttpContext httpContext,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.StartMfaSetupAsync(httpContext, cancellationToken);
            return result is null
                ? TypedResults.Unauthorized()
                : TypedResults.Ok(result);
        })
            .WithName("AuthNetApiMfaSetupStart")
            .WithSummary("Start authenticator-app MFA setup for the current user.")
            .Produces<AuthNetMfaSetupStartResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/mfa/setup/verify", async Task<IResult> (
            AuthNetMfaSetupVerifyRequest request,
            HttpContext httpContext,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.VerifyMfaSetupAsync(request, httpContext, cancellationToken);
            return ToDataResult(result);
        })
            .WithName("AuthNetApiMfaSetupVerify")
            .WithSummary("Verify an authenticator code and enable MFA for the current user.")
            .Produces<AuthNetMfaSetupVerifyResponse>(StatusCodes.Status200OK)
            .Produces<AuthNetApiResult>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/mfa/disable", async Task<IResult> (
            HttpContext httpContext,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.DisableMfaAsync(httpContext, cancellationToken);
            return result is null
                ? TypedResults.Unauthorized()
                : ToWriteResult(result);
        })
            .WithName("AuthNetApiMfaDisable")
            .WithSummary("Disable authenticator-app MFA for the current user.")
            .Produces<AuthNetApiResult>(StatusCodes.Status200OK)
            .Produces<AuthNetApiResult>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/mfa/recovery-codes", async Task<IResult> (
            HttpContext httpContext,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.GetRecoveryCodesAsync(httpContext, cancellationToken);
            return result is null
                ? TypedResults.Unauthorized()
                : TypedResults.Ok(result);
        })
            .WithName("AuthNetApiMfaRecoveryCodes")
            .WithSummary("Get the current user's recovery-code count.")
            .Produces<AuthNetRecoveryCodesResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/mfa/recovery-codes/regenerate", async Task<IResult> (
            HttpContext httpContext,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.RegenerateRecoveryCodesAsync(httpContext, cancellationToken);
            return ToDataResult(result);
        })
            .WithName("AuthNetApiMfaRecoveryCodesRegenerate")
            .WithSummary("Regenerate recovery codes for the current MFA-enabled user.")
            .Produces<AuthNetRecoveryCodesRegenerateResponse>(StatusCodes.Status200OK)
            .Produces<AuthNetApiResult>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/login/mfa", async Task<IResult> (
            AuthNetMfaChallengeRequest request,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.LoginWithMfaAsync(request, cancellationToken);
            return ToMfaLoginResult(result);
        })
            .WithName("AuthNetApiLoginMfa")
            .WithSummary("Complete a pending sign-in with an authenticator code.")
            .Produces<AuthNetApiResult>(StatusCodes.Status200OK)
            .Produces<AuthNetApiResult>(StatusCodes.Status400BadRequest)
            .Produces<AuthNetApiResult>(StatusCodes.Status401Unauthorized)
            .Produces<AuthNetApiResult>(StatusCodes.Status409Conflict);

        group.MapPost("/login/recovery-code", async Task<IResult> (
            AuthNetRecoveryCodeLoginRequest request,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.LoginWithRecoveryCodeAsync(request, cancellationToken);
            return ToMfaLoginResult(result);
        })
            .WithName("AuthNetApiLoginRecoveryCode")
            .WithSummary("Complete a pending sign-in with a recovery code.")
            .Produces<AuthNetApiResult>(StatusCodes.Status200OK)
            .Produces<AuthNetApiResult>(StatusCodes.Status400BadRequest)
            .Produces<AuthNetApiResult>(StatusCodes.Status401Unauthorized)
            .Produces<AuthNetApiResult>(StatusCodes.Status409Conflict);

        group.MapGet("/external-providers", async Task<Ok<AuthNetExternalProvidersResponse>> (
            IAuthNetSpaAccountService accountService) =>
        {
            return TypedResults.Ok(await accountService.GetExternalProvidersAsync());
        })
            .WithName("AuthNetApiExternalProviders")
            .WithSummary("Get configured external login providers.")
            .Produces<AuthNetExternalProvidersResponse>(StatusCodes.Status200OK);

        group.MapPost("/external-login/challenge", async Task<IResult> (
            AuthNetExternalChallengeRequest request,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.PrepareExternalLoginChallengeAsync(
                request,
                options.NormalizedAccountRoutePrefix + "/api/external-login/callback",
                cancellationToken);
            return ToExternalChallengeResult(result);
        })
            .WithName("AuthNetApiExternalLoginChallenge")
            .WithSummary("Start an external login challenge.")
            .Produces(StatusCodes.Status302Found)
            .Produces<AuthNetApiResult>(StatusCodes.Status400BadRequest);

        group.MapGet("/external-login/callback", async Task<Ok<AuthNetExternalLoginCallbackResponse>> (
            HttpContext httpContext,
            string? returnUrl,
            string? remoteError,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            return TypedResults.Ok(await accountService.CompleteExternalLoginAsync(
                httpContext,
                returnUrl,
                remoteError,
                cancellationToken));
        })
            .WithName("AuthNetApiExternalLoginCallback")
            .WithSummary("Complete an external login callback.")
            .Produces<AuthNetExternalLoginCallbackResponse>(StatusCodes.Status200OK);

        group.MapPost("/external-login/link/challenge", async Task<IResult> (
            AuthNetExternalChallengeRequest request,
            HttpContext httpContext,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.PrepareExternalLinkChallengeAsync(
                request,
                httpContext,
                options.NormalizedAccountRoutePrefix + "/api/external-login/link/callback",
                cancellationToken);
            return result is null
                ? TypedResults.Unauthorized()
                : ToExternalChallengeResult(result);
        })
            .WithName("AuthNetApiExternalLoginLinkChallenge")
            .WithSummary("Start an external login link challenge for the current user.")
            .Produces(StatusCodes.Status302Found)
            .Produces<AuthNetApiResult>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/external-login/link/callback", async Task<IResult> (
            HttpContext httpContext,
            string? returnUrl,
            string? remoteError,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.CompleteExternalLinkAsync(
                httpContext,
                returnUrl,
                remoteError,
                cancellationToken);
            return result is null
                ? TypedResults.Unauthorized()
                : TypedResults.Ok(result);
        })
            .WithName("AuthNetApiExternalLoginLinkCallback")
            .WithSummary("Complete an external login link callback for the current user.")
            .Produces<AuthNetExternalLinkCallbackResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapGet("/invitations/accept", async Task<IResult> (
            string? token,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.GetInvitationAcceptanceStatusAsync(token, cancellationToken);
            return result.Result.Succeeded
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result);
        })
            .WithName("AuthNetApiInvitationAcceptanceStatus")
            .WithSummary("Inspect an account invitation token before accepting it.")
            .Produces<AuthNetInvitationAcceptanceStatusResponse>(StatusCodes.Status200OK)
            .Produces<AuthNetInvitationAcceptanceStatusResponse>(StatusCodes.Status400BadRequest);

        group.MapPost("/invitations/accept", async Task<IResult> (
            AuthNetAcceptInvitationRequest request,
            IAuthNetSpaAccountService accountService,
            CancellationToken cancellationToken) =>
        {
            var result = await accountService.AcceptInvitationAsync(request, cancellationToken);
            return result.Result.Succeeded
                ? TypedResults.Ok(result)
                : TypedResults.BadRequest(result);
        })
            .WithName("AuthNetApiAcceptInvitation")
            .WithSummary("Accept an account invitation with local credentials.")
            .Produces<AuthNetAcceptInvitationResponse>(StatusCodes.Status200OK)
            .Produces<AuthNetAcceptInvitationResponse>(StatusCodes.Status400BadRequest);

        return group;
    }

    private static IResult ToExternalChallengeResult(AuthNetExternalChallengeResult result)
    {
        if (!result.Result.Succeeded)
        {
            return TypedResults.BadRequest(result.Result);
        }

        return Results.Challenge(
            result.Properties,
            [result.Provider!]);
    }

    private static IResult ToDataResult<T>(AuthNetApiResponse<T>? result)
    {
        if (result is null)
        {
            return TypedResults.Unauthorized();
        }

        return result.Result.Succeeded
            ? TypedResults.Ok(result.Value)
            : TypedResults.BadRequest(result.Result);
    }

    private static IResult ToWriteResult(AuthNetApiResult result)
    {
        return result.Succeeded
            ? TypedResults.Ok(result)
            : TypedResults.BadRequest(result);
    }

    private static IResult ToLoginResult(AuthNetApiResult result)
    {
        if (result.Succeeded)
        {
            return TypedResults.Ok(result);
        }

        if (result.Errors.Any(error => error.Code == "InvalidCredentials"))
        {
            return Results.Json(result, statusCode: StatusCodes.Status401Unauthorized);
        }

        if (result.Errors.Any(error => error.Code is "LockedOut" or "NotAllowed" or "RequiresTwoFactor"))
        {
            return TypedResults.Conflict(result);
        }

        return TypedResults.BadRequest(result);
    }

    private static IResult ToMfaLoginResult(AuthNetApiResult result)
    {
        if (result.Succeeded)
        {
            return TypedResults.Ok(result);
        }

        if (result.Errors.Any(error => error.Code is "NoTwoFactorChallenge" or "InvalidAuthenticatorCode" or "InvalidRecoveryCode"))
        {
            return Results.Json(result, statusCode: StatusCodes.Status401Unauthorized);
        }

        if (result.Errors.Any(error => error.Code == "LockedOut"))
        {
            return TypedResults.Conflict(result);
        }

        return TypedResults.BadRequest(result);
    }
}
