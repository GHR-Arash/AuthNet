using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthNet.Persistence.Postgres;

public sealed class AuthNetDbContext(DbContextOptions<AuthNetDbContext> options)
    : IdentityDbContext<AuthNetUser, IdentityRole, string>(options);

