using Circle.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Circle.Application.Abstractions;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }
    DbSet<CollectionActivity> CollectionActivities { get; }
    DbSet<Customer> Customers { get; }
    DbSet<CustomerDocument> CustomerDocuments { get; }
    DbSet<CustomerVirtualAccount> CustomerVirtualAccounts { get; }
    DbSet<InboundPayment> InboundPayments { get; }
    DbSet<Loan> Loans { get; }
    DbSet<LoanDisbursement> LoanDisbursements { get; }
    DbSet<LoanRepaymentSchedule> LoanRepaymentSchedules { get; }
    DbSet<Repayment> Repayments { get; }
    DbSet<SystemSetting> SystemSettings { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<UserOtp> UserOtps { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
