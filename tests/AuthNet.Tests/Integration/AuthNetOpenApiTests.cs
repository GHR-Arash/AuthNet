using System.Net;
using System.Text.Json;

namespace AuthNet.Tests.Integration;

public sealed class AuthNetOpenApiTests
{
    [Fact]
    public async Task OpenApi_document_is_available_under_default_api_prefix()
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var response = await host.Client.GetAsync("/auth/api/openapi.json");
        var document = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.StartsWith("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("3.1.0", document.RootElement.GetProperty("openapi").GetString());
        Assert.Equal("AuthNet SPA API", document.RootElement.GetProperty("info").GetProperty("title").GetString());
    }

    [Fact]
    public async Task OpenApi_document_uses_custom_account_route_prefix()
    {
        await using var host = await AuthNetTestHost.CreateAsync(options =>
            options.AccountRoutePrefix = "/accounts");

        var response = await host.Client.GetAsync("/accounts/api/openapi.json");
        var document = await ReadJsonAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var paths = document.RootElement.GetProperty("paths");
        Assert.True(paths.TryGetProperty("/accounts/api/session", out _));
        Assert.False(paths.TryGetProperty("/auth/api/session", out _));
    }

    [Fact]
    public async Task OpenApi_document_includes_all_spa_api_paths_and_excludes_ui_routes()
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var document = await GetOpenApiDocumentAsync(host);
        var paths = document.RootElement.GetProperty("paths");

        AssertOperation(paths, "/auth/api/session", "get", "AuthNetApiSession");
        AssertOperation(paths, "/auth/api/profile", "get", "AuthNetApiProfile");
        AssertOperation(paths, "/auth/api/profile", "put", "AuthNetApiUpdateProfile");
        AssertOperation(paths, "/auth/api/login", "post", "AuthNetApiLogin");
        AssertOperation(paths, "/auth/api/logout", "post", "AuthNetApiLogout");
        AssertOperation(paths, "/auth/api/register", "post", "AuthNetApiRegister");
        AssertOperation(paths, "/auth/api/forgot-password", "post", "AuthNetApiForgotPassword");
        AssertOperation(paths, "/auth/api/reset-password", "post", "AuthNetApiResetPassword");
        AssertOperation(paths, "/auth/api/resend-confirmation", "post", "AuthNetApiResendConfirmation");
        AssertOperation(paths, "/auth/api/confirm-email", "post", "AuthNetApiConfirmEmail");
        AssertOperation(paths, "/auth/api/change-password", "post", "AuthNetApiChangePassword");
        AssertOperation(paths, "/auth/api/mfa", "get", "AuthNetApiMfaStatus");
        AssertOperation(paths, "/auth/api/mfa/setup/start", "post", "AuthNetApiMfaSetupStart");
        AssertOperation(paths, "/auth/api/mfa/setup/verify", "post", "AuthNetApiMfaSetupVerify");
        AssertOperation(paths, "/auth/api/mfa/disable", "post", "AuthNetApiMfaDisable");
        AssertOperation(paths, "/auth/api/mfa/recovery-codes", "get", "AuthNetApiMfaRecoveryCodes");
        AssertOperation(paths, "/auth/api/mfa/recovery-codes/regenerate", "post", "AuthNetApiMfaRecoveryCodesRegenerate");
        AssertOperation(paths, "/auth/api/login/mfa", "post", "AuthNetApiLoginMfa");
        AssertOperation(paths, "/auth/api/login/recovery-code", "post", "AuthNetApiLoginRecoveryCode");
        AssertOperation(paths, "/auth/api/external-providers", "get", "AuthNetApiExternalProviders");
        AssertOperation(paths, "/auth/api/external-login/challenge", "post", "AuthNetApiExternalLoginChallenge");
        AssertOperation(paths, "/auth/api/external-login/callback", "get", "AuthNetApiExternalLoginCallback");
        AssertOperation(paths, "/auth/api/external-login/link/challenge", "post", "AuthNetApiExternalLoginLinkChallenge");
        AssertOperation(paths, "/auth/api/external-login/link/callback", "get", "AuthNetApiExternalLoginLinkCallback");
        AssertOperation(paths, "/auth/api/invitations/accept", "get", "AuthNetApiInvitationAcceptanceStatus");
        AssertOperation(paths, "/auth/api/invitations/accept", "post", "AuthNetApiAcceptInvitation");
        Assert.False(paths.TryGetProperty("/auth/login", out _));
        Assert.False(paths.TryGetProperty("/auth/admin/users", out _));
        Assert.False(paths.TryGetProperty("/Spa", out _));
    }

    [Fact]
    public async Task OpenApi_document_includes_request_and_response_schemas()
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var document = await GetOpenApiDocumentAsync(host);
        var schemas = document.RootElement
            .GetProperty("components")
            .GetProperty("schemas");

        AssertSchema(schemas, "AuthNetApiResult");
        AssertSchema(schemas, "AuthNetApiError");
        AssertSchema(schemas, "AuthNetSessionResponse");
        AssertSchema(schemas, "AuthNetProfileResponse");
        AssertSchema(schemas, "AuthNetLoginRequest");
        AssertSchema(schemas, "AuthNetRegisterRequest");
        AssertSchema(schemas, "AuthNetForgotPasswordRequest");
        AssertSchema(schemas, "AuthNetResetPasswordRequest");
        AssertSchema(schemas, "AuthNetResendConfirmationRequest");
        AssertSchema(schemas, "AuthNetConfirmEmailRequest");
        AssertSchema(schemas, "AuthNetUpdateProfileRequest");
        AssertSchema(schemas, "AuthNetChangePasswordRequest");
        AssertSchema(schemas, "AuthNetMfaStatusResponse");
        AssertSchema(schemas, "AuthNetMfaSetupStartResponse");
        AssertSchema(schemas, "AuthNetMfaSetupVerifyRequest");
        AssertSchema(schemas, "AuthNetMfaSetupVerifyResponse");
        AssertSchema(schemas, "AuthNetRecoveryCodesResponse");
        AssertSchema(schemas, "AuthNetRecoveryCodesRegenerateResponse");
        AssertSchema(schemas, "AuthNetMfaChallengeRequest");
        AssertSchema(schemas, "AuthNetRecoveryCodeLoginRequest");
        AssertSchema(schemas, "AuthNetExternalProviderResponse");
        AssertSchema(schemas, "AuthNetExternalProvidersResponse");
        AssertSchema(schemas, "AuthNetExternalChallengeRequest");
        AssertSchema(schemas, "AuthNetExternalLoginCallbackResponse");
        AssertSchema(schemas, "AuthNetExternalLinkCallbackResponse");
        AssertSchema(schemas, "AuthNetInvitationAcceptanceStatusResponse");
        AssertSchema(schemas, "AuthNetAcceptInvitationRequest");
        AssertSchema(schemas, "AuthNetAcceptInvitationResponse");

        var loginRequestRef = document.RootElement
            .GetProperty("paths")
            .GetProperty("/auth/api/login")
            .GetProperty("post")
            .GetProperty("requestBody")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString();
        Assert.Equal("#/components/schemas/AuthNetLoginRequest", loginRequestRef);

        var profileUpdateRequestRef = document.RootElement
            .GetProperty("paths")
            .GetProperty("/auth/api/profile")
            .GetProperty("put")
            .GetProperty("requestBody")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString();
        Assert.Equal("#/components/schemas/AuthNetUpdateProfileRequest", profileUpdateRequestRef);

        var mfaChallengeRequestRef = document.RootElement
            .GetProperty("paths")
            .GetProperty("/auth/api/login/mfa")
            .GetProperty("post")
            .GetProperty("requestBody")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString();
        Assert.Equal("#/components/schemas/AuthNetMfaChallengeRequest", mfaChallengeRequestRef);

        var externalChallengeRequestRef = document.RootElement
            .GetProperty("paths")
            .GetProperty("/auth/api/external-login/challenge")
            .GetProperty("post")
            .GetProperty("requestBody")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString();
        Assert.Equal("#/components/schemas/AuthNetExternalChallengeRequest", externalChallengeRequestRef);

        var acceptInvitationRequestRef = document.RootElement
            .GetProperty("paths")
            .GetProperty("/auth/api/invitations/accept")
            .GetProperty("post")
            .GetProperty("requestBody")
            .GetProperty("content")
            .GetProperty("application/json")
            .GetProperty("schema")
            .GetProperty("$ref")
            .GetString();
        Assert.Equal("#/components/schemas/AuthNetAcceptInvitationRequest", acceptInvitationRequestRef);

        var invitationStatusTokenParameter = document.RootElement
            .GetProperty("paths")
            .GetProperty("/auth/api/invitations/accept")
            .GetProperty("get")
            .GetProperty("parameters")[0];
        Assert.Equal("token", invitationStatusTokenParameter.GetProperty("name").GetString());
        Assert.Equal("query", invitationStatusTokenParameter.GetProperty("in").GetString());
    }

    [Fact]
    public async Task OpenApi_document_describes_cookie_auth_without_bearer_auth()
    {
        await using var host = await AuthNetTestHost.CreateAsync();

        var document = await GetOpenApiDocumentAsync(host);
        var securitySchemes = document.RootElement
            .GetProperty("components")
            .GetProperty("securitySchemes");

        Assert.True(securitySchemes.TryGetProperty("AuthNetApplicationCookie", out var cookieScheme));
        Assert.Equal("apiKey", cookieScheme.GetProperty("type").GetString());
        Assert.Equal("cookie", cookieScheme.GetProperty("in").GetString());
        Assert.False(securitySchemes.TryGetProperty("Bearer", out _));

        var profileSecurity = document.RootElement
            .GetProperty("paths")
            .GetProperty("/auth/api/profile")
            .GetProperty("get")
            .GetProperty("security")[0];
        Assert.True(profileSecurity.TryGetProperty("AuthNetApplicationCookie", out _));

        var changePasswordSecurity = document.RootElement
            .GetProperty("paths")
            .GetProperty("/auth/api/change-password")
            .GetProperty("post")
            .GetProperty("security")[0];
        Assert.True(changePasswordSecurity.TryGetProperty("AuthNetApplicationCookie", out _));

        var mfaSecurity = document.RootElement
            .GetProperty("paths")
            .GetProperty("/auth/api/mfa/setup/verify")
            .GetProperty("post")
            .GetProperty("security")[0];
        Assert.True(mfaSecurity.TryGetProperty("AuthNetApplicationCookie", out _));

        var externalLinkSecurity = document.RootElement
            .GetProperty("paths")
            .GetProperty("/auth/api/external-login/link/challenge")
            .GetProperty("post")
            .GetProperty("security")[0];
        Assert.True(externalLinkSecurity.TryGetProperty("AuthNetApplicationCookie", out _));

        var invitationStatusOperation = document.RootElement
            .GetProperty("paths")
            .GetProperty("/auth/api/invitations/accept")
            .GetProperty("get");
        Assert.False(invitationStatusOperation.TryGetProperty("security", out _));

        var invitationAcceptOperation = document.RootElement
            .GetProperty("paths")
            .GetProperty("/auth/api/invitations/accept")
            .GetProperty("post");
        Assert.False(invitationAcceptOperation.TryGetProperty("security", out _));
    }

    private static async Task<JsonDocument> GetOpenApiDocumentAsync(AuthNetTestHost host)
    {
        var response = await host.Client.GetAsync("/auth/api/openapi.json");
        return await ReadJsonAsync(response);
    }

    private static async Task<JsonDocument> ReadJsonAsync(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.False(string.IsNullOrWhiteSpace(body));
        return JsonDocument.Parse(body);
    }

    private static void AssertOperation(
        JsonElement paths,
        string path,
        string method,
        string operationId)
    {
        Assert.True(paths.TryGetProperty(path, out var pathItem), $"Expected path '{path}' was not documented.");
        Assert.True(pathItem.TryGetProperty(method, out var operation), $"Expected method '{method}' on path '{path}' was not documented.");
        Assert.Equal(operationId, operation.GetProperty("operationId").GetString());
        Assert.True(operation.TryGetProperty("responses", out _));
    }

    private static void AssertSchema(JsonElement schemas, string schemaName)
    {
        Assert.True(schemas.TryGetProperty(schemaName, out _), $"Expected schema '{schemaName}' was not documented.");
    }
}
