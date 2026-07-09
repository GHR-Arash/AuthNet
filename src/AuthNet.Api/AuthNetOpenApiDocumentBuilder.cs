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
                    requiresCookie: true),
                ["put"] = Operation(
                    "AuthNetApiUpdateProfile",
                    "Update the current authenticated user's profile.",
                    requestSchema: "AuthNetUpdateProfileRequest",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("Updated user profile.", "AuthNetProfileResponse"),
                        ["400"] = JsonResponse("Validation or profile update failed.", "AuthNetApiResult"),
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
            [$"{apiRoot}/reset-password"] = new Dictionary<string, object?>
            {
                ["post"] = Operation(
                    "AuthNetApiResetPassword",
                    "Complete password recovery with a reset code.",
                    requestSchema: "AuthNetResetPasswordRequest",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("Password reset.", "AuthNetApiResult"),
                        ["400"] = JsonResponse("Validation or password reset failed.", "AuthNetApiResult")
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
            },
            [$"{apiRoot}/confirm-email"] = new Dictionary<string, object?>
            {
                ["post"] = Operation(
                    "AuthNetApiConfirmEmail",
                    "Complete email confirmation with a confirmation code.",
                    requestSchema: "AuthNetConfirmEmailRequest",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("Email confirmed.", "AuthNetApiResult"),
                        ["400"] = JsonResponse("Validation or email confirmation failed.", "AuthNetApiResult")
                    })
            },
            [$"{apiRoot}/change-password"] = new Dictionary<string, object?>
            {
                ["post"] = Operation(
                    "AuthNetApiChangePassword",
                    "Change the current authenticated user's password.",
                    requestSchema: "AuthNetChangePasswordRequest",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("Password changed.", "AuthNetApiResult"),
                        ["400"] = JsonResponse("Validation or password change failed.", "AuthNetApiResult"),
                        ["401"] = Response("Authentication is required.")
                    },
                    requiresCookie: true)
            },
            [$"{apiRoot}/mfa"] = new Dictionary<string, object?>
            {
                ["get"] = Operation(
                    "AuthNetApiMfaStatus",
                    "Get the current authenticated user's MFA state.",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("Current MFA state.", "AuthNetMfaStatusResponse"),
                        ["401"] = Response("Authentication is required.")
                    },
                    requiresCookie: true)
            },
            [$"{apiRoot}/mfa/setup/start"] = new Dictionary<string, object?>
            {
                ["post"] = Operation(
                    "AuthNetApiMfaSetupStart",
                    "Start authenticator-app MFA setup for the current user.",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("Authenticator setup data.", "AuthNetMfaSetupStartResponse"),
                        ["401"] = Response("Authentication is required.")
                    },
                    requiresCookie: true)
            },
            [$"{apiRoot}/mfa/setup/verify"] = new Dictionary<string, object?>
            {
                ["post"] = Operation(
                    "AuthNetApiMfaSetupVerify",
                    "Verify an authenticator code and enable MFA for the current user.",
                    requestSchema: "AuthNetMfaSetupVerifyRequest",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("MFA setup completed.", "AuthNetMfaSetupVerifyResponse"),
                        ["400"] = JsonResponse("Validation or MFA setup failed.", "AuthNetApiResult"),
                        ["401"] = Response("Authentication is required.")
                    },
                    requiresCookie: true)
            },
            [$"{apiRoot}/mfa/disable"] = new Dictionary<string, object?>
            {
                ["post"] = Operation(
                    "AuthNetApiMfaDisable",
                    "Disable authenticator-app MFA for the current user.",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("MFA disabled.", "AuthNetApiResult"),
                        ["400"] = JsonResponse("MFA disable failed.", "AuthNetApiResult"),
                        ["401"] = Response("Authentication is required.")
                    },
                    requiresCookie: true)
            },
            [$"{apiRoot}/mfa/recovery-codes"] = new Dictionary<string, object?>
            {
                ["get"] = Operation(
                    "AuthNetApiMfaRecoveryCodes",
                    "Get the current user's recovery-code count.",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("Recovery-code count.", "AuthNetRecoveryCodesResponse"),
                        ["401"] = Response("Authentication is required.")
                    },
                    requiresCookie: true)
            },
            [$"{apiRoot}/mfa/recovery-codes/regenerate"] = new Dictionary<string, object?>
            {
                ["post"] = Operation(
                    "AuthNetApiMfaRecoveryCodesRegenerate",
                    "Regenerate recovery codes for the current MFA-enabled user.",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("New recovery codes.", "AuthNetRecoveryCodesRegenerateResponse"),
                        ["400"] = JsonResponse("Recovery-code regeneration failed.", "AuthNetApiResult"),
                        ["401"] = Response("Authentication is required.")
                    },
                    requiresCookie: true)
            },
            [$"{apiRoot}/login/mfa"] = new Dictionary<string, object?>
            {
                ["post"] = Operation(
                    "AuthNetApiLoginMfa",
                    "Complete a pending sign-in with an authenticator code.",
                    requestSchema: "AuthNetMfaChallengeRequest",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("Signed in.", "AuthNetApiResult"),
                        ["400"] = JsonResponse("Validation failed.", "AuthNetApiResult"),
                        ["401"] = JsonResponse("No pending challenge or invalid code.", "AuthNetApiResult"),
                        ["409"] = JsonResponse("Account cannot complete sign-in.", "AuthNetApiResult")
                    })
            },
            [$"{apiRoot}/login/recovery-code"] = new Dictionary<string, object?>
            {
                ["post"] = Operation(
                    "AuthNetApiLoginRecoveryCode",
                    "Complete a pending sign-in with a recovery code.",
                    requestSchema: "AuthNetRecoveryCodeLoginRequest",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("Signed in.", "AuthNetApiResult"),
                        ["400"] = JsonResponse("Validation failed.", "AuthNetApiResult"),
                        ["401"] = JsonResponse("No pending challenge or invalid recovery code.", "AuthNetApiResult"),
                        ["409"] = JsonResponse("Account cannot complete sign-in.", "AuthNetApiResult")
                    })
            },
            [$"{apiRoot}/external-providers"] = new Dictionary<string, object?>
            {
                ["get"] = Operation(
                    "AuthNetApiExternalProviders",
                    "Get configured external login providers.",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("Configured external providers.", "AuthNetExternalProvidersResponse")
                    })
            },
            [$"{apiRoot}/external-login/challenge"] = new Dictionary<string, object?>
            {
                ["post"] = Operation(
                    "AuthNetApiExternalLoginChallenge",
                    "Start an external login challenge.",
                    requestSchema: "AuthNetExternalChallengeRequest",
                    responses: new Dictionary<string, object?>
                    {
                        ["302"] = Response("Redirects to the selected external provider."),
                        ["400"] = JsonResponse("Validation or provider selection failed.", "AuthNetApiResult")
                    })
            },
            [$"{apiRoot}/external-login/callback"] = new Dictionary<string, object?>
            {
                ["get"] = Operation(
                    "AuthNetApiExternalLoginCallback",
                    "Complete an external login callback.",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("External login callback result.", "AuthNetExternalLoginCallbackResponse")
                    })
            },
            [$"{apiRoot}/external-login/link/challenge"] = new Dictionary<string, object?>
            {
                ["post"] = Operation(
                    "AuthNetApiExternalLoginLinkChallenge",
                    "Start an external login link challenge for the current user.",
                    requestSchema: "AuthNetExternalChallengeRequest",
                    responses: new Dictionary<string, object?>
                    {
                        ["302"] = Response("Redirects to the selected external provider."),
                        ["400"] = JsonResponse("Validation or provider selection failed.", "AuthNetApiResult"),
                        ["401"] = Response("Authentication is required.")
                    },
                    requiresCookie: true)
            },
            [$"{apiRoot}/external-login/link/callback"] = new Dictionary<string, object?>
            {
                ["get"] = Operation(
                    "AuthNetApiExternalLoginLinkCallback",
                    "Complete an external login link callback for the current user.",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("External login link callback result.", "AuthNetExternalLinkCallbackResponse"),
                        ["401"] = Response("Authentication is required.")
                    },
                    requiresCookie: true)
            },
            [$"{apiRoot}/invitations/accept"] = new Dictionary<string, object?>
            {
                ["get"] = Operation(
                    "AuthNetApiInvitationAcceptanceStatus",
                    "Inspect an account invitation token before accepting it.",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("Invitation can be accepted.", "AuthNetInvitationAcceptanceStatusResponse"),
                        ["400"] = JsonResponse("Invitation token is invalid or cannot be accepted.", "AuthNetInvitationAcceptanceStatusResponse")
                    },
                    parameters:
                    [
                        QueryParameter("token", "Raw invitation token from the invitation link.")
                    ]),
                ["post"] = Operation(
                    "AuthNetApiAcceptInvitation",
                    "Accept an account invitation with local credentials.",
                    requestSchema: "AuthNetAcceptInvitationRequest",
                    responses: new Dictionary<string, object?>
                    {
                        ["200"] = JsonResponse("Invitation accepted.", "AuthNetAcceptInvitationResponse"),
                        ["400"] = JsonResponse("Validation or invitation acceptance failed.", "AuthNetAcceptInvitationResponse")
                    })
            }
        };
    }

    private static IReadOnlyDictionary<string, object?> Operation(
        string operationId,
        string summary,
        IReadOnlyDictionary<string, object?> responses,
        string? requestSchema = null,
        bool requiresCookie = false,
        IReadOnlyList<IReadOnlyDictionary<string, object?>>? parameters = null)
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

        if (parameters is not null)
        {
            operation["parameters"] = parameters;
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

    private static IReadOnlyDictionary<string, object?> QueryParameter(string name, string description)
    {
        return new Dictionary<string, object?>
        {
            ["name"] = name,
            ["in"] = "query",
            ["required"] = true,
            ["description"] = description,
            ["schema"] = new Dictionary<string, object?>
            {
                ["type"] = "string"
            }
        };
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
            ["AuthNetResetPasswordRequest"] = ObjectSchema(
                required: ["email", "code", "password"],
                properties: new Dictionary<string, object?>
                {
                    ["email"] = StringSchema("Email address for password recovery.", format: "email"),
                    ["code"] = StringSchema("Base64Url-encoded password reset token."),
                    ["password"] = StringSchema("New local account password.", format: "password")
                }),
            ["AuthNetResendConfirmationRequest"] = ObjectSchema(
                required: ["email"],
                properties: new Dictionary<string, object?>
                {
                    ["email"] = StringSchema("Email address that needs confirmation.", format: "email")
                }),
            ["AuthNetConfirmEmailRequest"] = ObjectSchema(
                required: ["userId", "code"],
                properties: new Dictionary<string, object?>
                {
                    ["userId"] = StringSchema("User identifier from the confirmation link."),
                    ["code"] = StringSchema("Base64Url-encoded email confirmation token.")
                }),
            ["AuthNetUpdateProfileRequest"] = ObjectSchema(
                required: [],
                properties: new Dictionary<string, object?>
                {
                    ["displayName"] = NullableStringSchema("Optional display name.", maxLength: 200),
                    ["phoneNumber"] = NullableStringSchema("Optional phone number.")
                }),
            ["AuthNetChangePasswordRequest"] = ObjectSchema(
                required: ["currentPassword", "newPassword"],
                properties: new Dictionary<string, object?>
                {
                    ["currentPassword"] = StringSchema("Current local account password.", format: "password"),
                    ["newPassword"] = StringSchema("New local account password.", format: "password")
                }),
            ["AuthNetMfaStatusResponse"] = ObjectSchema(
                required: ["isMfaEnabled", "hasAuthenticator", "recoveryCodesLeft"],
                properties: new Dictionary<string, object?>
                {
                    ["isMfaEnabled"] = BooleanSchema("Whether authenticator-app MFA is enabled."),
                    ["hasAuthenticator"] = BooleanSchema("Whether an authenticator key is configured."),
                    ["recoveryCodesLeft"] = IntegerSchema("Number of remaining recovery codes.")
                }),
            ["AuthNetMfaSetupStartResponse"] = ObjectSchema(
                required: ["sharedKey", "authenticatorUri"],
                properties: new Dictionary<string, object?>
                {
                    ["sharedKey"] = StringSchema("Formatted authenticator shared key."),
                    ["authenticatorUri"] = StringSchema("otpauth URI for authenticator-app setup.")
                }),
            ["AuthNetMfaSetupVerifyRequest"] = ObjectSchema(
                required: ["code"],
                properties: new Dictionary<string, object?>
                {
                    ["code"] = StringSchema("Authenticator-app verification code.")
                }),
            ["AuthNetMfaSetupVerifyResponse"] = ObjectSchema(
                required: ["isMfaEnabled", "recoveryCodes"],
                properties: new Dictionary<string, object?>
                {
                    ["isMfaEnabled"] = BooleanSchema("Whether MFA is enabled after verification."),
                    ["recoveryCodes"] = StringArraySchema("One-time recovery codes generated during setup.")
                }),
            ["AuthNetRecoveryCodesResponse"] = ObjectSchema(
                required: ["recoveryCodesLeft"],
                properties: new Dictionary<string, object?>
                {
                    ["recoveryCodesLeft"] = IntegerSchema("Number of remaining recovery codes.")
                }),
            ["AuthNetRecoveryCodesRegenerateResponse"] = ObjectSchema(
                required: ["recoveryCodes"],
                properties: new Dictionary<string, object?>
                {
                    ["recoveryCodes"] = StringArraySchema("New one-time recovery codes.")
                }),
            ["AuthNetMfaChallengeRequest"] = ObjectSchema(
                required: ["code"],
                properties: new Dictionary<string, object?>
                {
                    ["code"] = StringSchema("Authenticator-app code for the pending sign-in."),
                    ["rememberMe"] = BooleanSchema("Whether the sign-in cookie should be persistent.")
                }),
            ["AuthNetRecoveryCodeLoginRequest"] = ObjectSchema(
                required: ["recoveryCode"],
                properties: new Dictionary<string, object?>
                {
                    ["recoveryCode"] = StringSchema("Recovery code for the pending sign-in.")
                }),
            ["AuthNetExternalProviderResponse"] = ObjectSchema(
                required: ["name", "displayName"],
                properties: new Dictionary<string, object?>
                {
                    ["name"] = StringSchema("Authentication scheme name."),
                    ["displayName"] = StringSchema("Provider display name.")
                }),
            ["AuthNetExternalProvidersResponse"] = ObjectSchema(
                required: ["providers"],
                properties: new Dictionary<string, object?>
                {
                    ["providers"] = ArraySchema(Ref("AuthNetExternalProviderResponse"), "Configured external login providers.")
                }),
            ["AuthNetExternalChallengeRequest"] = ObjectSchema(
                required: ["provider"],
                properties: new Dictionary<string, object?>
                {
                    ["provider"] = StringSchema("Authentication scheme name to challenge."),
                    ["returnUrl"] = NullableStringSchema("Local URL to return to after the callback.")
                }),
            ["AuthNetExternalLoginCallbackResponse"] = ObjectSchema(
                required: ["status", "message", "returnUrl"],
                properties: new Dictionary<string, object?>
                {
                    ["status"] = StringSchema("Stable callback outcome."),
                    ["message"] = StringSchema("Human-readable callback message."),
                    ["returnUrl"] = StringSchema("Safe local URL supplied by the challenge."),
                    ["provider"] = NullableStringSchema("External login provider, when available."),
                    ["email"] = NullableStringSchema("External or linked user email, when available."),
                    ["userId"] = NullableStringSchema("AuthNet user identifier, when available.")
                }),
            ["AuthNetExternalLinkCallbackResponse"] = ObjectSchema(
                required: ["status", "message", "returnUrl"],
                properties: new Dictionary<string, object?>
                {
                    ["status"] = StringSchema("Stable link callback outcome."),
                    ["message"] = StringSchema("Human-readable callback message."),
                    ["returnUrl"] = StringSchema("Safe local URL supplied by the challenge."),
                    ["provider"] = NullableStringSchema("External login provider, when available.")
                }),
            ["AuthNetInvitationAcceptanceStatusResponse"] = ObjectSchema(
                required: ["result", "status"],
                properties: new Dictionary<string, object?>
                {
                    ["result"] = Ref("AuthNetApiResult"),
                    ["status"] = StringSchema("Stable invitation token state."),
                    ["email"] = NullableStringSchema("Invited email address when a token resolves to an invitation."),
                    ["expiresAtUtc"] = NullableStringSchema("Invitation expiration timestamp in UTC.")
                }),
            ["AuthNetAcceptInvitationRequest"] = ObjectSchema(
                required: ["token", "userName", "password", "confirmPassword"],
                properties: new Dictionary<string, object?>
                {
                    ["token"] = StringSchema("Raw invitation token from the invitation link."),
                    ["userName"] = StringSchema("Local username for the invited account.", maxLength: 256),
                    ["displayName"] = NullableStringSchema("Optional display name.", maxLength: 200),
                    ["password"] = StringSchema("Local account password.", format: "password"),
                    ["confirmPassword"] = StringSchema("Password confirmation.", format: "password")
                }),
            ["AuthNetAcceptInvitationResponse"] = ObjectSchema(
                required: ["result", "status"],
                properties: new Dictionary<string, object?>
                {
                    ["result"] = Ref("AuthNetApiResult"),
                    ["status"] = StringSchema("Stable invitation acceptance outcome."),
                    ["email"] = NullableStringSchema("Invited email address when available."),
                    ["userId"] = NullableStringSchema("Created AuthNet user identifier when acceptance succeeds.")
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

    private static IReadOnlyDictionary<string, object?> IntegerSchema(string description)
    {
        return new Dictionary<string, object?>
        {
            ["type"] = "integer",
            ["format"] = "int32",
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
