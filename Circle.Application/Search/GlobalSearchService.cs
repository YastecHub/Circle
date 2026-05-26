using Circle.Application.Abstractions;
using Circle.Application.Common.Exceptions;
using Circle.Domain.Entities;
using Circle.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Circle.Application.Search;

public sealed class GlobalSearchService(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService) : IGlobalSearchService
{
    private const int MaximumLimit = 50;

    public async Task<GlobalSearchResponse> SearchAsync(
        string query,
        int limit,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
        {
            throw new AppException("Search query must be at least 2 characters.");
        }

        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var normalizedQuery = query.Trim();
        var take = Math.Clamp(limit <= 0 ? 20 : limit, 1, MaximumLimit);
        var perTypeLimit = Math.Max(5, take);

        var customerResults = await SearchCustomersAsync(normalizedQuery, currentUser, perTypeLimit, cancellationToken);
        var loanResults = await SearchLoansAsync(normalizedQuery, currentUser, perTypeLimit, cancellationToken);
        var virtualAccountResults = await SearchVirtualAccountsAsync(normalizedQuery, currentUser, perTypeLimit, cancellationToken);
        var repaymentResults = await SearchRepaymentsAsync(normalizedQuery, currentUser, perTypeLimit, cancellationToken);
        var inboundPaymentResults = await SearchInboundPaymentsAsync(normalizedQuery, currentUser, perTypeLimit, cancellationToken);

        var results = customerResults
            .Concat(loanResults)
            .Concat(virtualAccountResults)
            .Concat(repaymentResults)
            .Concat(inboundPaymentResults)
            .OrderByDescending(result => result.CreatedAtUtc)
            .Take(take)
            .ToList();

        return new GlobalSearchResponse(normalizedQuery, results);
    }

    private async Task<IReadOnlyCollection<GlobalSearchResultResponse>> SearchCustomersAsync(
        string query,
        User currentUser,
        int limit,
        CancellationToken cancellationToken)
    {
        return await ApplyCustomerScope(dbContext.Customers.AsNoTracking(), currentUser)
            .Where(customer =>
                customer.CustomerCode.Contains(query) ||
                customer.PassbookReference.Contains(query) ||
                customer.FullName.Contains(query) ||
                customer.Phone.Contains(query))
            .OrderByDescending(customer => customer.CreatedAtUtc)
            .Take(limit)
            .Select(customer => new GlobalSearchResultResponse(
                "Customer",
                customer.Id,
                customer.FullName,
                $"{customer.CustomerCode} | {customer.Phone}",
                customer.FullName.Contains(query) ? "FullName" :
                    customer.Phone.Contains(query) ? "Phone" :
                    customer.PassbookReference.Contains(query) ? "PassbookReference" : "CustomerCode",
                customer.Id,
                customer.FullName,
                null,
                null,
                null,
                customer.Status.ToString(),
                customer.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<GlobalSearchResultResponse>> SearchLoansAsync(
        string query,
        User currentUser,
        int limit,
        CancellationToken cancellationToken)
    {
        return await (
            from loan in ApplyLoanScope(dbContext.Loans.AsNoTracking(), currentUser)
            join customer in ApplyCustomerScope(dbContext.Customers.AsNoTracking(), currentUser) on loan.CustomerId equals customer.Id
            where loan.LoanCode.Contains(query) || customer.FullName.Contains(query) || customer.Phone.Contains(query)
            orderby loan.CreatedAtUtc descending
            select new GlobalSearchResultResponse(
                "Loan",
                loan.Id,
                loan.LoanCode,
                $"{customer.FullName} | {loan.PrincipalAmount:N2}",
                loan.LoanCode.Contains(query) ? "LoanCode" :
                    customer.Phone.Contains(query) ? "CustomerPhone" : "CustomerName",
                customer.Id,
                customer.FullName,
                loan.Id,
                loan.LoanCode,
                loan.PrincipalAmount,
                loan.Status.ToString(),
                loan.CreatedAtUtc))
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<GlobalSearchResultResponse>> SearchVirtualAccountsAsync(
        string query,
        User currentUser,
        int limit,
        CancellationToken cancellationToken)
    {
        return await (
            from account in dbContext.CustomerVirtualAccounts.AsNoTracking()
            join customer in ApplyCustomerScope(dbContext.Customers.AsNoTracking(), currentUser) on account.CustomerId equals customer.Id
            where account.AccountNumber.Contains(query) ||
                  account.AccountName.Contains(query) ||
                  account.ProviderReference != null && account.ProviderReference.Contains(query)
            orderby account.CreatedAtUtc descending
            select new GlobalSearchResultResponse(
                "VirtualAccount",
                account.Id,
                account.AccountNumber,
                $"{account.AccountName} | {account.BankName}",
                account.AccountNumber.Contains(query) ? "AccountNumber" :
                    account.ProviderReference != null && account.ProviderReference.Contains(query) ? "ProviderReference" : "AccountName",
                customer.Id,
                customer.FullName,
                null,
                null,
                null,
                account.Status.ToString(),
                account.CreatedAtUtc))
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<GlobalSearchResultResponse>> SearchRepaymentsAsync(
        string query,
        User currentUser,
        int limit,
        CancellationToken cancellationToken)
    {
        return await (
            from repayment in ApplyRepaymentScope(dbContext.Repayments.AsNoTracking(), currentUser)
            join loan in ApplyLoanScope(dbContext.Loans.AsNoTracking(), currentUser) on repayment.LoanId equals loan.Id
            join customer in ApplyCustomerScope(dbContext.Customers.AsNoTracking(), currentUser) on repayment.CustomerId equals customer.Id
            where repayment.Reference != null && repayment.Reference.Contains(query) ||
                  loan.LoanCode.Contains(query) ||
                  customer.FullName.Contains(query)
            orderby repayment.CreatedAtUtc descending
            select new GlobalSearchResultResponse(
                "Repayment",
                repayment.Id,
                repayment.Reference ?? repayment.Method.ToString(),
                $"{customer.FullName} | {repayment.Amount:N2}",
                repayment.Reference != null && repayment.Reference.Contains(query) ? "Reference" :
                    loan.LoanCode.Contains(query) ? "LoanCode" : "CustomerName",
                customer.Id,
                customer.FullName,
                loan.Id,
                loan.LoanCode,
                repayment.Amount,
                repayment.Method.ToString(),
                repayment.CreatedAtUtc))
            .Take(limit)
            .ToListAsync(cancellationToken);
    }

    private async Task<IReadOnlyCollection<GlobalSearchResultResponse>> SearchInboundPaymentsAsync(
        string query,
        User currentUser,
        int limit,
        CancellationToken cancellationToken)
    {
        return await (
            from payment in ApplyInboundPaymentScope(dbContext.InboundPayments.AsNoTracking(), currentUser)
            join customer in ApplyCustomerScope(dbContext.Customers.AsNoTracking(), currentUser) on payment.CustomerId equals customer.Id into customerGroup
            from customer in customerGroup.DefaultIfEmpty()
            join loan in ApplyLoanScope(dbContext.Loans.AsNoTracking(), currentUser) on payment.LoanId equals loan.Id into loanGroup
            from loan in loanGroup.DefaultIfEmpty()
            where payment.AccountNumber.Contains(query) ||
                  payment.ProviderReference != null && payment.ProviderReference!.Contains(query) ||
                  payment.PayerName != null && payment.PayerName!.Contains(query)
            orderby payment.CreatedAtUtc descending
            select new GlobalSearchResultResponse(
                "InboundPayment",
                payment.Id,
                payment.ProviderReference ?? payment.AccountNumber,
                $"{payment.PayerName ?? "Unknown payer"} | {payment.Amount:N2}",
                payment.ProviderReference != null && payment.ProviderReference!.Contains(query) ? "ProviderReference" :
                    payment.PayerName != null && payment.PayerName!.Contains(query) ? "PayerName" : "AccountNumber",
                payment.CustomerId,
                customer == null ? null : customer.FullName,
                payment.LoanId,
                loan == null ? null : loan.LoanCode,
                payment.Amount,
                payment.Status.ToString(),
                payment.CreatedAtUtc))
            .Take(limit)
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
}
