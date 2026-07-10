using System.Security.Claims;
using AuthNet.Persistence.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace AuthNetRazor;

public interface IAuthNetAuditWriter
{
    Task RecordAsync(
        ClaimsPrincipal actor,
        string action,
        AuthNetUser? targetUser = null,
        string? targetEmail = null,
        string? metadata = null,
        CancellationToken cancellationToken = default);
}

public sealed class AuthNetAuditWriter(
    AuthNetDbContext dbContext,
    UserManager<AuthNetUser> userManager) : IAuthNetAuditWriter
{
    private const string SucceededOutcome = "Succeeded";
    private const int MetadataMaxLength = 1024;

    public async Task RecordAsync(
        ClaimsPrincipal actor,
        string action,
        AuthNetUser? targetUser = null,
        string? targetEmail = null,
        string? metadata = null,
        CancellationToken cancellationToken = default)
    {
        var actorUser = await userManager.GetUserAsync(actor);
        dbContext.AuditEvents.Add(new AuthNetAuditEvent
        {
            CreatedAtUtc = DateTimeOffset.UtcNow,
            Action = action,
            Outcome = SucceededOutcome,
            ActorUserId = actorUser?.Id,
            ActorEmail = actorUser?.Email,
            TargetUserId = targetUser?.Id,
            TargetEmail = targetUser?.Email ?? targetEmail,
            Metadata = TruncateMetadata(metadata)
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string? TruncateMetadata(string? metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata))
        {
            return null;
        }

        var trimmed = metadata.Trim();
        return trimmed.Length <= MetadataMaxLength
            ? trimmed
            : trimmed[..MetadataMaxLength];
    }
}
