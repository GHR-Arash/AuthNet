using System.Net;
using Microsoft.Net.Http.Headers;

namespace AuthNet.Tests.Integration;

internal static class AuthNetTestHtml
{
    public static FormUrlEncodedContent Form(params (string Name, string Value)[] values)
    {
        return new FormUrlEncodedContent(values.Select(value =>
            new KeyValuePair<string, string>(value.Name, value.Value)));
    }

    public static string GetRequestVerificationToken(string html)
    {
        const string marker = "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"";
        var start = html.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0)
        {
            throw new InvalidOperationException("The response did not contain an antiforgery token.");
        }

        start += marker.Length;
        var end = html.IndexOf('"', start);
        if (end < 0)
        {
            throw new InvalidOperationException("The antiforgery token value was not terminated.");
        }

        return WebUtility.HtmlDecode(html[start..end]);
    }

    public static string? GetFirstLinkContaining(string html, string text)
    {
        var textIndex = html.IndexOf(text, StringComparison.OrdinalIgnoreCase);
        if (textIndex < 0)
        {
            return null;
        }

        var hrefIndex = html.LastIndexOf("href=\"", textIndex, StringComparison.OrdinalIgnoreCase);
        if (hrefIndex < 0)
        {
            return null;
        }

        hrefIndex += "href=\"".Length;
        var hrefEnd = html.IndexOf('"', hrefIndex);
        return hrefEnd < 0 ? null : WebUtility.HtmlDecode(html[hrefIndex..hrefEnd]);
    }

    public static IReadOnlyDictionary<string, string> GetCookies(HttpResponseMessage response)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var values))
        {
            return new Dictionary<string, string>();
        }

        return values
            .Select(value =>
            {
                var cookie = SetCookieHeaderValue.Parse(value);
                return new KeyValuePair<string, string>(cookie.Name.Value!, cookie.Value.Value!);
            })
            .ToDictionary(cookie => cookie.Key, cookie => cookie.Value, StringComparer.Ordinal);
    }
}
