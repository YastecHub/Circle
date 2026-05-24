using Microsoft.Extensions.DependencyInjection;
using Circle.Application.Auth;
using Circle.Application.Customers;
using Circle.Application.Disbursements;
using Circle.Application.Loans;
using Circle.Application.Repayments;
using Circle.Application.Users;

namespace Circle.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IDisbursementService, DisbursementService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<IRepaymentService, RepaymentService>();
        services.AddScoped<IUserManagementService, UserManagementService>();

        return services;
    }
}
