using Circle.Domain.Common;

namespace Circle.Domain.Entities;

public sealed class RefreshToken : BaseEntity
{
    private RefreshToken()
    {
    }

    public RefreshToken(Guid userId, string tokenHash, DateTime expiresAtUtc, string? createdByIp)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedByIp = createdByIp;
    }

    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public string? CreatedByIp { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? RevokedByIp { get; private set; }

    public bool IsActive(DateTime utcNow)
    {
        return RevokedAtUtc is null && ExpiresAtUtc > utcNow;
    }

    public void Revoke(DateTime utcNow, string? revokedByIp)
    {
        RevokedAtUtc = utcNow;
        RevokedByIp = revokedByIp;
    }
}
