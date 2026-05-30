namespace Circle.Domain.Enums;

public static class UserRoleExtensions
{
    public static bool HasAdminAccess(this UserRole role)
    {
        return role is UserRole.Admin or UserRole.AppAdmin or UserRole.Developer;
    }
}
