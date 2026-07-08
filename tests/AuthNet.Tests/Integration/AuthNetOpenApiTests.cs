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
        AssertOperation(paths, "/auth/api/login", "post", "AuthNetApiLogin");
        AssertOperation(paths, "/auth/api/logout", "post", "AuthNetApiLogout");
        AssertOperation(paths, "/auth/api/register", "post", "AuthNetApiRegister");
        AssertOperation(paths, "/auth/api/forgot-password", "post", "AuthNetApiForgotPassword");
        AssertOperation(paths, "/auth/api/resend-confirmation", "post", "AuthNetApiResendConfirmation");
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
        AssertSchema(schemas, "AuthNetResendConfirmationRequest");

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
