using Circle.Application.Abstractions;
using Circle.Application.Common.Exceptions;
using Circle.Domain.Entities;
using Circle.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Circle.Application.Dashboard;

public sealed class DashboardService(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : IDashboardService
{
    public async Task<DashboardOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow);

        var customers = ApplyCustomerScope(dbContext.Customers.AsNoTracking(), currentUser);
        var loans = ApplyLoanScope(dbContext.Loans.AsNoTracking(), currentUser);
        var schedules = ApplyScheduleScope(dbContext.LoanRepaymentSchedules.AsNoTracking(), currentUser);
        var disbursements = ApplyDisbursementScope(dbContext.LoanDisbursements.AsNoTracking(), currentUser);
        var repayments = ApplyRepaymentScope(dbContext.Repayments.AsNoTracking(), currentUser);
        var activities = ApplyActivityScope(dbContext.CollectionActivities.AsNoTracking(), currentUser);

        var totalCustomers = await customers.CountAsync(cancellationToken);
        var activeCustomers = await customers.CountAsync(customer => customer.Status == CustomerStatus.Active, cancellationToken);
        var activeLoans = await loans.CountAsync(loan => loan.Status == LoanStatus.Approved || loan.Status == LoanStatus.Disbursed, cancellationToken);
        var pendingApprovalLoans = await loans.CountAsync(loan => loan.Status == LoanStatus.PendingOwnerApproval, cancellationToken);
        var totalOutstanding = await schedules
            .Where(schedule => schedule.Status != RepaymentScheduleStatus.Paid)
            .SumAsync(schedule => (decimal?)(schedule.TotalAmount - schedule.AmountPaid), cancellationToken) ?? 0m;
        var totalOverdue = await schedules
            .Where(schedule => schedule.Status != RepaymentScheduleStatus.Paid && schedule.DueDate < today)
            .SumAsync(schedule => (decimal?)(schedule.TotalAmount - schedule.AmountPaid), cancellationToken) ?? 0m;

        var dueTodaySchedules = schedules.Where(schedule => schedule.Status != RepaymentScheduleStatus.Paid && schedule.DueDate == today);
        var overdueSchedules = schedules.Where(schedule => schedule.Status != RepaymentScheduleStatus.Paid && schedule.DueDate < today);
        var promiseToPayDue = activities.Where(activity =>
            activity.Outcome == CollectionOutcome.PromiseToPay &&
            activity.PromisedPaymentDate != null &&
            activity.PromisedPaymentDate <= today);

        var disbursedToday = await disbursements
            .Where(disbursement => disbursement.DisbursementDate == today)
            .SumAsync(disbursement => (decimal?)disbursement.Amount, cancellationToken) ?? 0m;
        var collectedToday = await repayments
            .Where(repayment => repayment.PaymentDate == today)
            .SumAsync(repayment => (decimal?)repayment.Amount, cancellationToken) ?? 0m;

        var recentRepayments = await GetRecentRepaymentsAsync(repayments, cancellationToken);
        var recentActivities = await GetRecentActivitiesAsync(activities, cancellationToken);

        return new DashboardOverviewResponse(
            today,
            new DashboardPortfolioSnapshot(
                totalCustomers,
                activeCustomers,
                activeLoans,
                pendingApprovalLoans,
                totalOutstanding,
                totalOverdue),
            new DashboardCollectionSnapshot(
                await dueTodaySchedules.CountAsync(cancellationToken),
                await dueTodaySchedules.SumAsync(schedule => (decimal?)(schedule.TotalAmount - schedule.AmountPaid), cancellationToken) ?? 0m,
                await overdueSchedules.CountAsync(cancellationToken),
                await overdueSchedules.SumAsync(schedule => (decimal?)(schedule.TotalAmount - schedule.AmountPaid), cancellationToken) ?? 0m,
                await promiseToPayDue.CountAsync(cancellationToken),
                await promiseToPayDue.SumAsync(activity => activity.PromisedAmount, cancellationToken) ?? 0m),
            new DashboardCashSnapshot(
                disbursedToday,
                collectedToday,
                collectedToday - disbursedToday),
            recentRepayments,
            recentActivities);
    }

    private async Task<IReadOnlyCollection<DashboardRecentRepaymentResponse>> GetRecentRepaymentsAsync(
        IQueryable<Repayment> scopedRepayments,
        CancellationToken cancellationToken)
    {
        return await (
            from repayment in scopedRepayments
            join loan in dbContext.Loans.AsNoTracking() on repayment.LoanId equals loan.Id
            join customer in dbContext.Customers.AsNoTracking() on repayment.CustomerId equals customer.Id
            orderby repayment.CreatedAtUtc descending
            select new DashboardRecentRepaymentResponse(
                repayment.Id,
                repayment.LoanId,
                loan.LoanCode,
                repayment.CustomerId,
                customer.FullName,
                repayment.Amount,
                repayment.PaymentDate,
                repayment.Method,
                repayment.Reference,
                repayment.CreatedAtUtc))
            .Take(10)
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<DashboardRecentCollectionActivityResponse>> GetRecentActivitiesAsync(
        IQueryable<CollectionActivity> scopedActivities,
        CancellationToken cancellationToken)
    {
        return await (
            from activity in scopedActivities
            join customer in dbContext.Customers.AsNoTracking() on activity.CustomerId equals customer.Id
            join loan in dbContext.Loans.AsNoTracking() on activity.LoanId equals loan.Id into loanGroup
            from loan in loanGroup.DefaultIfEmpty()
            orderby activity.CreatedAtUtc descending
            select new DashboardRecentCollectionActivityResponse(
                activity.Id,
                activity.CustomerId,
                customer.FullName,
                activity.LoanId,
                loan == null ? null : loan.LoanCode,
                activity.ActivityType,
                activity.Outcome,
                activity.PromisedPaymentDate,
                activity.PromisedAmount,
                activity.Notes,
                activity.CreatedAtUtc))
            .Take(10)
            .ToListAsync(cancellationToken);
    }

    private async Task<User> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var currentUserId = currentUserService.UserId
            ?? throw new AppException("Current user was not found.");

        return await dbContext.Users.SingleOrDefaultAsync(user => user.Id == currentUserId, cancellationToken)
            ?? throw new AppException("Current user was not found.");
    }

    private IQueryable<Customer> ApplyCustomerScope(IQueryable<Customer> query, User currentUser)
    {
        return currentUser.Role.HasAdminAccess()
            ? query
            : query.Where(customer => customer.AssignedCreditOfficerId == currentUser.Id);
    }

    private IQueryable<Loan> ApplyLoanScope(IQueryable<Loan> query, User currentUser)
    {
        if (currentUser.Role.HasAdminAccess())
        {
            return query;
        }

        return query.Where(loan => dbContext.Customers.Any(customer =>
            customer.Id == loan.CustomerId &&
            customer.AssignedCreditOfficerId == currentUser.Id));
    }

    private IQueryable<LoanRepaymentSchedule> ApplyScheduleScope(IQueryable<LoanRepaymentSchedule> query, User currentUser)
    {
        if (currentUser.Role.HasAdminAccess())
        {
            return query;
        }

        return query.Where(schedule => dbContext.Loans.Any(loan =>
            loan.Id == schedule.LoanId &&
            dbContext.Customers.Any(customer =>
                customer.Id == loan.CustomerId &&
                customer.AssignedCreditOfficerId == currentUser.Id)));
    }

    private IQueryable<LoanDisbursement> ApplyDisbursementScope(IQueryable<LoanDisbursement> query, User currentUser)
    {
        if (currentUser.Role.HasAdminAccess())
        {
            return query;
        }

        return query.Where(disbursement => dbContext.Customers.Any(customer =>
            customer.Id == disbursement.CustomerId &&
            customer.AssignedCreditOfficerId == currentUser.Id));
    }

    private IQueryable<Repayment> ApplyRepaymentScope(IQueryable<Repayment> query, User currentUser)
    {
        if (currentUser.Role.HasAdminAccess())
        {
            return query;
        }

        return query.Where(repayment => dbContext.Customers.Any(customer =>
            customer.Id == repayment.CustomerId &&
            customer.AssignedCreditOfficerId == currentUser.Id));
    }

    private IQueryable<CollectionActivity> ApplyActivityScope(IQueryable<CollectionActivity> query, User currentUser)
    {
        if (currentUser.Role.HasAdminAccess())
        {
            return query;
        }

        return query.Where(activity => dbContext.Customers.Any(customer =>
            customer.Id == activity.CustomerId &&
            customer.AssignedCreditOfficerId == currentUser.Id));
    }
}

