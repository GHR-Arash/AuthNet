using Microsoft.AspNetCore.Identity;

namespace AuthNet.Persistence.EntityFrameworkCore;

public sealed class AuthNetUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
