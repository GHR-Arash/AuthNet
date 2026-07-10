using AuthNet.Persistence.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthNet.Persistence.SqlServer;

public sealed class AuthNetSqlServerDesignTimeDbContextFactory : IDesignTimeDbContextFactory<AuthNetDbContext>
{
    public AuthNetDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AuthNetDbContext>()
            .UseSqlServer(
                "Server=(localdb)\\mssqllocaldb;Database=AuthNetDesignTime;Trusted_Connection=True;",
                sqlServer => sqlServer.MigrationsAssembly(typeof(AuthNetSqlServerMigrationsAssembly).Assembly.GetName().Name))
            .Options;

        return new AuthNetDbContext(options);
    }
}
