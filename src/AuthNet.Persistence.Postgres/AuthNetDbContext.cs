using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthNet.Persistence.Postgres;

public sealed class AuthNetDbContext(DbContextOptions<AuthNetDbContext> options)
    : IdentityDbContext<AuthNetUser, IdentityRole, string>(options)
{
    public DbSet<AuthNetInvitation> Invitations => Set<AuthNetInvitation>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AuthNetInvitation>(invitation =>
        {
            invitation.ToTable("AuthNetInvitations");
            invitation.HasKey(item => item.Id);
            invitation.Property(item => item.Email).IsRequired().HasMaxLength(256);
            invitation.Property(item => item.NormalizedEmail).IsRequired().HasMaxLength(256);
            invitation.Property(item => item.TokenHash).IsRequired().HasMaxLength(128);
            invitation.Property(item => item.CreatedByUserId).HasMaxLength(450);
            invitation.Property(item => item.AcceptedByUserId).HasMaxLength(450);
            invitation.HasIndex(item => item.TokenHash).IsUnique();
            invitation.HasIndex(item => item.NormalizedEmail);
        });
    }
}
