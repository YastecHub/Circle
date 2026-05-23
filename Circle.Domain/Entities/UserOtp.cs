using Circle.Domain.Common;
using Circle.Domain.Enums;

namespace Circle.Domain.Entities;

public sealed class UserOtp : BaseEntity
{
    private UserOtp()
    {
    }

    public UserOtp(Guid userId, string codeHash, OtpPurpose purpose, DateTime expiresAtUtc)
    {
        UserId = userId;
        CodeHash = codeHash;
        Purpose = purpose;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid UserId { get; private set; }
    public string CodeHash { get; private set; } = string.Empty;
    public OtpPurpose Purpose { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? ConsumedAtUtc { get; private set; }
    public int FailedAttempts { get; private set; }

    public bool IsUsable(DateTime utcNow)
    {
        return ConsumedAtUtc is null && ExpiresAtUtc > utcNow && FailedAttempts < 5;
    }

    public void RegisterFailedAttempt()
    {
        FailedAttempts++;
    }

    public void Consume(DateTime utcNow)
    {
        ConsumedAtUtc = utcNow;
    }
}
