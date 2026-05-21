using Circle.Domain.Common;

namespace Circle.Domain.Entities;

public sealed class AuditLog : BaseEntity
{
    private AuditLog()
    {
    }

    public AuditLog(
        Guid? actorUserId,
        string action,
        string affectedRecord,
        string? oldValue,
        string? newValue,
        string? ipAddress,
        string? reason)
    {
        ActorUserId = actorUserId;
        Action = action.Trim();
        AffectedRecord = affectedRecord.Trim();
        OldValue = oldValue;
        NewValue = newValue;
        IpAddress = ipAddress;
        Reason = reason;
    }

    public Guid? ActorUserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string AffectedRecord { get; private set; } = string.Empty;
    public string? OldValue { get; private set; }
    public string? NewValue { get; private set; }
    public string? IpAddress { get; private set; }
    public string? Reason { get; private set; }
}
