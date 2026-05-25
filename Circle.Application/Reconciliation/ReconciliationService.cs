using Circle.Application.Abstractions;
using Circle.Application.Common.Constants;
using Circle.Application.Common.Exceptions;
using Circle.Domain.Entities;
using Circle.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Circle.Application.Reconciliation;

public sealed class ReconciliationService(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : IReconciliationService
{
    private const int MaximumPageSize = 100;
    private const string MonnifyProvider = "Monnify";

    public async Task<InboundPaymentListResponse> GetInboundPaymentsAsync(
        string? search,
        InboundPaymentStatus? status,
        Guid? customerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize <= 0 ? 20 : pageSize, 1, MaximumPageSize);

        var query = ApplyInboundPaymentScope(dbContext.InboundPayments.AsNoTracking(), currentUser);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(payment =>
                payment.AccountNumber.Contains(normalizedSearch) ||
                (payment.ProviderReference != null && payment.ProviderReference.Contains(normalizedSearch)) ||
                (payment.PayerName != null && payment.PayerName.Contains(normalizedSearch)));
        }

        if (status is not null)
        {
            query = query.Where(payment => payment.Status == status);
        }

        if (customerId is not null)
        {
            query = query.Where(payment => payment.CustomerId == customerId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(payment => payment.PaidAtUtc)
            .ThenByDescending(payment => payment.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(payment => ToResponse(payment))
            .ToListAsync(cancellationToken);

        return new InboundPaymentListResponse(items, page, pageSize, totalCount);
    }

    public async Task<InboundPaymentResponse> RecordInboundPaymentAsync(RecordInboundPaymentRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var currentUser = await RequireAdminAsync(cancellationToken);
        return await RecordInboundPaymentInternalAsync(request, currentUser.Id, currentUserService.IpAddress, "Manual", cancellationToken);
    }

    public async Task<InboundPaymentResponse> RecordSystemInboundPaymentAsync(
        RecordInboundPaymentRequest request,
        string source,
        CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        return await RecordInboundPaymentInternalAsync(request, null, null, source, cancellationToken);
    }

    private async Task<InboundPaymentResponse> RecordInboundPaymentInternalAsync(
        RecordInboundPaymentRequest request,
        Guid? recordedByUserId,
        string? ipAddress,
        string source,
        CancellationToken cancellationToken)
    {
        var provider = await GetConfiguredProviderAsync(cancellationToken);
        var inboundPayment = new InboundPayment(
            provider,
            request.ProviderReference,
            request.AccountNumber,
            request.Amount,
            request.PaidAtUtc,
            request.PayerName,
            request.Narration,
            recordedByUserId);

        if (!string.IsNullOrWhiteSpace(inboundPayment.ProviderReference) &&
            await dbContext.InboundPayments.AnyAsync(payment => payment.Provider == provider && payment.ProviderReference == inboundPayment.ProviderReference, cancellationToken))
        {
            inboundPayment.MarkDuplicate("A payment with this provider reference has already been recorded.");
            dbContext.InboundPayments.Add(inboundPayment);
            await dbContext.SaveChangesAsync(cancellationToken);
            return ToResponse(inboundPayment);
        }

        var virtualAccount = await dbContext.CustomerVirtualAccounts
            .AsNoTracking()
            .SingleOrDefaultAsync(account =>
                account.Provider == provider &&
                account.AccountNumber == inboundPayment.AccountNumber &&
                account.Status == VirtualAccountStatus.Active,
                cancellationToken);

        if (virtualAccount is null)
        {
            inboundPayment.MarkUnmatched("No active customer virtual account matched this payment account number.");
            dbContext.InboundPayments.Add(inboundPayment);
            await dbContext.SaveChangesAsync(cancellationToken);
            return ToResponse(inboundPayment);
        }

        var loan = await dbContext.Loans
            .Include(candidate => candidate.RepaymentSchedules)
            .Where(candidate =>
                candidate.CustomerId == virtualAccount.CustomerId &&
                (candidate.Status == LoanStatus.Approved || candidate.Status == LoanStatus.Disbursed))
            .OrderBy(candidate => candidate.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (loan is null)
        {
            inboundPayment.MarkUnmatched("Customer has no approved or disbursed loan to receive this payment.");
            dbContext.InboundPayments.Add(inboundPayment);
            await dbContext.SaveChangesAsync(cancellationToken);
            return ToResponse(inboundPayment);
        }

        var outstandingBeforePayment = loan.RepaymentSchedules.Sum(schedule => schedule.OutstandingAmount);
        if (outstandingBeforePayment <= 0)
        {
            inboundPayment.MarkUnmatched("Customer loan has no outstanding balance.");
            dbContext.InboundPayments.Add(inboundPayment);
            await dbContext.SaveChangesAsync(cancellationToken);
            return ToResponse(inboundPayment);
        }

        var appliedAmount = Math.Min(request.Amount, outstandingBeforePayment);
        ApplyPaymentToSchedules(loan, appliedAmount);

        if (loan.RepaymentSchedules.All(schedule => schedule.Status == RepaymentScheduleStatus.Paid))
        {
            loan.MarkClosed();
        }

        var repayment = new Repayment(
            loan.Id,
            loan.CustomerId,
            appliedAmount,
            DateOnly.FromDateTime(request.PaidAtUtc),
            RepaymentMethod.Monnify,
            inboundPayment.ProviderReference,
            recordedByUserId,
            "Applied from inbound virtual account payment.");

        dbContext.Repayments.Add(repayment);
        dbContext.InboundPayments.Add(inboundPayment);
        await dbContext.SaveChangesAsync(cancellationToken);

        inboundPayment.Apply(loan.CustomerId, loan.Id, repayment.Id, appliedAmount);
        dbContext.AuditLogs.Add(new AuditLog(recordedByUserId, "ReconcileInboundPayment", $"Loan:{loan.LoanCode}", null, appliedAmount.ToString("F2"), ipAddress, source));
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(inboundPayment);
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
        if (currentUser.Role != UserRole.Admin)
        {
            throw new AppException("Only admins can reconcile inbound payments.");
        }

        return currentUser;
    }

    private IQueryable<InboundPayment> ApplyInboundPaymentScope(IQueryable<InboundPayment> query, User currentUser)
    {
        if (currentUser.Role == UserRole.Admin)
        {
            return query;
        }

        return query.Where(payment => payment.CustomerId != null && dbContext.Customers.Any(customer =>
            customer.Id == payment.CustomerId &&
            customer.AssignedCreditOfficerId == currentUser.Id));
    }

    private async Task<string> GetConfiguredProviderAsync(CancellationToken cancellationToken)
    {
        var provider = await dbContext.SystemSettings
            .Where(setting => setting.Key == SystemSettingKeys.PaymentGateway)
            .Select(setting => setting.Value)
            .SingleOrDefaultAsync(cancellationToken);

        return string.IsNullOrWhiteSpace(provider) ? MonnifyProvider : provider;
    }

    private void ApplyPaymentToSchedules(Loan loan, decimal amount)
    {
        var remainingAmount = amount;

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
    }

    private static void ValidateRequest(RecordInboundPaymentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AccountNumber))
        {
            throw new AppException("Payment account number is required.");
        }

        if (request.Amount <= 0)
        {
            throw new AppException("Payment amount must be greater than zero.");
        }
    }

    private static InboundPaymentResponse ToResponse(InboundPayment payment)
    {
        return new InboundPaymentResponse(
            payment.Id,
            payment.Provider,
            payment.ProviderReference,
            payment.AccountNumber,
            payment.Amount,
            payment.AppliedAmount,
            payment.UnappliedAmount,
            payment.PaidAtUtc,
            payment.PayerName,
            payment.Narration,
            payment.CustomerId,
            payment.LoanId,
            payment.RepaymentId,
            payment.Status,
            payment.FailureReason,
            payment.CreatedAtUtc);
    }
}
