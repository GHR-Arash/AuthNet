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

        return group;
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
}
