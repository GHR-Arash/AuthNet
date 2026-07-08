using System.Text;
using AuthNet.Core.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace AuthNetRazor.Areas.AuthNet.Pages.Account;

internal static class AccountEmailMessages
{
    public static string EncodeToken(string token)
    {
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    }

    public static bool TryDecodeToken(string encodedToken, out string token)
    {
        try
        {
            token = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(encodedToken));
            return true;
        }
        catch (FormatException)
        {
            token = string.Empty;
            return false;
        }
    }

    public static string BuildConfirmEmailUrl(PageModel page, string userId, string token, string? changedEmail = null)
    {
        return page.Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new { area = "AuthNet", userId, code = EncodeToken(token), changedEmail },
            protocol: page.Request.Scheme)!;
    }

    public static AuthNetEmailMessage CreateConfirmEmailMessage(string email, string callbackUrl)
    {
        return new AuthNetEmailMessage(
            email,
            "Confirm your email",
            $"Confirm your account by <a href=\"{callbackUrl}\">clicking here</a>.",
            $"Confirm your account: {callbackUrl}");
    }

    public static AuthNetEmailMessage CreateChangeEmailMessage(string email, string callbackUrl)
    {
        return new AuthNetEmailMessage(
            email,
            "Confirm your new email",
            $"Confirm your new email by <a href=\"{callbackUrl}\">clicking here</a>.",
            $"Confirm your new email: {callbackUrl}");
    }

    public static string BuildAcceptInvitationUrl(PageModel page, string token)
    {
        return page.Url.Page(
            "/Account/AcceptInvitation",
            pageHandler: null,
            values: new { area = "AuthNet", token },
            protocol: page.Request.Scheme)!;
    }

    public static AuthNetEmailMessage CreateInvitationMessage(string email, string callbackUrl)
    {
        return new AuthNetEmailMessage(
            email,
            "You're invited to create an account",
            $"Create your account by <a href=\"{callbackUrl}\">clicking here</a>.",
            $"Create your account: {callbackUrl}");
    }
}
