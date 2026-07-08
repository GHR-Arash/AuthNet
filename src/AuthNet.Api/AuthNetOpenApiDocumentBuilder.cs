using AuthNet.Core;

namespace AuthNet.Api;

internal static class AuthNetOpenApiDocumentBuilder
{
    private const string JsonContentType = "application/json";
    private const string CookieSecurityScheme = "AuthNetApplicationCookie";

    public static IReadOnlyDictionary<string, object?> Build(AuthNetOptions options)
    {
        var apiRoot = options.NormalizedAccountRoutePrefix + "/api";
        return new Dictionary<string, object?>
        {
            ["openapi"] = "3.1.0",
            ["info"] = new Dictionary<string, object?>
            {
                ["title"] = "AuthNet SPA API",
                ["version"] = "0.1.0",
                ["description"] = "Same-origin JSON account endpoints backed by the AuthNet application cookie."
            },
            ["paths"] = BuildPaths(apiRoot),
            ["components"] = new Dictionary<string, object?>
            {
                ["securitySchemes"] = new Dictionary<string, object?>
                {
                    [CookieSecurityScheme] = new Dictionary<string, object?>
                    {
                        ["type"] = "apiKey",
                        ["in"] = "cookie",
                        ["name"] = ".AspNetCore.Identity.Application",
                        ["description"] = "ASP.NET Core Identity application cookie issued by AuthNet sign-in endpoints."
                    }
                },
                ["schemas"] = BuildSchemas()
            }
        };
    }

    private static IReadOnlyDictionary<string, object?> BuildPaths(string apiRoot)
    {
        return new Dictionary<string, object?>
        {
            [$"{apiRoot}/session"] = new Dictionary<string, object?>
            {
                ["get"] = Operation(
                    "AuthNetApiSession",
                    "Get the current AuthNet browser session.",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("Current session state.", "AuthNetSessionResponse")
                    })
            },
            [$"{apiRoot}/profile"] = new Dictionary<string, object?>
            {
                ["get"] = Operation(
                    "AuthNetApiProfile",
                    "Get the current authenticated user's profile.",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("Current user profile.", "AuthNetProfileResponse"),
                        ["401"] = Response("Authentication is required.")
                    },
                    requiresCookie: true)
            },
            [$"{apiRoot}/login"] = new Dictionary<string, object?>
            {
                ["post"] = Operation(
                    "AuthNetApiLogin",
                    "Sign in with a local password credential.",
                    requestSchema: "AuthNetLoginRequest",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("Signed in.", "AuthNetApiResult"),
                        ["400"] = JsonResponse("Validation failed.", "AuthNetApiResult"),
                        ["401"] = JsonResponse("Invalid credentials.", "AuthNetApiResult"),
                        ["409"] = JsonResponse("Account cannot complete sign-in.", "AuthNetApiResult")
                    })
            },
            [$"{apiRoot}/logout"] = new Dictionary<string, object?>
            {
                ["post"] = Operation(
                    "AuthNetApiLogout",
                    "Sign out of the current AuthNet browser session.",
                    requestSchema: null,
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("Signed out.", "AuthNetApiResult")
                    },
                    requiresCookie: true)
            },
            [$"{apiRoot}/register"] = new Dictionary<string, object?>
            {
                ["post"] = Operation(
                    "AuthNetApiRegister",
                    "Register a local AuthNet account.",
                    requestSchema: "AuthNetRegisterRequest",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("Account created.", "AuthNetApiResult"),
                        ["400"] = JsonResponse("Registration failed.", "AuthNetApiResult")
                    })
            },
            [$"{apiRoot}/forgot-password"] = new Dictionary<string, object?>
            {
                ["post"] = Operation(
                    "AuthNetApiForgotPassword",
                    "Send password reset instructions when the account can receive them.",
                    requestSchema: "AuthNetForgotPasswordRequest",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("Password reset instructions response.", "AuthNetApiResult"),
                        ["400"] = JsonResponse("Validation failed.", "AuthNetApiResult")
                    })
            },
            [$"{apiRoot}/resend-confirmation"] = new Dictionary<string, object?>
            {
                ["post"] = Operation(
                    "AuthNetApiResendConfirmation",
                    "Send email confirmation instructions when the account needs them.",
                    requestSchema: "AuthNetResendConfirmationRequest",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("Confirmation instructions response.", "AuthNetApiResult"),
                        ["400"] = JsonResponse("Validation failed.", "AuthNetApiResult")
                    })
            }
        };
    }

    private static IReadOnlyDictionary<string, object?> Operation(
        string operationId,
        string summary,
        IReadOnlyDictionary<string, object?> responses,
        string? requestSchema = null,
        bool requiresCookie = false)
    {
        var operation = new Dictionary<string, object?>
        {
            ["operationId"] = operationId,
            ["summary"] = summary,
            ["tags"] = new[] { "AuthNet SPA" },
            ["responses"] = responses
        };

        if (requestSchema is not null)
        {
            operation["requestBody"] = new Dictionary<string, object?>
            {
                ["required"] = true,
                ["content"] = new Dictionary<string, object?>
                {
                    [JsonContentType] = SchemaContent(requestSchema)
                }
            };
        }

        if (requiresCookie)
        {
            operation["security"] = new[]
            {
                new Dictionary<string, IReadOnlyList<string>>
                {
                    [CookieSecurityScheme] = []
                }
            };
        }

        return operation;
    }

    private static IReadOnlyDictionary<string, object?> Response(string description)
    {
        return new Dictionary<string, object?>
        {
            ["description"] = description
        };
    }

    private static IReadOnlyDictionary<string, object?> JsonResponse(string description, string schemaName)
    {
        return new Dictionary<string, object?>
        {
            ["description"] = description,
            ["content"] = new Dictionary<string, object?>
            {
                [JsonContentType] = SchemaContent(schemaName)
            }
        };
    }

    private static IReadOnlyDictionary<string, object?> SchemaContent(string schemaName)
    {
        return new Dictionary<string, object?>
        {
            ["schema"] = Ref(schemaName)
        };
    }

    private static IReadOnlyDictionary<string, object?> Ref(string schemaName)
    {
        return new Dictionary<string, object?>
        {
            ["$ref"] = $"#/components/schemas/{schemaName}"
        };
    }

    private static IReadOnlyDictionary<string, object?> BuildSchemas()
    {
        return new Dictionary<string, object?>
        {
            ["AuthNetApiResult"] = ObjectSchema(
                required: ["succeeded", "message", "errors"],
                properties: new Dictionary<string, object?>
                {
                    ["succeeded"] = BooleanSchema("Whether the operation succeeded."),
                    ["message"] = StringSchema("Human-readable operation message."),
                    ["errors"] = ArraySchema(Ref("AuthNetApiError"), "Field-addressable operation errors.")
                }),
            ["AuthNetApiError"] = ObjectSchema(
                required: ["code", "description"],
                properties: new Dictionary<string, object?>
                {
                    ["code"] = StringSchema("Stable error code."),
                    ["field"] = NullableStringSchema("Request field associated with the error, when applicable."),
                    ["description"] = StringSchema("Human-readable error description.")
                }),
            ["AuthNetSessionResponse"] = ObjectSchema(
                required: ["isAuthenticated", "roles"],
                properties: new Dictionary<string, object?>
                {
                    ["isAuthenticated"] = BooleanSchema("Whether the browser session is authenticated."),
                    ["userId"] = NullableStringSchema("Authenticated user identifier."),
                    ["email"] = NullableStringSchema("Authenticated user's email address."),
                    ["userName"] = NullableStringSchema("Authenticated user's username."),
                    ["displayName"] = NullableStringSchema("Authenticated user's display name."),
                    ["roles"] = StringArraySchema("Role names assigned to the authenticated user.")
                }),
            ["AuthNetProfileResponse"] = ObjectSchema(
                required: ["userId", "email", "userName", "emailConfirmed", "mfaEnabled", "roles"],
                properties: new Dictionary<string, object?>
                {
                    ["userId"] = StringSchema("Authenticated user identifier."),
                    ["email"] = StringSchema("Authenticated user's email address."),
                    ["userName"] = StringSchema("Authenticated user's username."),
                    ["displayName"] = NullableStringSchema("Authenticated user's display name."),
                    ["phoneNumber"] = NullableStringSchema("Authenticated user's phone number."),
                    ["emailConfirmed"] = BooleanSchema("Whether the user's email is confirmed."),
                    ["mfaEnabled"] = BooleanSchema("Whether authenticator-app MFA is enabled."),
                    ["roles"] = StringArraySchema("Role names assigned to the authenticated user.")
                }),
            ["AuthNetLoginRequest"] = ObjectSchema(
                required: ["identifier", "password"],
                properties: new Dictionary<string, object?>
                {
                    ["identifier"] = StringSchema("Email address or username."),
                    ["password"] = StringSchema("Local account password.", format: "password"),
                    ["rememberMe"] = BooleanSchema("Whether the sign-in cookie should be persistent.")
                }),
            ["AuthNetRegisterRequest"] = ObjectSchema(
                required: ["email", "password"],
                properties: new Dictionary<string, object?>
                {
                    ["email"] = StringSchema("Email address for the new account.", format: "email"),
                    ["displayName"] = NullableStringSchema("Optional display name.", maxLength: 200),
                    ["password"] = StringSchema("Local account password.", format: "password")
                }),
            ["AuthNetForgotPasswordRequest"] = ObjectSchema(
                required: ["email"],
                properties: new Dictionary<string, object?>
                {
                    ["email"] = StringSchema("Email address for password recovery.", format: "email")
                }),
            ["AuthNetResendConfirmationRequest"] = ObjectSchema(
                required: ["email"],
                properties: new Dictionary<string, object?>
                {
                    ["email"] = StringSchema("Email address that needs confirmation.", format: "email")
                })
        };
    }

    private static IReadOnlyDictionary<string, object?> ObjectSchema(
        IReadOnlyList<string> required,
        IReadOnlyDictionary<string, object?> properties)
    {
        return new Dictionary<string, object?>
        {
            ["type"] = "object",
            ["required"] = required,
            ["additionalProperties"] = false,
            ["properties"] = properties
        };
    }

    private static IReadOnlyDictionary<string, object?> StringSchema(
        string description,
        string? format = null,
        int? maxLength = null)
    {
        var schema = new Dictionary<string, object?>
        {
            ["type"] = "string",
            ["description"] = description
        };

        if (format is not null)
        {
            schema["format"] = format;
        }

        if (maxLength is not null)
        {
            schema["maxLength"] = maxLength;
        }

        return schema;
    }

    private static IReadOnlyDictionary<string, object?> NullableStringSchema(
        string description,
        int? maxLength = null)
    {
        var schema = new Dictionary<string, object?>
        {
            ["type"] = new[] { "string", "null" },
            ["description"] = description
        };

        if (maxLength is not null)
        {
            schema["maxLength"] = maxLength;
        }

        return schema;
    }

    private static IReadOnlyDictionary<string, object?> BooleanSchema(string description)
    {
        return new Dictionary<string, object?>
        {
            ["type"] = "boolean",
            ["description"] = description
        };
    }

    private static IReadOnlyDictionary<string, object?> StringArraySchema(string description)
    {
        return ArraySchema(
            new Dictionary<string, object?>
            {
                ["type"] = "string"
            },
            description);
    }

    private static IReadOnlyDictionary<string, object?> ArraySchema(
        IReadOnlyDictionary<string, object?> items,
        string description)
    {
        return new Dictionary<string, object?>
        {
            ["type"] = "array",
            ["description"] = description,
            ["items"] = items
        };
    }
}
