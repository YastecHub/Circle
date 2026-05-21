namespace Circle.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; private set; }

    public void MarkCreated(DateTime utcNow)
    {
        CreatedAtUtc = utcNow;
    }

    public void MarkUpdated(DateTime utcNow)
    {
        UpdatedAtUtc = utcNow;
    }
}
