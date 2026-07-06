using Microsoft.AspNetCore.Identity;

namespace AuthNet.Persistence.Postgres;

public sealed class AuthNetUser : IdentityUser
{
    public string? DisplayName { get; set; }
}

