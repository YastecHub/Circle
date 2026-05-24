using Circle.Application.Abstractions;
using Circle.Application.Common.Constants;
using Circle.Application.Common.Exceptions;
using Circle.Domain.Entities;
using Circle.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Circle.Application.VirtualAccounts;

public sealed class VirtualAccountService(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService) : IVirtualAccountService
{
    private const int MaximumPageSize = 100;
    private const string MonnifyProvider = "Monnify";

    public async Task<CustomerVirtualAccountListResponse> GetVirtualAccountsAsync(
        string? search,
        Guid? customerId,
        VirtualAccountStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize <= 0 ? 20 : pageSize, 1, MaximumPageSize);

        var query = ApplyVirtualAccountScope(dbContext.CustomerVirtualAccounts.AsNoTracking(), currentUser);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(account =>
                account.AccountName.Contains(normalizedSearch) ||
                account.AccountNumber.Contains(normalizedSearch) ||
                account.BankName.Contains(normalizedSearch) ||
                (account.ProviderReference != null && account.ProviderReference.Contains(normalizedSearch)));
        }

        if (customerId is not null)
        {
            query = query.Where(account => account.CustomerId == customerId);
        }

        if (status is not null)
        {
            query = query.Where(account => account.Status == status);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(account => account.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(account => ToResponse(account))
            .ToListAsync(cancellationToken);

        return new CustomerVirtualAccountListResponse(items, page, pageSize, totalCount);
    }

    public async Task<CustomerVirtualAccountResponse> GetCustomerVirtualAccountAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        await EnsureCanAccessCustomerAsync(customerId, currentUser, cancellationToken);

        var account = await dbContext.CustomerVirtualAccounts
            .AsNoTracking()
            .Where(candidate => candidate.CustomerId == customerId && candidate.Status == VirtualAccountStatus.Active)
            .OrderByDescending(candidate => candidate.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new AppException("Customer does not have an active virtual account.");

        return ToResponse(account);
    }

    public async Task<CustomerVirtualAccountResponse> RegisterCustomerVirtualAccountAsync(Guid customerId, RegisterVirtualAccountRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var currentUser = await RequireAdminAsync(cancellationToken);
        var customer = await EnsureCanAccessCustomerAsync(customerId, currentUser, cancellationToken);
        var provider = await GetConfiguredProviderAsync(cancellationToken);

        if (provider != MonnifyProvider)
        {
            throw new AppException("Only Monnify virtual accounts are supported for the MVP.");
        }

        if (await dbContext.CustomerVirtualAccounts.AnyAsync(account => account.CustomerId == customer.Id && account.Status == VirtualAccountStatus.Active, cancellationToken))
        {
            throw new AppException("Customer already has an active virtual account.");
        }

        if (await dbContext.CustomerVirtualAccounts.AnyAsync(account => account.AccountNumber == request.AccountNumber.Trim() && account.Status == VirtualAccountStatus.Active, cancellationToken))
        {
            throw new AppException("This account number is already assigned to another active customer virtual account.");
        }

        var virtualAccount = new CustomerVirtualAccount(
            customer.Id,
            provider,
            request.AccountName,
            request.AccountNumber,
            request.BankName,
            request.BankCode,
            request.ProviderReference,
            currentUser.Id);

        dbContext.CustomerVirtualAccounts.Add(virtualAccount);
        dbContext.AuditLogs.Add(new AuditLog(currentUser.Id, "RegisterVirtualAccount", $"Customer:{customer.CustomerCode}", null, virtualAccount.AccountNumber, currentUserService.IpAddress, provider));
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(virtualAccount);
    }

    public async Task<CustomerVirtualAccountResponse> DeactivateCustomerVirtualAccountAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var currentUser = await RequireAdminAsync(cancellationToken);
        var customer = await EnsureCanAccessCustomerAsync(customerId, currentUser, cancellationToken);
        var account = await dbContext.CustomerVirtualAccounts
            .Where(candidate => candidate.CustomerId == customerId && candidate.Status == VirtualAccountStatus.Active)
            .OrderByDescending(candidate => candidate.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new AppException("Customer does not have an active virtual account.");

        account.Deactivate();
        dbContext.AuditLogs.Add(new AuditLog(currentUser.Id, "DeactivateVirtualAccount", $"Customer:{customer.CustomerCode}", account.AccountNumber, null, currentUserService.IpAddress, account.Provider));
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(account);
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
            throw new AppException("Only admins can manage customer virtual accounts.");
        }

        return currentUser;
    }

    private async Task<Customer> EnsureCanAccessCustomerAsync(Guid customerId, User currentUser, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Customers.SingleOrDefaultAsync(candidate => candidate.Id == customerId, cancellationToken)
            ?? throw new AppException("Customer was not found.");

        if (currentUser.Role == UserRole.CreditOfficer && customer.AssignedCreditOfficerId != currentUser.Id)
        {
            throw new AppException("Credit officers can only view virtual accounts for assigned customers.");
        }

        return customer;
    }

    private IQueryable<CustomerVirtualAccount> ApplyVirtualAccountScope(IQueryable<CustomerVirtualAccount> query, User currentUser)
    {
        if (currentUser.Role == UserRole.Admin)
        {
            return query;
        }

        return query.Where(account => dbContext.Customers.Any(customer =>
            customer.Id == account.CustomerId &&
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

    private static void ValidateRequest(RegisterVirtualAccountRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.AccountName))
        {
            throw new AppException("Virtual account name is required.");
        }

        if (string.IsNullOrWhiteSpace(request.AccountNumber))
        {
            throw new AppException("Virtual account number is required.");
        }

        if (string.IsNullOrWhiteSpace(request.BankName))
        {
            throw new AppException("Virtual account bank name is required.");
        }
    }

    private static CustomerVirtualAccountResponse ToResponse(CustomerVirtualAccount account)
    {
        return new CustomerVirtualAccountResponse(
            account.Id,
            account.CustomerId,
            account.Provider,
            account.AccountName,
            account.AccountNumber,
            account.BankName,
            account.BankCode,
            account.ProviderReference,
            account.CreatedByUserId,
            account.Status,
            account.CreatedAtUtc,
            account.UpdatedAtUtc);
    }
}
