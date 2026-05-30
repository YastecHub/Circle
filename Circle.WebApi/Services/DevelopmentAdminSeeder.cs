using Circle.Application.Abstractions;
using Circle.Domain.Entities;
using Circle.Domain.Enums;
using Circle.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Circle.WebApi.Services;

public static class DevelopmentAdminSeeder
{
    public static async Task SeedDevelopmentAdminAsync(this WebApplication app)
    {
        var options = app.Configuration
            .GetSection(DevelopmentAdminSeedOptions.SectionName)
            .Get<DevelopmentAdminSeedOptions>();

        if (options is not { Enabled: true })
        {
            return;
        }

        Validate(options);

        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CircleDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Circle.WebApi.Services.DevelopmentAdminSeeder");

        var email = options.Email.Trim().ToLowerInvariant();
        var admin = await dbContext.Users.SingleOrDefaultAsync(user => user.Email == email);
        var action = "SeedDevelopmentAdmin";

        if (admin is null)
        {
            admin = new User(
                options.Email,
                options.FullName,
                options.Phone,
                UserRole.Admin,
                branch: null,
                createdByUserId: null);

            admin.SetPasswordHash(passwordHasher.HashPassword(options.Password));
            admin.Activate();
            dbContext.Users.Add(admin);
        }
        else
        {
            action = "UpdateDevelopmentAdmin";
            admin.UpdateProfile(options.FullName, options.Phone, branch: null);
            admin.ChangeRole(UserRole.Admin);
            admin.Activate();

            if (options.ResetPassword)
            {
                admin.SetPasswordHash(passwordHasher.HashPassword(options.Password));
            }
        }

        if (options.DeactivateOtherAdmins)
        {
            var otherAdmins = await dbContext.Users
                .Where(user => user.Email != email && user.Role == UserRole.Admin && user.Status == UserStatus.Active)
                .ToListAsync();

            foreach (var otherAdmin in otherAdmins)
            {
                otherAdmin.Deactivate();
                dbContext.AuditLogs.Add(new AuditLog(
                    null,
                    "DeactivateOtherDevelopmentAdmin",
                    $"User:{otherAdmin.Email}",
                    UserStatus.Active.ToString(),
                    UserStatus.Deactivated.ToString(),
                    null,
                    $"Development admin seed kept {email} as the configured admin."));
            }
        }

        dbContext.AuditLogs.Add(new AuditLog(
            null,
            action,
            $"User:{email}",
            null,
            "AdminReady",
            null,
            "Development admin seed"));

        await dbContext.SaveChangesAsync();
        logger.LogWarning("Development admin seed applied for {Email}. Disable SeedAdmin__Enabled after confirming login.", email);
    }

    private static void Validate(DevelopmentAdminSeedOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Email))
        {
            throw new InvalidOperationException("SeedAdmin is enabled but SeedAdmin__Email is missing.");
        }

        if (string.IsNullOrWhiteSpace(options.Password))
        {
            throw new InvalidOperationException("SeedAdmin is enabled but SeedAdmin__Password is missing.");
        }

        if (string.IsNullOrWhiteSpace(options.FullName))
        {
            throw new InvalidOperationException("SeedAdmin is enabled but SeedAdmin__FullName is missing.");
        }

        if (string.IsNullOrWhiteSpace(options.Phone))
        {
            throw new InvalidOperationException("SeedAdmin is enabled but SeedAdmin__Phone is missing.");
        }
    }
}
