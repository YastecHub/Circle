using Microsoft.Extensions.DependencyInjection;
using Circle.Application.Administration;
using Circle.Application.Auth;
using Circle.Application.Collections;
using Circle.Application.CustomerDocuments;
using Circle.Application.Customers;
using Circle.Application.Disbursements;
using Circle.Application.Loans;
using Circle.Application.Reconciliation;
using Circle.Application.Repayments;
using Circle.Application.Reports;
using Circle.Application.Users;
using Circle.Application.VirtualAccounts;

namespace Circle.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICollectionService, CollectionService>();
        services.AddScoped<ICustomerDocumentService, CustomerDocumentService>();
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IDisbursementService, DisbursementService>();
        services.AddScoped<ILoanService, LoanService>();
        services.AddScoped<IReconciliationService, ReconciliationService>();
        services.AddScoped<IRepaymentService, RepaymentService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IUserManagementService, UserManagementService>();
        services.AddScoped<IVirtualAccountService, VirtualAccountService>();

        return services;
    }
}
