using Circle.Application.Abstractions;
using Circle.Application.Common.Exceptions;
using Circle.Domain.Entities;
using Circle.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Circle.Application.Users;

public sealed class UserManagementService(
    IApplicationDbContext dbContext,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUserService) : IUserManagementService
{
    public async Task<IReadOnlyCollection<UserResponse>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Users
            .OrderBy(user => user.FullName)
            .Select(user => new UserResponse(
                user.Id,
                user.Email,
                user.FullName,
                user.Phone,
                user.Role,
                user.Status,
                user.Branch,
                user.CreatedAtUtc,
                user.LastLoginAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<UserResponse> InviteCreditOfficerAsync(InviteCreditOfficerRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken))
        {
            throw new AppException("A user with this email already exists.");
        }

        var user = new User(
            email,
            request.FullName,
            request.Phone,
            UserRole.CreditOfficer,
            request.Branch,
            currentUserService.UserId);

        dbContext.Users.Add(user);
        dbContext.AuditLogs.Add(new AuditLog(currentUserService.UserId, "InviteCreditOfficer", $"User:{user.Email}", null, user.Email, currentUserService.IpAddress, null));
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(user);
    }

    public async Task<UserResponse> ActivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await FindUserAsync(userId, cancellationToken);
        user.Activate();
        dbContext.AuditLogs.Add(new AuditLog(currentUserService.UserId, "ActivateUser", $"User:{user.Email}", null, user.Status.ToString(), currentUserService.IpAddress, null));
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(user);
    }

    public async Task<UserResponse> SuspendUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await FindUserAsync(userId, cancellationToken);
        user.Suspend();
        dbContext.AuditLogs.Add(new AuditLog(currentUserService.UserId, "SuspendUser", $"User:{user.Email}", null, user.Status.ToString(), currentUserService.IpAddress, null));
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(user);
    }

    public async Task<UserResponse> SetPasswordAsync(Guid userId, SetUserPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await FindUserAsync(userId, cancellationToken);
        user.SetPasswordHash(passwordHasher.HashPassword(request.Password));
        user.Activate();
        dbContext.AuditLogs.Add(new AuditLog(currentUserService.UserId, "SetUserPassword", $"User:{user.Email}", null, "PasswordUpdated", currentUserService.IpAddress, null));
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToResponse(user);
    }

    private async Task<User> FindUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await dbContext.Users.SingleOrDefaultAsync(user => user.Id == userId, cancellationToken)
            ?? throw new AppException("User was not found.");
    }

    private static UserResponse ToResponse(User user)
    {
        return new UserResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.Phone,
            user.Role,
            user.Status,
            user.Branch,
            user.CreatedAtUtc,
            user.LastLoginAtUtc);
    }
}
