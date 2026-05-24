using Circle.Application.Abstractions;
using Circle.Application.Common.Constants;
using Circle.Application.Common.Exceptions;
using Circle.Domain.Entities;
using Circle.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Circle.Application.Loans;

public sealed class LoanService(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : ILoanService
{
    private const int MaximumPageSize = 100;

    public async Task<LoanListResponse> GetLoansAsync(
        string? search,
        LoanStatus? status,
        Guid? customerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize <= 0 ? 20 : pageSize, 1, MaximumPageSize);

        var query = ApplyLoanScope(dbContext.Loans.AsNoTracking().Include(loan => loan.RepaymentSchedules), currentUser);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(loan => loan.LoanCode.Contains(normalizedSearch));
        }

        if (status is not null)
        {
            query = query.Where(loan => loan.Status == status);
        }

        if (customerId is not null)
        {
            query = query.Where(loan => loan.CustomerId == customerId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var loans = await query
            .OrderByDescending(loan => loan.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new LoanListResponse(loans.Select(ToResponse).ToList(), page, pageSize, totalCount);
    }

    public async Task<LoanResponse> GetLoanAsync(Guid loanId, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var loan = await ApplyLoanScope(dbContext.Loans.AsNoTracking().Include(candidate => candidate.RepaymentSchedules), currentUser)
            .SingleOrDefaultAsync(candidate => candidate.Id == loanId, cancellationToken)
            ?? throw new AppException("Loan was not found.");

        return ToResponse(loan);
    }

    public async Task<LoanResponse> CreateLoanAsync(CreateLoanRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCreateLoanRequest(request);

        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var customer = await dbContext.Customers.SingleOrDefaultAsync(candidate => candidate.Id == request.CustomerId, cancellationToken)
            ?? throw new AppException("Customer was not found.");

        if (customer.Status != CustomerStatus.Active)
        {
            throw new AppException("Loans can only be created for active customers.");
        }

        if (currentUser.Role == UserRole.CreditOfficer && customer.AssignedCreditOfficerId != currentUser.Id)
        {
            throw new AppException("Credit officers can only create loans for their assigned customers.");
        }

        var interestAmount = Math.Round(request.PrincipalAmount * request.InterestRatePercent / 100m, 2, MidpointRounding.AwayFromZero);
        var totalRepayable = request.PrincipalAmount + interestAmount;
        var maturityDate = CalculateMaturityDate(request.FirstRepaymentDate, request.TermCount, request.RepaymentFrequency);

        var loan = new Loan(
            await GenerateLoanCodeAsync(cancellationToken),
            customer.Id,
            request.PrincipalAmount,
            request.InterestRatePercent,
            interestAmount,
            totalRepayable,
            request.TermCount,
            request.RepaymentFrequency,
            InterestCalculationMethod.FlatRate,
            request.FirstRepaymentDate,
            maturityDate,
            currentUser.Id,
            request.Purpose);

        foreach (var schedule in GenerateRepaymentSchedules(loan, request.FirstRepaymentDate))
        {
            loan.AddRepaymentSchedule(schedule);
        }

        var autoApprovalThreshold = await GetAutoApprovalThresholdAsync(cancellationToken);
        if (loan.PrincipalAmount <= autoApprovalThreshold)
        {
            loan.Approve(currentUser.Id, dateTimeProvider.UtcNow, $"Auto-approved because principal is within the {autoApprovalThreshold:N2} threshold.");
        }

        dbContext.Loans.Add(loan);
        dbContext.AuditLogs.Add(new AuditLog(currentUser.Id, "CreateLoan", $"Loan:{loan.LoanCode}", null, loan.Status.ToString(), currentUserService.IpAddress, null));
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(loan);
    }

    public async Task<LoanResponse> ApproveLoanAsync(Guid loanId, ApproveLoanRequest request, CancellationToken cancellationToken = default)
    {
        var currentUser = await RequireAdminAsync(cancellationToken);
        var loan = await dbContext.Loans
            .Include(candidate => candidate.RepaymentSchedules)
            .SingleOrDefaultAsync(candidate => candidate.Id == loanId, cancellationToken)
            ?? throw new AppException("Loan was not found.");

        if (loan.Status != LoanStatus.PendingOwnerApproval)
        {
            throw new AppException("Only loans pending owner approval can be approved manually.");
        }

        loan.Approve(currentUser.Id, dateTimeProvider.UtcNow, request.Note);
        dbContext.AuditLogs.Add(new AuditLog(currentUser.Id, "ApproveLoan", $"Loan:{loan.LoanCode}", null, loan.Status.ToString(), currentUserService.IpAddress, request.Note));
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(loan);
    }

    public async Task<LoanResponse> RejectLoanAsync(Guid loanId, RejectLoanRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new AppException("Rejection reason is required.");
        }

        var currentUser = await RequireAdminAsync(cancellationToken);
        var loan = await dbContext.Loans
            .Include(candidate => candidate.RepaymentSchedules)
            .SingleOrDefaultAsync(candidate => candidate.Id == loanId, cancellationToken)
            ?? throw new AppException("Loan was not found.");

        if (loan.Status != LoanStatus.PendingOwnerApproval)
        {
            throw new AppException("Only loans pending owner approval can be rejected.");
        }

        loan.Reject(request.Reason);
        dbContext.AuditLogs.Add(new AuditLog(currentUser.Id, "RejectLoan", $"Loan:{loan.LoanCode}", null, loan.Status.ToString(), currentUserService.IpAddress, request.Reason));
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(loan);
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
            throw new AppException("Only admins can approve or reject loans.");
        }

        return currentUser;
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

    private async Task<decimal> GetAutoApprovalThresholdAsync(CancellationToken cancellationToken)
    {
        var settingValue = await dbContext.SystemSettings
            .Where(setting => setting.Key == SystemSettingKeys.AutoApprovalThreshold)
            .Select(setting => setting.Value)
            .SingleOrDefaultAsync(cancellationToken);

        return decimal.TryParse(settingValue, out var threshold) ? threshold : 20000m;
    }

    private async Task<string> GenerateLoanCodeAsync(CancellationToken cancellationToken)
    {
        var prefix = $"LN-{DateTime.UtcNow:yyyyMMdd}";

        for (var attempt = 1; attempt <= 10; attempt++)
        {
            var count = await dbContext.Loans.CountAsync(loan => loan.LoanCode.StartsWith(prefix), cancellationToken);
            var loanCode = $"{prefix}-{count + attempt:0000}";

            if (!await dbContext.Loans.AnyAsync(loan => loan.LoanCode == loanCode, cancellationToken))
            {
                return loanCode;
            }
        }

        throw new AppException("Could not generate a unique loan code. Please try again.");
    }

    private static IReadOnlyCollection<LoanRepaymentSchedule> GenerateRepaymentSchedules(Loan loan, DateOnly firstRepaymentDate)
    {
        var schedules = new List<LoanRepaymentSchedule>();
        var principalInstallment = Math.Round(loan.PrincipalAmount / loan.TermCount, 2, MidpointRounding.AwayFromZero);
        var interestInstallment = Math.Round(loan.InterestAmount / loan.TermCount, 2, MidpointRounding.AwayFromZero);

        var allocatedPrincipal = 0m;
        var allocatedInterest = 0m;

        for (var installmentNumber = 1; installmentNumber <= loan.TermCount; installmentNumber++)
        {
            var isFinalInstallment = installmentNumber == loan.TermCount;
            var principalAmount = isFinalInstallment ? loan.PrincipalAmount - allocatedPrincipal : principalInstallment;
            var interestAmount = isFinalInstallment ? loan.InterestAmount - allocatedInterest : interestInstallment;
            var dueDate = loan.RepaymentFrequency == RepaymentFrequency.Daily
                ? firstRepaymentDate.AddDays(installmentNumber - 1)
                : firstRepaymentDate.AddDays((installmentNumber - 1) * 7);

            allocatedPrincipal += principalAmount;
            allocatedInterest += interestAmount;

            schedules.Add(new LoanRepaymentSchedule(
                loan.Id,
                installmentNumber,
                dueDate,
                principalAmount,
                interestAmount,
                principalAmount + interestAmount));
        }

        return schedules;
    }

    private static DateOnly CalculateMaturityDate(DateOnly firstRepaymentDate, int termCount, RepaymentFrequency repaymentFrequency)
    {
        return repaymentFrequency == RepaymentFrequency.Daily
            ? firstRepaymentDate.AddDays(termCount - 1)
            : firstRepaymentDate.AddDays((termCount - 1) * 7);
    }

    private static void ValidateCreateLoanRequest(CreateLoanRequest request)
    {
        if (request.PrincipalAmount <= 0)
        {
            throw new AppException("Loan principal amount must be greater than zero.");
        }

        if (request.InterestRatePercent < 0)
        {
            throw new AppException("Interest rate cannot be negative.");
        }

        if (request.TermCount <= 0)
        {
            throw new AppException("Loan term count must be greater than zero.");
        }
    }

    private static LoanResponse ToResponse(Loan loan)
    {
        return new LoanResponse(
            loan.Id,
            loan.LoanCode,
            loan.CustomerId,
            loan.PrincipalAmount,
            loan.InterestRatePercent,
            loan.InterestAmount,
            loan.TotalRepayable,
            loan.TermCount,
            loan.RepaymentFrequency,
            loan.InterestCalculationMethod,
            loan.Status,
            loan.StartDate,
            loan.MaturityDate,
            loan.RequestedByUserId,
            loan.ApprovedByUserId,
            loan.ApprovedAtUtc,
            loan.ApprovalNote,
            loan.RejectionReason,
            loan.Purpose,
            loan.CreatedAtUtc,
            loan.UpdatedAtUtc,
            loan.RepaymentSchedules
                .OrderBy(schedule => schedule.InstallmentNumber)
                .Select(schedule => new LoanRepaymentScheduleResponse(
                    schedule.Id,
                    schedule.InstallmentNumber,
                    schedule.DueDate,
                    schedule.PrincipalAmount,
                    schedule.InterestAmount,
                    schedule.TotalAmount,
                    schedule.AmountPaid,
                    schedule.Status,
                    schedule.PaidAtUtc))
                .ToList());
    }
}
