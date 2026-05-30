using Circle.Application.Abstractions;
using Circle.Application.Common.Exceptions;
using Circle.Domain.Entities;
using Circle.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Circle.Application.Repayments;

public sealed class RepaymentService(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : IRepaymentService
{
    private const int MaximumPageSize = 100;

    public async Task<RepaymentListResponse> GetRepaymentsAsync(
        Guid? loanId,
        Guid? customerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize <= 0 ? 20 : pageSize, 1, MaximumPageSize);

        var query = ApplyRepaymentScope(dbContext.Repayments.AsNoTracking(), currentUser);

        if (loanId is not null)
        {
            query = query.Where(repayment => repayment.LoanId == loanId);
        }

        if (customerId is not null)
        {
            query = query.Where(repayment => repayment.CustomerId == customerId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(repayment => repayment.PaymentDate)
            .ThenByDescending(repayment => repayment.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(repayment => ToResponse(repayment))
            .ToListAsync(cancellationToken);

        return new RepaymentListResponse(items, page, pageSize, totalCount);
    }

    public async Task<RepaymentResponse> RecordRepaymentAsync(RecordRepaymentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new AppException("Repayment amount must be greater than zero.");
        }

        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var loan = await dbContext.Loans
            .Include(candidate => candidate.RepaymentSchedules)
            .SingleOrDefaultAsync(candidate => candidate.Id == request.LoanId, cancellationToken)
            ?? throw new AppException("Loan was not found.");

        var customer = await dbContext.Customers.SingleOrDefaultAsync(candidate => candidate.Id == loan.CustomerId, cancellationToken)
            ?? throw new AppException("Customer was not found.");

        if (currentUser.Role == UserRole.CreditOfficer && customer.AssignedCreditOfficerId != currentUser.Id)
        {
            throw new AppException("Credit officers can only record repayments for assigned customers.");
        }

        if (loan.Status is not LoanStatus.Approved and not LoanStatus.Disbursed)
        {
            throw new AppException("Repayments can only be recorded for approved or disbursed loans.");
        }

        var outstandingBeforePayment = loan.RepaymentSchedules.Sum(schedule => schedule.OutstandingAmount);
        if (request.Amount > outstandingBeforePayment)
        {
            throw new AppException("Repayment amount cannot exceed the loan outstanding balance.");
        }

        var remainingAmount = request.Amount;
        foreach (var schedule in loan.RepaymentSchedules
                     .Where(schedule => schedule.Status != RepaymentScheduleStatus.Paid)
                     .OrderBy(schedule => schedule.DueDate)
                     .ThenBy(schedule => schedule.InstallmentNumber))
        {
            remainingAmount = schedule.ApplyPayment(remainingAmount, dateTimeProvider.UtcNow);
            if (remainingAmount <= 0)
            {
                break;
            }
        }

        if (loan.RepaymentSchedules.All(schedule => schedule.Status == RepaymentScheduleStatus.Paid))
        {
            loan.MarkClosed();
        }

        var repayment = new Repayment(
            loan.Id,
            loan.CustomerId,
            request.Amount,
            request.PaymentDate,
            request.Method,
            request.Reference,
            currentUser.Id,
            request.Notes);

        dbContext.Repayments.Add(repayment);
        dbContext.AuditLogs.Add(new AuditLog(currentUser.Id, "RecordRepayment", $"Loan:{loan.LoanCode}", null, request.Amount.ToString("F2"), currentUserService.IpAddress, request.Notes));
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(repayment);
    }

    private async Task<User> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var currentUserId = currentUserService.UserId
            ?? throw new AppException("Current user was not found.");

        return await dbContext.Users.SingleOrDefaultAsync(user => user.Id == currentUserId, cancellationToken)
            ?? throw new AppException("Current user was not found.");
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

    private static RepaymentResponse ToResponse(Repayment repayment)
    {
        return new RepaymentResponse(
            repayment.Id,
            repayment.LoanId,
            repayment.CustomerId,
            repayment.Amount,
            repayment.PaymentDate,
            repayment.Method,
            repayment.Reference,
            repayment.RecordedByUserId,
            repayment.Notes,
            repayment.CreatedAtUtc);
    }
}

