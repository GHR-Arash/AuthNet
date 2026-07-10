using AuthNet.Persistence.SqlServer;
using Microsoft.EntityFrameworkCore;

namespace AuthNet.AspNetCore;

internal static class AuthNetSqlServerDbContextOptions
{
    public static void Configure(DbContextOptionsBuilder db, string connectionString)
    {
        db.UseSqlServer(
            connectionString,
            sqlServer => sqlServer.MigrationsAssembly(typeof(AuthNetSqlServerMigrationsAssembly).Assembly.GetName().Name));
    }
}
