using Circle.Domain.Common;
using Circle.Domain.Enums;

namespace Circle.Domain.Entities;

public sealed class User : BaseEntity
{
    private User()
    {
    }

    public User(
        string email,
        string fullName,
        string phone,
        UserRole role,
        string? branch,
        Guid? createdByUserId)
    {
        Email = email.Trim().ToLowerInvariant();
        FullName = fullName.Trim();
        Phone = phone.Trim();
        Role = role;
        Branch = branch?.Trim();
        Status = UserStatus.Invited;
        CreatedByUserId = createdByUserId;
    }

    public string Email { get; private set; } = string.Empty;
    public string? PasswordHash { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }
    public string? Branch { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTime? LastLoginAtUtc { get; private set; }
    public Guid? CreatedByUserId { get; private set; }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    public void Activate()
    {
        Status = UserStatus.Active;
    }

    public void Suspend()
    {
        Status = UserStatus.Suspended;
    }

    public void Deactivate()
    {
        Status = UserStatus.Deactivated;
    }

    public void RecordLogin(DateTime utcNow)
    {
        LastLoginAtUtc = utcNow;
    }
}
