using Circle.Application.Abstractions;
using Circle.Application.Common.Exceptions;
using Circle.Domain.Entities;
using Circle.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Circle.Application.Disbursements;

public sealed class DisbursementService(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService) : IDisbursementService
{
    private const int MaximumPageSize = 100;

    public async Task<DisbursementListResponse> GetDisbursementsAsync(
        Guid? loanId,
        Guid? customerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize <= 0 ? 20 : pageSize, 1, MaximumPageSize);

        var query = ApplyDisbursementScope(dbContext.LoanDisbursements.AsNoTracking(), currentUser);

        if (loanId is not null)
        {
            query = query.Where(disbursement => disbursement.LoanId == loanId);
        }

        if (customerId is not null)
        {
            query = query.Where(disbursement => disbursement.CustomerId == customerId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(disbursement => disbursement.DisbursementDate)
            .ThenByDescending(disbursement => disbursement.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(disbursement => ToResponse(disbursement))
            .ToListAsync(cancellationToken);

        return new DisbursementListResponse(items, page, pageSize, totalCount);
    }

    public async Task<DisbursementResponse> RecordDisbursementAsync(RecordDisbursementRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
        {
            throw new AppException("Disbursement amount must be greater than zero.");
        }

        var currentUser = await RequireAdminAsync(cancellationToken);
        var loan = await dbContext.Loans.SingleOrDefaultAsync(candidate => candidate.Id == request.LoanId, cancellationToken)
            ?? throw new AppException("Loan was not found.");

        if (loan.Status != LoanStatus.Approved)
        {
            throw new AppException("Only approved loans can be disbursed.");
        }

        if (request.Amount != loan.PrincipalAmount)
        {
            throw new AppException("Disbursement amount must match the approved loan principal.");
        }

        if (await dbContext.LoanDisbursements.AnyAsync(disbursement => disbursement.LoanId == loan.Id, cancellationToken))
        {
            throw new AppException("This loan has already been disbursed.");
        }

        var disbursement = new LoanDisbursement(
            loan.Id,
            loan.CustomerId,
            request.Amount,
            request.DisbursementDate,
            request.Method,
            request.Reference,
            currentUser.Id,
            request.Notes);

        loan.MarkDisbursed();

        dbContext.LoanDisbursements.Add(disbursement);
        dbContext.AuditLogs.Add(new AuditLog(currentUser.Id, "RecordDisbursement", $"Loan:{loan.LoanCode}", null, request.Amount.ToString("F2"), currentUserService.IpAddress, request.Notes));
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(disbursement);
    }

    private async Task<User> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var currentUserId = currentUserService.UserId
            ?? throw new AppException("Current user was not found.");

        return await dbContext.Users.SingleOrDefaultAsync(user => user.Id == currentUserId, cancellationToken)
            ?? throw new AppException("Current user was not found.");
    }

    private async Task<User> RequireAdminAsync(CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        if (!currentUser.Role.HasAdminAccess())
        {
            throw new AppException("Only admins can record disbursements.");
        }

        return currentUser;
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

    private static DisbursementResponse ToResponse(LoanDisbursement disbursement)
    {
        return new DisbursementResponse(
            disbursement.Id,
            disbursement.LoanId,
            disbursement.CustomerId,
            disbursement.Amount,
            disbursement.DisbursementDate,
            disbursement.Method,
            disbursement.Reference,
            disbursement.DisbursedByUserId,
            disbursement.Notes,
            disbursement.CreatedAtUtc);
    }
}

