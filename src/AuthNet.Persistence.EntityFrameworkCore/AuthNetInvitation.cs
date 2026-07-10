namespace AuthNet.Persistence.EntityFrameworkCore;

public sealed class AuthNetInvitation
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Email { get; set; } = string.Empty;

    public string NormalizedEmail { get; set; } = string.Empty;

    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset ExpiresAtUtc { get; set; }

    public DateTimeOffset? AcceptedAtUtc { get; set; }

    public string? AcceptedByUserId { get; set; }

    public string? CreatedByUserId { get; set; }

    public bool IsAccepted => AcceptedAtUtc is not null;

    public bool IsExpired(DateTimeOffset nowUtc) => ExpiresAtUtc <= nowUtc;

    public bool IsPending(DateTimeOffset nowUtc) => !IsAccepted && !IsExpired(nowUtc);
}
