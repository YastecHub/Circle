using Circle.Application.Abstractions;
using Circle.Application.Common.Options;
using Circle.Persistence.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;

namespace Circle.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["MYSQLCONNSTR_DefaultConnection"];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Database connection string is missing. Set ConnectionStrings__DefaultConnection in App Settings or MYSQLCONNSTR_DefaultConnection in Azure Connection strings.");
        }

        services.AddDbContextPool<CircleDbContext>(options =>
            options.UseMySQL(connectionString));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<CircleDbContext>());

        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IFileStorageService, LocalFileStorageService>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();

        var resendApiKey = configuration["Resend:ApiKey"] ?? configuration["RESEND_APITOKEN"];
        if (string.IsNullOrWhiteSpace(resendApiKey))
        {
            services.AddSingleton<IEmailSender, ConsoleEmailSender>();
        }
        else
        {
            services.Configure<ResendEmailOptions>(configuration.GetSection(ResendEmailOptions.SectionName));
            services.AddResend(options =>
            {
                options.ApiToken = resendApiKey;
            });
            services.AddScoped<IEmailSender, ResendEmailSender>();
        }

        return services;
    }
}
