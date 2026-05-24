using Circle.Application.Abstractions;
using Circle.Domain.Common;
using Circle.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Circle.Persistence;

public sealed class CircleDbContext(DbContextOptions<CircleDbContext> options, IDateTimeProvider dateTimeProvider)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerDocument> CustomerDocuments => Set<CustomerDocument>();
    public DbSet<Loan> Loans => Set<Loan>();
    public DbSet<LoanDisbursement> LoanDisbursements => Set<LoanDisbursement>();
    public DbSet<LoanRepaymentSchedule> LoanRepaymentSchedules => Set<LoanRepaymentSchedule>();
    public DbSet<Repayment> Repayments => Set<Repayment>();
    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<UserOtp> UserOtps => Set<UserOtp>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CircleDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = dateTimeProvider.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.MarkCreated(utcNow);
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.MarkUpdated(utcNow);
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
