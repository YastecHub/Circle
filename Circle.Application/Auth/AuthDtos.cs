using Circle.Domain.Enums;

namespace Circle.Application.Auth;

public sealed record BootstrapAdminRequest(
    string Email,
    string Password,
    string FullName,
    string Phone);

public sealed record LoginRequest(string Email, string Password);

public sealed record LoginResponse(
    bool RequiresOtp,
    string Message,
    string? DebugOtpCode = null);

public sealed record VerifyOtpRequest(string Email, string Code);

public sealed record AuthUserResponse(
    Guid Id,
    string Email,
    string FullName,
    string Phone,
    UserRole Role,
    UserStatus Status);

public sealed record AuthTokenResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    AuthUserResponse User);
