namespace Circle.Application.Abstractions;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    string? IpAddress { get; }
}
