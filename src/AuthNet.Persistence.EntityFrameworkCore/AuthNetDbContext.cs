using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthNet.Persistence.EntityFrameworkCore;

public sealed class AuthNetDbContext(DbContextOptions<AuthNetDbContext> options)
    : IdentityDbContext<AuthNetUser, IdentityRole, string>(options)
{
    public DbSet<AuthNetAuditEvent> AuditEvents => Set<AuthNetAuditEvent>();

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

        builder.Entity<AuthNetAuditEvent>(auditEvent =>
        {
            auditEvent.ToTable("AuthNetAuditEvents");
            auditEvent.HasKey(item => item.Id);
            auditEvent.Property(item => item.Action).IsRequired().HasMaxLength(100);
            auditEvent.Property(item => item.Outcome).IsRequired().HasMaxLength(50);
            auditEvent.Property(item => item.ActorUserId).HasMaxLength(450);
            auditEvent.Property(item => item.ActorEmail).HasMaxLength(256);
            auditEvent.Property(item => item.TargetUserId).HasMaxLength(450);
            auditEvent.Property(item => item.TargetEmail).HasMaxLength(256);
            auditEvent.Property(item => item.Metadata).HasMaxLength(1024);
            auditEvent.HasIndex(item => item.CreatedAtUtc);
            auditEvent.HasIndex(item => item.Action);
            auditEvent.HasIndex(item => item.ActorUserId);
            auditEvent.HasIndex(item => item.TargetUserId);
        });
    }
}
