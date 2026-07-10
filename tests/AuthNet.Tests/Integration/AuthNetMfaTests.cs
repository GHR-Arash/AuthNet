using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using AuthNet.Persistence.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.Tests.Integration;

public sealed partial class AuthNetMfaTests
{
    [Fact]
    public async Task Authenticated_user_can_view_mfa_management()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("mfa.manage@example.test");
        await host.SignInAsync("mfa.manage@example.test");

        var response = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/mfa"));

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        Assert.Contains("Multi-factor authentication", body);
        Assert.Contains("Disabled", body);
    }

    [Fact]
    public async Task Authenticated_user_can_enable_mfa_and_receive_recovery_codes()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("mfa.setup@example.test");
        await host.SignInAsync("mfa.setup@example.test");

        var form = await host.GetFormAsync("/auth/mfa/setup");
        var token = await GenerateAuthenticatorTokenAsync(host, user.Id);
        var response = await host.PostFormAsync("/auth/mfa/setup", form, ("Input.Code", token));

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.OK, body);
        Assert.Contains("Multi-factor authentication is enabled.", body);
        Assert.Equal(10, RecoveryCodeRegex().Matches(body).Count);
        await AssertMfaEnabledAsync(host, user.Id, expected: true);
    }

    [Fact]
    public async Task Mfa_enabled_user_completes_password_sign_in_with_authenticator_code()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("mfa.login@example.test");
        await EnableMfaAsync(host, user.Id);

        var loginResponse = await host.SignInAsync("mfa.login@example.test");

        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
        Assert.StartsWith("/auth/login/mfa", loginResponse.Headers.Location?.OriginalString);

        var form = await host.GetFormAsync("/auth/login/mfa");
        var token = await GenerateAuthenticatorTokenAsync(host, user.Id);
        var mfaResponse = await host.PostFormAsync("/auth/login/mfa", form, ("Input.Code", token));

        Assert.Equal(HttpStatusCode.Redirect, mfaResponse.StatusCode);
        Assert.Equal("/", mfaResponse.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task Mfa_enabled_user_can_sign_in_with_recovery_code()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("mfa.recovery@example.test");
        var recoveryCodes = await EnableMfaAsync(host, user.Id);

        await host.SignInAsync("mfa.recovery@example.test");
        var form = await host.GetFormAsync("/auth/login/recovery-code");
        var response = await host.PostFormAsync(
            "/auth/login/recovery-code",
            form,
            ("Input.RecoveryCode", recoveryCodes[0]));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/", response.Headers.Location?.OriginalString);
        await AssertRecoveryCodesLeftAsync(host, user.Id, expected: 9);
    }

    [Fact]
    public async Task User_can_disable_mfa_and_return_to_password_only_sign_in()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("mfa.disable@example.test");
        await EnableMfaAsync(host, user.Id);
        await CompleteMfaSignInAsync(host, user.Id, "mfa.disable@example.test");

        var form = await host.GetFormAsync("/auth/mfa/disable");
        var disableResponse = await host.PostFormAsync("/auth/mfa/disable", form);

        Assert.Equal(HttpStatusCode.Redirect, disableResponse.StatusCode);
        await AssertMfaEnabledAsync(host, user.Id, expected: false);

        host.ClearCookies();
        var loginResponse = await host.SignInAsync("mfa.disable@example.test");

        Assert.Equal(HttpStatusCode.Redirect, loginResponse.StatusCode);
        Assert.Equal("/", loginResponse.Headers.Location?.OriginalString);
    }

    private static async Task CompleteMfaSignInAsync(AuthNetTestHost host, string userId, string email)
    {
        await host.SignInAsync(email);
        var form = await host.GetFormAsync("/auth/login/mfa");
        var token = await GenerateAuthenticatorTokenAsync(host, userId);
        var response = await host.PostFormAsync("/auth/login/mfa", form, ("Input.Code", token));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private static async Task<IReadOnlyList<string>> EnableMfaAsync(AuthNetTestHost host, string userId)
    {
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByIdAsync(userId);
        Assert.NotNull(user);

        if (string.IsNullOrWhiteSpace(await userManager.GetAuthenticatorKeyAsync(user)))
        {
            var resetResult = await userManager.ResetAuthenticatorKeyAsync(user);
            Assert.True(resetResult.Succeeded);
        }

        var enableResult = await userManager.SetTwoFactorEnabledAsync(user, true);
        Assert.True(enableResult.Succeeded);

        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        return [.. recoveryCodes ?? []];
    }

    private static async Task<string> GenerateAuthenticatorTokenAsync(AuthNetTestHost host, string userId)
    {
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByIdAsync(userId);
        Assert.NotNull(user);

        if (string.IsNullOrWhiteSpace(await userManager.GetAuthenticatorKeyAsync(user)))
        {
            var resetResult = await userManager.ResetAuthenticatorKeyAsync(user);
            Assert.True(resetResult.Succeeded);
        }

        var key = await userManager.GetAuthenticatorKeyAsync(user);
        Assert.False(string.IsNullOrWhiteSpace(key));

        return GenerateTotp(key);
    }

    private static async Task AssertMfaEnabledAsync(AuthNetTestHost host, string userId, bool expected)
    {
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByIdAsync(userId);
        Assert.NotNull(user);
        Assert.Equal(expected, await userManager.GetTwoFactorEnabledAsync(user));
    }

    private static async Task AssertRecoveryCodesLeftAsync(AuthNetTestHost host, string userId, int expected)
    {
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByIdAsync(userId);
        Assert.NotNull(user);
        Assert.Equal(expected, await userManager.CountRecoveryCodesAsync(user));
    }

    [GeneratedRegex("<code>[^<]+</code>")]
    private static partial Regex RecoveryCodeRegex();

    private static string GenerateTotp(string base32Key)
    {
        var key = Base32Decode(base32Key);
        var timestep = BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timestep);
        }

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(timestep);
        var offset = hash[^1] & 0x0f;
        var binaryCode =
            ((hash[offset] & 0x7f) << 24)
            | ((hash[offset + 1] & 0xff) << 16)
            | ((hash[offset + 2] & 0xff) << 8)
            | (hash[offset + 3] & 0xff);

        return (binaryCode % 1_000_000).ToString("D6");
    }

    private static byte[] Base32Decode(string value)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var bits = 0;
        var bitCount = 0;
        var bytes = new List<byte>();

        foreach (var character in value.Where(character => !char.IsWhiteSpace(character)))
        {
            var index = alphabet.IndexOf(char.ToUpperInvariant(character));
            if (index < 0)
            {
                throw new InvalidOperationException($"Invalid base32 character '{character}'.");
            }

            bits = (bits << 5) | index;
            bitCount += 5;

            if (bitCount >= 8)
            {
                bytes.Add((byte)((bits >> (bitCount - 8)) & 0xff));
                bitCount -= 8;
            }
        }

        return [.. bytes];
    }
}
