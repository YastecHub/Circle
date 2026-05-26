using System.Security.Cryptography;
using Circle.Application.Abstractions;
using Circle.Application.Common.Exceptions;
using Circle.Application.Common.Options;
using Circle.Domain.Entities;
using Circle.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Circle.Application.Auth;

public sealed class AuthService(
    IApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    IEmailSender emailSender,
    ITokenService tokenService,
    IDateTimeProvider dateTimeProvider,
    ICurrentUserService currentUserService,
    IOptions<JwtOptions> jwtOptions,
    IOptions<OtpOptions> otpOptions) : IAuthService
{
    public async Task<AuthUserResponse> BootstrapAdminAsync(BootstrapAdminRequest request, CancellationToken cancellationToken = default)
    {
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            throw new AppException("Bootstrap is only allowed before any user exists.");
        }

        var admin = new User(
            request.Email,
            request.FullName,
            request.Phone,
            UserRole.Admin,
            branch: null,
            createdByUserId: null);

        admin.SetPasswordHash(passwordHasher.HashPassword(request.Password));
        admin.Activate();

        dbContext.Users.Add(admin);
        dbContext.AuditLogs.Add(new AuditLog(null, "BootstrapAdmin", $"User:{admin.Email}", null, admin.Email, currentUserService.IpAddress, "Initial admin bootstrap"));
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToAuthUserResponse(admin);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var user = await dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Email == email, cancellationToken);

        if (user is null ||
            user.Status != UserStatus.Active ||
            string.IsNullOrWhiteSpace(user.PasswordHash) ||
            !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return new LoginResponse(false, "Invalid login credentials.");
        }

        var otpCode = GenerateOtpCode(otpOptions.Value.CodeLength);
        var otp = new UserOtp(
            user.Id,
            passwordHasher.HashPassword(otpCode),
            OtpPurpose.Login,
            dateTimeProvider.UtcNow.AddMinutes(otpOptions.Value.ExpiryMinutes));

        dbContext.UserOtps.Add(otp);
        dbContext.AuditLogs.Add(new AuditLog(user.Id, "LoginOtpRequested", $"User:{user.Email}", null, null, currentUserService.IpAddress, null));
        await dbContext.SaveChangesAsync(cancellationToken);
        await emailSender.SendOtpAsync(user, otpCode, cancellationToken);

        return new LoginResponse(true, "OTP has been sent to the user's email.", otpOptions.Value.IncludeCodeInResponse ? otpCode : null);
    }

    public async Task<AuthTokenResponse> VerifyOtpAsync(VerifyOtpRequest request, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(request.Email);
        var user = await dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Email == email, cancellationToken)
            ?? throw new AppException("Invalid OTP.");

        var utcNow = dateTimeProvider.UtcNow;
        var otp = await dbContext.UserOtps
            .Where(candidate => candidate.UserId == user.Id && candidate.Purpose == OtpPurpose.Login)
            .OrderByDescending(candidate => candidate.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new AppException("Invalid OTP.");

        if (!otp.IsUsable(utcNow))
        {
            throw new AppException("OTP has expired or can no longer be used.");
        }

        if (!passwordHasher.VerifyPassword(request.Code, otp.CodeHash))
        {
            otp.RegisterFailedAttempt();
            await dbContext.SaveChangesAsync(cancellationToken);
            throw new AppException("Invalid OTP.");
        }

        otp.Consume(utcNow);
        user.RecordLogin(utcNow);

        var accessTokenExpiresAt = utcNow.AddMinutes(jwtOptions.Value.AccessTokenMinutes);
        var refreshTokenExpiresAt = utcNow.AddDays(jwtOptions.Value.RefreshTokenDays);
        var refreshToken = tokenService.CreateRefreshToken();

        dbContext.RefreshTokens.Add(new RefreshToken(
            user.Id,
            passwordHasher.HashPassword(refreshToken),
            refreshTokenExpiresAt,
            currentUserService.IpAddress));
        dbContext.AuditLogs.Add(new AuditLog(user.Id, "LoginCompleted", $"User:{user.Email}", null, null, currentUserService.IpAddress, null));

        await dbContext.SaveChangesAsync(cancellationToken);

        return new AuthTokenResponse(
            tokenService.CreateAccessToken(user, accessTokenExpiresAt),
            accessTokenExpiresAt,
            refreshToken,
            refreshTokenExpiresAt,
            ToAuthUserResponse(user));
    }

    public async Task<CurrentUserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var currentUserId = currentUserService.UserId
            ?? throw new AppException("Current user was not found.");

        var user = await dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Id == currentUserId, cancellationToken)
            ?? throw new AppException("Current user was not found.");

        return new CurrentUserResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.Phone,
            user.Role,
            user.Status,
            user.Branch,
            user.CreatedAtUtc,
            user.LastLoginAtUtc,
            GetPermissions(user.Role));
    }

    private static AuthUserResponse ToAuthUserResponse(User user)
    {
        return new AuthUserResponse(user.Id, user.Email, user.FullName, user.Phone, user.Role, user.Status);
    }

    private static IReadOnlyCollection<string> GetPermissions(UserRole role)
    {
        var commonPermissions = new[]
        {
            "dashboard.read",
            "customers.read",
            "customers.write",
            "loans.read",
            "loans.write",
            "repayments.read",
            "repayments.write",
            "collections.read",
            "collections.write",
            "reports.read",
            "search.read"
        };

        if (role == UserRole.CreditOfficer)
        {
            return commonPermissions;
        }

        return commonPermissions
            .Concat([
                "users.manage",
                "customers.reassign",
                "loans.approve",
                "disbursements.read",
                "disbursements.write",
                "reconciliation.read",
                "reconciliation.write",
                "settings.manage",
                "audit.read",
                "webhooks.read"
            ])
            .ToList();
    }

    private static string NormalizeEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    private static string GenerateOtpCode(int length)
    {
        var min = (int)Math.Pow(10, length - 1);
        var max = (int)Math.Pow(10, length) - 1;
        return RandomNumberGenerator.GetInt32(min, max + 1).ToString();
    }
}
