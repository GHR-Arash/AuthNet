using AuthNet.Persistence.Postgres;
using Microsoft.EntityFrameworkCore;

namespace AuthNet.AspNetCore;

internal static class AuthNetPostgresDbContextOptions
{
    public static void Configure(DbContextOptionsBuilder db, string connectionString)
    {
        db.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsAssembly(typeof(AuthNetPostgresMigrationsAssembly).Assembly.GetName().Name));
    }
}
