using Circle.Application.Abstractions;
using Circle.Application.Common.Exceptions;
using Circle.Domain.Entities;
using Circle.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Circle.Application.Reports;

public sealed class ReportService(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : IReportService
{
    public async Task<PortfolioSummaryResponse> GetPortfolioSummaryAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var customers = ApplyCustomerScope(dbContext.Customers.AsNoTracking(), currentUser);
        var loans = ApplyLoanScope(dbContext.Loans.AsNoTracking(), currentUser);
        var disbursements = ApplyDisbursementScope(dbContext.LoanDisbursements.AsNoTracking(), currentUser);
        var repayments = ApplyRepaymentScope(dbContext.Repayments.AsNoTracking(), currentUser);
        var schedules = ApplyScheduleScope(dbContext.LoanRepaymentSchedules.AsNoTracking(), currentUser);
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow);

        var totalCustomers = await customers.CountAsync(cancellationToken);
        var activeCustomers = await customers.CountAsync(customer => customer.Status == CustomerStatus.Active, cancellationToken);
        var totalLoans = await loans.CountAsync(cancellationToken);
        var pendingApprovalLoans = await loans.CountAsync(loan => loan.Status == LoanStatus.PendingOwnerApproval, cancellationToken);
        var approvedLoans = await loans.CountAsync(loan => loan.Status == LoanStatus.Approved, cancellationToken);
        var disbursedLoans = await loans.CountAsync(loan => loan.Status == LoanStatus.Disbursed, cancellationToken);
        var closedLoans = await loans.CountAsync(loan => loan.Status == LoanStatus.Closed, cancellationToken);

        var totalPrincipal = await loans.SumAsync(loan => (decimal?)loan.PrincipalAmount, cancellationToken) ?? 0m;
        var totalDisbursed = await disbursements.SumAsync(disbursement => (decimal?)disbursement.Amount, cancellationToken) ?? 0m;
        var totalRepaid = await repayments.SumAsync(repayment => (decimal?)repayment.Amount, cancellationToken) ?? 0m;
        var totalOutstanding = await schedules
            .Where(schedule => schedule.Status != RepaymentScheduleStatus.Paid)
            .SumAsync(schedule => (decimal?)(schedule.TotalAmount - schedule.AmountPaid), cancellationToken) ?? 0m;

        var overdueSchedules = schedules
            .Where(schedule => schedule.Status != RepaymentScheduleStatus.Paid && schedule.DueDate < today);
        var overdueInstallments = await overdueSchedules.CountAsync(cancellationToken);
        var overdueAmount = await overdueSchedules
            .SumAsync(schedule => (decimal?)(schedule.TotalAmount - schedule.AmountPaid), cancellationToken) ?? 0m;

        return new PortfolioSummaryResponse(
            totalCustomers,
            activeCustomers,
            totalLoans,
            pendingApprovalLoans,
            approvedLoans,
            disbursedLoans,
            closedLoans,
            totalPrincipal,
            totalDisbursed,
            totalRepaid,
            totalOutstanding,
            overdueInstallments,
            overdueAmount);
    }

    public async Task<CashflowSummaryResponse> GetCashflowSummaryAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var endDate = to ?? DateOnly.FromDateTime(dateTimeProvider.UtcNow);
        var startDate = from ?? endDate.AddDays(-29);

        if (startDate > endDate)
        {
            throw new AppException("Report start date cannot be after end date.");
        }

        var disbursements = await ApplyDisbursementScope(dbContext.LoanDisbursements.AsNoTracking(), currentUser)
            .Where(disbursement => disbursement.DisbursementDate >= startDate && disbursement.DisbursementDate <= endDate)
            .GroupBy(disbursement => disbursement.DisbursementDate)
            .Select(group => new { Date = group.Key, Amount = group.Sum(disbursement => disbursement.Amount) })
            .ToListAsync(cancellationToken);

        var repayments = await ApplyRepaymentScope(dbContext.Repayments.AsNoTracking(), currentUser)
            .Where(repayment => repayment.PaymentDate >= startDate && repayment.PaymentDate <= endDate)
            .GroupBy(repayment => repayment.PaymentDate)
            .Select(group => new { Date = group.Key, Amount = group.Sum(repayment => repayment.Amount) })
            .ToListAsync(cancellationToken);

        var disbursementsByDate = disbursements.ToDictionary(item => item.Date, item => item.Amount);
        var repaymentsByDate = repayments.ToDictionary(item => item.Date, item => item.Amount);
        var days = new List<CashflowDayResponse>();

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
        {
            var disbursed = disbursementsByDate.GetValueOrDefault(date);
            var repaid = repaymentsByDate.GetValueOrDefault(date);
            days.Add(new CashflowDayResponse(date, disbursed, repaid, repaid - disbursed));
        }

        var totalDisbursed = days.Sum(day => day.Disbursed);
        var totalRepaid = days.Sum(day => day.Repaid);

        return new CashflowSummaryResponse(startDate, endDate, totalDisbursed, totalRepaid, totalRepaid - totalDisbursed, days);
    }

    public async Task<IReadOnlyCollection<OverdueInstallmentResponse>> GetOverdueInstallmentsAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow);

        return await (
            from schedule in ApplyScheduleScope(dbContext.LoanRepaymentSchedules.AsNoTracking(), currentUser)
            join loan in ApplyLoanScope(dbContext.Loans.AsNoTracking(), currentUser) on schedule.LoanId equals loan.Id
            join customer in ApplyCustomerScope(dbContext.Customers.AsNoTracking(), currentUser) on loan.CustomerId equals customer.Id
            where schedule.Status != RepaymentScheduleStatus.Paid && schedule.DueDate < today
            orderby schedule.DueDate, customer.FullName
            select new OverdueInstallmentResponse(
                loan.Id,
                loan.LoanCode,
                customer.Id,
                customer.FullName,
                customer.Phone,
                schedule.InstallmentNumber,
                schedule.DueDate,
                today.DayNumber - schedule.DueDate.DayNumber,
                schedule.TotalAmount,
                schedule.AmountPaid,
                schedule.TotalAmount - schedule.AmountPaid,
                schedule.Status))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<OfficerPerformanceResponse>> GetOfficerPerformanceAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var officersQuery = dbContext.Users.AsNoTracking().Where(user => user.Role == UserRole.CreditOfficer);

        if (currentUser.Role == UserRole.CreditOfficer)
        {
            officersQuery = officersQuery.Where(user => user.Id == currentUser.Id);
        }

        var officers = await officersQuery.OrderBy(user => user.FullName).ToListAsync(cancellationToken);
        var responses = new List<OfficerPerformanceResponse>();

        foreach (var officer in officers)
        {
            var assignedCustomerIds = await dbContext.Customers.AsNoTracking()
                .Where(customer => customer.AssignedCreditOfficerId == officer.Id)
                .Select(customer => customer.Id)
                .ToListAsync(cancellationToken);

            var loans = dbContext.Loans.AsNoTracking().Where(loan => assignedCustomerIds.Contains(loan.CustomerId));
            var loanIds = await loans.Select(loan => loan.Id).ToListAsync(cancellationToken);
            var totalDisbursed = await dbContext.LoanDisbursements.AsNoTracking()
                .Where(disbursement => loanIds.Contains(disbursement.LoanId))
                .SumAsync(disbursement => (decimal?)disbursement.Amount, cancellationToken) ?? 0m;
            var totalRepaid = await dbContext.Repayments.AsNoTracking()
                .Where(repayment => loanIds.Contains(repayment.LoanId))
                .SumAsync(repayment => (decimal?)repayment.Amount, cancellationToken) ?? 0m;
            var outstandingAmount = await dbContext.LoanRepaymentSchedules.AsNoTracking()
                .Where(schedule => loanIds.Contains(schedule.LoanId) && schedule.Status != RepaymentScheduleStatus.Paid)
                .SumAsync(schedule => (decimal?)(schedule.TotalAmount - schedule.AmountPaid), cancellationToken) ?? 0m;

            var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow);
            var overdueAmount = await dbContext.LoanRepaymentSchedules.AsNoTracking()
                .Where(schedule => loanIds.Contains(schedule.LoanId) && schedule.Status != RepaymentScheduleStatus.Paid && schedule.DueDate < today)
                .SumAsync(schedule => (decimal?)(schedule.TotalAmount - schedule.AmountPaid), cancellationToken) ?? 0m;
            var activeLoans = await loans.CountAsync(loan => loan.Status == LoanStatus.Approved || loan.Status == LoanStatus.Disbursed, cancellationToken);

            responses.Add(new OfficerPerformanceResponse(
                officer.Id,
                officer.FullName,
                assignedCustomerIds.Count,
                activeLoans,
                totalDisbursed,
                totalRepaid,
                outstandingAmount,
                overdueAmount));
        }

        return responses;
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
        return currentUser.Role == UserRole.Admin
            ? query
            : query.Where(customer => customer.AssignedCreditOfficerId == currentUser.Id);
    }

    private IQueryable<Loan> ApplyLoanScope(IQueryable<Loan> query, User currentUser)
    {
        if (currentUser.Role == UserRole.Admin)
        {
            return query;
        }

        return query.Where(loan => dbContext.Customers.Any(customer =>
            customer.Id == loan.CustomerId &&
            customer.AssignedCreditOfficerId == currentUser.Id));
    }

    private IQueryable<LoanRepaymentSchedule> ApplyScheduleScope(IQueryable<LoanRepaymentSchedule> query, User currentUser)
    {
        if (currentUser.Role == UserRole.Admin)
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
        if (currentUser.Role == UserRole.Admin)
        {
            return query;
        }

        return query.Where(disbursement => dbContext.Customers.Any(customer =>
            customer.Id == disbursement.CustomerId &&
            customer.AssignedCreditOfficerId == currentUser.Id));
    }

    private IQueryable<Repayment> ApplyRepaymentScope(IQueryable<Repayment> query, User currentUser)
    {
        if (currentUser.Role == UserRole.Admin)
        {
            return query;
        }

        return query.Where(repayment => dbContext.Customers.Any(customer =>
            customer.Id == repayment.CustomerId &&
            customer.AssignedCreditOfficerId == currentUser.Id));
    }
}
