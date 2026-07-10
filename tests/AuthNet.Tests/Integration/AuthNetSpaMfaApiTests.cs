using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using AuthNet.Api;
using AuthNet.Persistence.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AuthNet.Tests.Integration;

public sealed class AuthNetSpaMfaApiTests
{
    [Fact]
    public async Task Mfa_status_and_setup_start_require_authentication_and_return_setup_data()
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var anonymousStatus = await host.Client.GetAsync("/auth/api/mfa");
        Assert.Equal(HttpStatusCode.Unauthorized, anonymousStatus.StatusCode);

        await host.CreateUserAsync("spa.mfa.status@example.test");
        await LoginAsync(host, "spa.mfa.status@example.test");

        var statusResponse = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/api/mfa"));
        Assert.Equal(HttpStatusCode.OK, statusResponse.StatusCode);
        var status = await statusResponse.Content.ReadFromJsonAsync<AuthNetMfaStatusResponse>();
        Assert.NotNull(status);
        Assert.False(status.IsMfaEnabled);
        Assert.False(status.HasAuthenticator);

        var setupResponse = await host.PostJsonAsync("/auth/api/mfa/setup/start", new { });
        Assert.Equal(HttpStatusCode.OK, setupResponse.StatusCode);
        var setup = await setupResponse.Content.ReadFromJsonAsync<AuthNetMfaSetupStartResponse>();
        Assert.NotNull(setup);
        Assert.False(string.IsNullOrWhiteSpace(setup.SharedKey));
        Assert.StartsWith("otpauth://totp/", setup.AuthenticatorUri, StringComparison.Ordinal);
        Assert.Contains("secret=", setup.AuthenticatorUri);
    }

    [Fact]
    public async Task Authenticated_user_can_enable_mfa_with_authenticator_code()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("spa.mfa.setup@example.test");
        await LoginAsync(host, "spa.mfa.setup@example.test");

        var setup = await StartSetupAsync(host);
        var invalidResponse = await host.PostJsonAsync("/auth/api/mfa/setup/verify", new AuthNetMfaSetupVerifyRequest
        {
            Code = "000000"
        });
        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);
        var invalid = await invalidResponse.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(invalid);
        Assert.Contains(invalid.Errors, error => error.Code == "InvalidAuthenticatorCode");

        var response = await host.PostJsonAsync("/auth/api/mfa/setup/verify", new AuthNetMfaSetupVerifyRequest
        {
            Code = GenerateTotp(setup.SharedKey)
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetMfaSetupVerifyResponse>();
        Assert.NotNull(result);
        Assert.True(result.IsMfaEnabled);
        Assert.Equal(10, result.RecoveryCodes.Count);

        var statusResponse = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/api/mfa"));
        var status = await statusResponse.Content.ReadFromJsonAsync<AuthNetMfaStatusResponse>();
        Assert.NotNull(status);
        Assert.True(status.IsMfaEnabled);
        Assert.True(status.HasAuthenticator);
        Assert.Equal(10, status.RecoveryCodesLeft);
    }

    [Fact]
    public async Task Recovery_codes_can_be_counted_regenerated_and_mfa_can_be_disabled()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        await host.CreateUserAsync("spa.mfa.recovery.manage@example.test");
        await LoginAsync(host, "spa.mfa.recovery.manage@example.test");
        await EnableMfaThroughApiAsync(host);

        var countResponse = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/api/mfa/recovery-codes"));
        Assert.Equal(HttpStatusCode.OK, countResponse.StatusCode);
        var count = await countResponse.Content.ReadFromJsonAsync<AuthNetRecoveryCodesResponse>();
        Assert.NotNull(count);
        Assert.Equal(10, count.RecoveryCodesLeft);

        var regenerateResponse = await host.PostJsonAsync("/auth/api/mfa/recovery-codes/regenerate", new { });
        Assert.Equal(HttpStatusCode.OK, regenerateResponse.StatusCode);
        var regenerated = await regenerateResponse.Content.ReadFromJsonAsync<AuthNetRecoveryCodesRegenerateResponse>();
        Assert.NotNull(regenerated);
        Assert.Equal(10, regenerated.RecoveryCodes.Count);

        var disableResponse = await host.PostJsonAsync("/auth/api/mfa/disable", new { });
        Assert.Equal(HttpStatusCode.OK, disableResponse.StatusCode);

        var disabledStatusResponse = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/api/mfa"));
        var disabledStatus = await disabledStatusResponse.Content.ReadFromJsonAsync<AuthNetMfaStatusResponse>();
        Assert.NotNull(disabledStatus);
        Assert.False(disabledStatus.IsMfaEnabled);

        var disabledRegenerateResponse = await host.PostJsonAsync("/auth/api/mfa/recovery-codes/regenerate", new { });
        Assert.Equal(HttpStatusCode.BadRequest, disabledRegenerateResponse.StatusCode);
        var disabledRegenerate = await disabledRegenerateResponse.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(disabledRegenerate);
        Assert.Contains(disabledRegenerate.Errors, error => error.Code == "MfaNotEnabled");
    }

    [Fact]
    public async Task Mfa_enabled_user_completes_password_sign_in_with_json_authenticator_code()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("spa.mfa.login@example.test");
        await EnableMfaWithIdentityAsync(host, user.Id);

        var loginResponse = await host.PostJsonAsync("/auth/api/login", new AuthNetLoginRequest
        {
            Identifier = "spa.mfa.login@example.test",
            Password = "Password1!"
        });
        Assert.Equal(HttpStatusCode.Conflict, loginResponse.StatusCode);
        var login = await loginResponse.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(login);
        Assert.Contains(login.Errors, error => error.Code == "RequiresTwoFactor");

        var response = await host.PostJsonAsync("/auth/api/login/mfa", new AuthNetMfaChallengeRequest
        {
            Code = await GenerateAuthenticatorTokenAsync(host, user.Id)
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var sessionResponse = await host.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/auth/api/session"));
        var session = await sessionResponse.Content.ReadFromJsonAsync<AuthNetSessionResponse>();
        Assert.NotNull(session);
        Assert.True(session.IsAuthenticated);
        Assert.Equal("spa.mfa.login@example.test", session.Email);
    }

    [Fact]
    public async Task Mfa_login_challenge_rejects_missing_or_invalid_challenge()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("spa.mfa.invalid@example.test");
        await EnableMfaWithIdentityAsync(host, user.Id);

        var missingChallenge = await host.PostJsonAsync("/auth/api/login/mfa", new AuthNetMfaChallengeRequest
        {
            Code = "000000"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, missingChallenge.StatusCode);

        var loginResponse = await host.PostJsonAsync("/auth/api/login", new AuthNetLoginRequest
        {
            Identifier = "spa.mfa.invalid@example.test",
            Password = "Password1!"
        });
        Assert.Equal(HttpStatusCode.Conflict, loginResponse.StatusCode);

        var invalidCode = await host.PostJsonAsync("/auth/api/login/mfa", new AuthNetMfaChallengeRequest
        {
            Code = "000000"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, invalidCode.StatusCode);
        var result = await invalidCode.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(result);
        Assert.Contains(result.Errors, error => error.Code == "InvalidAuthenticatorCode");
    }

    [Fact]
    public async Task Mfa_enabled_user_completes_password_sign_in_with_json_recovery_code()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("spa.mfa.recovery.login@example.test");
        var recoveryCodes = await EnableMfaWithIdentityAsync(host, user.Id);

        var loginResponse = await host.PostJsonAsync("/auth/api/login", new AuthNetLoginRequest
        {
            Identifier = "spa.mfa.recovery.login@example.test",
            Password = "Password1!"
        });
        Assert.Equal(HttpStatusCode.Conflict, loginResponse.StatusCode);

        var response = await host.PostJsonAsync("/auth/api/login/recovery-code", new AuthNetRecoveryCodeLoginRequest
        {
            RecoveryCode = recoveryCodes[0]
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        await AssertRecoveryCodesLeftAsync(host, user.Id, expected: 9);
    }

    [Fact]
    public async Task Recovery_code_login_rejects_missing_or_invalid_challenge()
    {
        await using var host = await AuthNetTestHost.CreateAsync();
        var user = await host.CreateUserAsync("spa.mfa.recovery.invalid@example.test");
        await EnableMfaWithIdentityAsync(host, user.Id);

        var missingChallenge = await host.PostJsonAsync("/auth/api/login/recovery-code", new AuthNetRecoveryCodeLoginRequest
        {
            RecoveryCode = "not-a-code"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, missingChallenge.StatusCode);

        var loginResponse = await host.PostJsonAsync("/auth/api/login", new AuthNetLoginRequest
        {
            Identifier = "spa.mfa.recovery.invalid@example.test",
            Password = "Password1!"
        });
        Assert.Equal(HttpStatusCode.Conflict, loginResponse.StatusCode);

        var invalidCode = await host.PostJsonAsync("/auth/api/login/recovery-code", new AuthNetRecoveryCodeLoginRequest
        {
            RecoveryCode = "not-a-code"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, invalidCode.StatusCode);
        var result = await invalidCode.Content.ReadFromJsonAsync<AuthNetApiResult>();
        Assert.NotNull(result);
        Assert.Contains(result.Errors, error => error.Code == "InvalidRecoveryCode");
    }

    private static async Task LoginAsync(AuthNetTestHost host, string email)
    {
        var response = await host.PostJsonAsync("/auth/api/login", new AuthNetLoginRequest
        {
            Identifier = email,
            Password = "Password1!"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static async Task<AuthNetMfaSetupStartResponse> StartSetupAsync(AuthNetTestHost host)
    {
        var response = await host.PostJsonAsync("/auth/api/mfa/setup/start", new { });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var setup = await response.Content.ReadFromJsonAsync<AuthNetMfaSetupStartResponse>();
        Assert.NotNull(setup);
        return setup;
    }

    private static async Task<IReadOnlyList<string>> EnableMfaThroughApiAsync(AuthNetTestHost host)
    {
        var setup = await StartSetupAsync(host);
        var response = await host.PostJsonAsync("/auth/api/mfa/setup/verify", new AuthNetMfaSetupVerifyRequest
        {
            Code = GenerateTotp(setup.SharedKey)
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthNetMfaSetupVerifyResponse>();
        Assert.NotNull(result);
        return result.RecoveryCodes;
    }

    private static async Task<IReadOnlyList<string>> EnableMfaWithIdentityAsync(AuthNetTestHost host, string userId)
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
        var key = await userManager.GetAuthenticatorKeyAsync(user);
        Assert.False(string.IsNullOrWhiteSpace(key));
        return GenerateTotp(key);
    }

    private static async Task AssertRecoveryCodesLeftAsync(AuthNetTestHost host, string userId, int expected)
    {
        using var scope = host.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthNetUser>>();
        var user = await userManager.FindByIdAsync(userId);
        Assert.NotNull(user);
        Assert.Equal(expected, await userManager.CountRecoveryCodesAsync(user));
    }

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
