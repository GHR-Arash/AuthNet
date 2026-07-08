using System.Text;
using AuthNet.Core.Email;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace AuthNet.Api;

internal static class AuthNetApiEmailMessages
{
    public static string BuildConfirmEmailUrl(HttpRequest request, string accountRoutePrefix, string userId, string token)
    {
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        return BuildUrl(request, accountRoutePrefix, "/confirm-email", $"userId={Uri.EscapeDataString(userId)}&code={Uri.EscapeDataString(code)}");
    }

    public static string BuildResetPasswordUrl(HttpRequest request, string accountRoutePrefix, string token)
    {
        var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        return BuildUrl(request, accountRoutePrefix, "/reset-password", $"code={Uri.EscapeDataString(code)}");
    }

    public static AuthNetEmailMessage CreateConfirmEmailMessage(string email, string callbackUrl)
    {
        return new AuthNetEmailMessage(
            email,
            "Confirm your email",
            $"Confirm your account by <a href=\"{callbackUrl}\">clicking here</a>.",
            $"Confirm your account: {callbackUrl}");
    }

    public static AuthNetEmailMessage CreateResetPasswordMessage(string email, string callbackUrl)
    {
        return new AuthNetEmailMessage(
            email,
            "Reset your password",
            $"Reset your password by <a href=\"{callbackUrl}\">clicking here</a>.",
            $"Reset your password: {callbackUrl}");
    }

    private static string BuildUrl(HttpRequest request, string accountRoutePrefix, string path, string query)
    {
        return $"{request.Scheme}://{request.Host}{request.PathBase}{accountRoutePrefix}{path}?{query}";
    }
}
