using Circle.Domain.Enums;

namespace Circle.Application.Users;

public sealed record InviteCreditOfficerRequest(
    string Email,
    string FullName,
    string Phone,
    string? Branch);

public sealed record UserResponse(
    Guid Id,
    string Email,
    string FullName,
    string Phone,
    UserRole Role,
    UserStatus Status,
    string? Branch,
    DateTime CreatedAtUtc,
    DateTime? LastLoginAtUtc);

public sealed record SetUserPasswordRequest(string Password);
