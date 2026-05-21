namespace Circle.Application.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
