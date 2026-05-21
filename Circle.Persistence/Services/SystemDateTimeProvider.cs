using Circle.Application.Abstractions;

namespace Circle.Persistence.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
