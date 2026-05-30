namespace Circle.WebApi.Services;

public sealed class DevelopmentAdminSeedOptions
{
    public const string SectionName = "SeedAdmin";

    public bool Enabled { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public bool ResetPassword { get; init; } = true;
    public bool DeactivateOtherAdmins { get; init; }
}
