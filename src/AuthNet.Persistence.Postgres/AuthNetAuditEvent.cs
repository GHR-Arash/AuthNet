namespace AuthNet.Persistence.Postgres;

public sealed class AuthNetAuditEvent
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public DateTimeOffset CreatedAtUtc { get; set; }

    public string Action { get; set; } = string.Empty;

    public string Outcome { get; set; } = string.Empty;

    public string? ActorUserId { get; set; }

    public string? ActorEmail { get; set; }

    public string? TargetUserId { get; set; }

    public string? TargetEmail { get; set; }

    public string? Metadata { get; set; }
}
