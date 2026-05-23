using Circle.Application.Abstractions;
using Circle.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Circle.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Server=localhost;Port=3306;Database=circle;User ID=root;Password=your_password;";
        }

        services.AddDbContext<CircleDbContext>(options =>
            options.UseMySQL(connectionString));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<CircleDbContext>());

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IEmailSender, ConsoleEmailSender>();

        return services;
    }
}
