using Microsoft.Extensions.DependencyInjection;
using Circle.Application.Auth;
using Circle.Application.Users;

namespace Circle.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserManagementService, UserManagementService>();

        return services;
    }
}
