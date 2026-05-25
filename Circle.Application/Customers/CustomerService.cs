using Circle.Application.Abstractions;
using Circle.Application.Common.Exceptions;
using Circle.Domain.Entities;
using Circle.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Circle.Application.Customers;

public sealed class CustomerService(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : ICustomerService
{
    private const int MaximumPageSize = 100;

    public async Task<CustomerListResponse> GetCustomersAsync(
        string? search,
        CustomerStatus? status,
        Guid? assignedCreditOfficerId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize <= 0 ? 20 : pageSize, 1, MaximumPageSize);

        var query = ApplyCustomerScope(dbContext.Customers.AsNoTracking(), currentUser);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim();
            query = query.Where(customer =>
                customer.CustomerCode.Contains(normalizedSearch) ||
                customer.PassbookReference.Contains(normalizedSearch) ||
                customer.FullName.Contains(normalizedSearch) ||
                customer.Phone.Contains(normalizedSearch));
        }

        if (status is not null)
        {
            query = query.Where(customer => customer.Status == status);
        }

        if (assignedCreditOfficerId is not null)
        {
            query = query.Where(customer => customer.AssignedCreditOfficerId == assignedCreditOfficerId);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(customer => customer.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(customer => ToResponse(customer))
            .ToListAsync(cancellationToken);

        return new CustomerListResponse(items, page, pageSize, totalCount);
    }

    public async Task<CustomerResponse> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var customer = await ApplyCustomerScope(dbContext.Customers.AsNoTracking(), currentUser)
            .SingleOrDefaultAsync(candidate => candidate.Id == customerId, cancellationToken)
            ?? throw new AppException("Customer was not found.");

        return ToResponse(customer);
    }

    public async Task<CustomerStatementResponse> GetCustomerStatementAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var customer = await ApplyCustomerScope(dbContext.Customers.AsNoTracking(), currentUser)
            .SingleOrDefaultAsync(candidate => candidate.Id == customerId, cancellationToken)
            ?? throw new AppException("Customer was not found.");

        var loans = await dbContext.Loans
            .AsNoTracking()
            .Include(loan => loan.RepaymentSchedules)
            .Where(loan => loan.CustomerId == customer.Id)
            .OrderByDescending(loan => loan.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var repayments = await dbContext.Repayments
            .AsNoTracking()
            .Where(repayment => repayment.CustomerId == customer.Id)
            .OrderByDescending(repayment => repayment.PaymentDate)
            .ThenByDescending(repayment => repayment.CreatedAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        var totalPaidByLoan = await dbContext.Repayments
            .AsNoTracking()
            .Where(repayment => repayment.CustomerId == customer.Id)
            .GroupBy(repayment => repayment.LoanId)
            .Select(group => new { LoanId = group.Key, TotalPaid = group.Sum(repayment => repayment.Amount) })
            .ToDictionaryAsync(item => item.LoanId, item => item.TotalPaid, cancellationToken);

        var totalDisbursed = await dbContext.LoanDisbursements
            .AsNoTracking()
            .Where(disbursement => disbursement.CustomerId == customer.Id)
            .SumAsync(disbursement => disbursement.Amount, cancellationToken);

        var activeVirtualAccount = await dbContext.CustomerVirtualAccounts
            .AsNoTracking()
            .Where(account => account.CustomerId == customer.Id && account.Status == VirtualAccountStatus.Active)
            .OrderByDescending(account => account.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var collectionActivities = await dbContext.CollectionActivities
            .AsNoTracking()
            .Where(activity => activity.CustomerId == customer.Id)
            .OrderByDescending(activity => activity.CreatedAtUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        var documents = await dbContext.CustomerDocuments
            .AsNoTracking()
            .Where(document => document.CustomerId == customer.Id)
            .OrderByDescending(document => document.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow);
        var loanLines = loans.Select(loan => ToStatementLoanLine(loan, totalPaidByLoan.GetValueOrDefault(loan.Id), today)).ToList();
        var nextDueLoan = loanLines
            .Where(loan => loan.NextDueDate is not null)
            .OrderBy(loan => loan.NextDueDate)
            .FirstOrDefault();

        var summary = new CustomerStatementSummary(
            loans.Count,
            loans.Count(loan => loan.Status is LoanStatus.Approved or LoanStatus.Disbursed),
            loans.Sum(loan => loan.PrincipalAmount),
            loans.Sum(loan => loan.TotalRepayable),
            totalDisbursed,
            totalPaidByLoan.Values.Sum(),
            loanLines.Sum(loan => loan.OutstandingBalance),
            loanLines.Sum(loan => loan.OverdueBalance),
            nextDueLoan?.NextDueDate,
            nextDueLoan?.NextDueAmount);

        return new CustomerStatementResponse(
            ToResponse(customer),
            summary,
            activeVirtualAccount is null ? null : new CustomerStatementVirtualAccount(
                activeVirtualAccount.Id,
                activeVirtualAccount.Provider,
                activeVirtualAccount.AccountName,
                activeVirtualAccount.AccountNumber,
                activeVirtualAccount.BankName,
                activeVirtualAccount.BankCode,
                activeVirtualAccount.ProviderReference,
                activeVirtualAccount.Status),
            loanLines,
            repayments.Select(repayment => new CustomerStatementRepaymentLine(
                repayment.Id,
                repayment.LoanId,
                repayment.Amount,
                repayment.PaymentDate,
                repayment.Method,
                repayment.Reference,
                repayment.Notes,
                repayment.CreatedAtUtc)).ToList(),
            collectionActivities.Select(activity => new CustomerStatementCollectionLine(
                activity.Id,
                activity.LoanId,
                activity.RepaymentScheduleId,
                activity.OfficerUserId,
                activity.ActivityType,
                activity.Outcome,
                activity.PromisedPaymentDate,
                activity.PromisedAmount,
                activity.Notes,
                activity.CreatedAtUtc)).ToList(),
            documents.Select(document => new CustomerStatementDocumentLine(
                document.Id,
                document.DocumentType,
                document.FileName,
                document.ContentType,
                document.UploadedByUserId,
                document.CreatedAtUtc)).ToList());
    }

    public async Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCustomerFields(request.FullName, request.Phone, request.Address);

        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var assignedCreditOfficerId = await ResolveAssignedCreditOfficerIdAsync(
            request.AssignedCreditOfficerId,
            currentUser,
            cancellationToken);

        var customerCode = string.IsNullOrWhiteSpace(request.CustomerCode)
            ? await GenerateCustomerCodeAsync(cancellationToken)
            : request.CustomerCode.Trim();

        if (await dbContext.Customers.AnyAsync(customer => customer.CustomerCode == customerCode, cancellationToken))
        {
            throw new AppException("A customer with this customer code already exists.");
        }

        var passbookReference = string.IsNullOrWhiteSpace(request.PassbookReference)
            ? customerCode
            : request.PassbookReference.Trim();

        var customer = new Customer(
            customerCode,
            passbookReference,
            request.FullName,
            request.Phone,
            request.Address,
            assignedCreditOfficerId);

        customer.UpdateProfile(
            request.FullName,
            request.Phone,
            request.AlternatePhone,
            request.Address,
            request.City,
            request.State,
            request.Occupation,
            request.Notes);

        customer.SetKyc(
            request.GovernmentIdType,
            request.GovernmentIdNumber,
            request.DateOfBirth,
            request.Gender);

        customer.SetGuarantor(
            request.GuarantorName,
            request.GuarantorPhone,
            request.GuarantorRelationship,
            request.GuarantorIdNumber);

        dbContext.Customers.Add(customer);
        dbContext.AuditLogs.Add(new AuditLog(currentUser.Id, "CreateCustomer", $"Customer:{customer.CustomerCode}", null, customer.FullName, currentUserService.IpAddress, null));
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(customer);
    }

    public async Task<CustomerResponse> UpdateCustomerAsync(Guid customerId, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        ValidateCustomerFields(request.FullName, request.Phone, request.Address);

        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var customer = await ApplyCustomerScope(dbContext.Customers, currentUser)
            .SingleOrDefaultAsync(candidate => candidate.Id == customerId, cancellationToken)
            ?? throw new AppException("Customer was not found.");

        var assignedCreditOfficerId = await ResolveAssignedCreditOfficerIdAsync(
            request.AssignedCreditOfficerId ?? customer.AssignedCreditOfficerId,
            currentUser,
            cancellationToken);

        customer.UpdateProfile(
            request.FullName,
            request.Phone,
            request.AlternatePhone,
            request.Address,
            request.City,
            request.State,
            request.Occupation,
            request.Notes);

        customer.SetKyc(
            request.GovernmentIdType,
            request.GovernmentIdNumber,
            request.DateOfBirth,
            request.Gender);

        customer.SetGuarantor(
            request.GuarantorName,
            request.GuarantorPhone,
            request.GuarantorRelationship,
            request.GuarantorIdNumber);

        customer.AssignCreditOfficer(assignedCreditOfficerId);

        dbContext.AuditLogs.Add(new AuditLog(currentUser.Id, "UpdateCustomer", $"Customer:{customer.CustomerCode}", null, customer.FullName, currentUserService.IpAddress, null));
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(customer);
    }

    public async Task<CustomerResponse> ArchiveCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var customer = await ApplyCustomerScope(dbContext.Customers, currentUser)
            .SingleOrDefaultAsync(candidate => candidate.Id == customerId, cancellationToken)
            ?? throw new AppException("Customer was not found.");

        customer.Archive();
        dbContext.AuditLogs.Add(new AuditLog(currentUser.Id, "ArchiveCustomer", $"Customer:{customer.CustomerCode}", null, CustomerStatus.Archived.ToString(), currentUserService.IpAddress, null));
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(customer);
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

    private async Task<Guid> ResolveAssignedCreditOfficerIdAsync(Guid? requestedCreditOfficerId, User currentUser, CancellationToken cancellationToken)
    {
        if (currentUser.Role == UserRole.CreditOfficer)
        {
            if (requestedCreditOfficerId is not null && requestedCreditOfficerId != currentUser.Id)
            {
                throw new AppException("Credit officers can only assign customers to themselves.");
            }

            return currentUser.Id;
        }

        if (requestedCreditOfficerId is null)
        {
            throw new AppException("Assigned credit officer is required.");
        }

        var assignedUser = await dbContext.Users.SingleOrDefaultAsync(user => user.Id == requestedCreditOfficerId, cancellationToken)
            ?? throw new AppException("Assigned credit officer was not found.");

        if (assignedUser.Role != UserRole.CreditOfficer || assignedUser.Status != UserStatus.Active)
        {
            throw new AppException("Assigned user must be an active credit officer.");
        }

        return assignedUser.Id;
    }

    private async Task<string> GenerateCustomerCodeAsync(CancellationToken cancellationToken)
    {
        var prefix = $"CUS-{DateTime.UtcNow:yyyyMMdd}";

        for (var attempt = 1; attempt <= 10; attempt++)
        {
            var count = await dbContext.Customers.CountAsync(customer => customer.CustomerCode.StartsWith(prefix), cancellationToken);
            var customerCode = $"{prefix}-{count + attempt:0000}";

            if (!await dbContext.Customers.AnyAsync(customer => customer.CustomerCode == customerCode, cancellationToken))
            {
                return customerCode;
            }
        }

        throw new AppException("Could not generate a unique customer code. Please provide one manually.");
    }

    private static void ValidateCustomerFields(string fullName, string phone, string address)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new AppException("Customer full name is required.");
        }

        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new AppException("Customer phone number is required.");
        }

        if (string.IsNullOrWhiteSpace(address))
        {
            throw new AppException("Customer address is required.");
        }
    }

    private static CustomerResponse ToResponse(Customer customer)
    {
        return new CustomerResponse(
            customer.Id,
            customer.CustomerCode,
            customer.PassbookReference,
            customer.FullName,
            customer.Phone,
            customer.AlternatePhone,
            customer.Address,
            customer.City,
            customer.State,
            customer.GovernmentIdType,
            customer.GovernmentIdNumber,
            customer.DateOfBirth,
            customer.Gender,
            customer.Occupation,
            customer.GuarantorName,
            customer.GuarantorPhone,
            customer.GuarantorRelationship,
            customer.GuarantorIdNumber,
            customer.AssignedCreditOfficerId,
            customer.Status,
            customer.Notes,
            customer.CreatedAtUtc,
            customer.UpdatedAtUtc);
    }

    private static CustomerStatementLoanLine ToStatementLoanLine(Loan loan, decimal totalPaid, DateOnly today)
    {
        var outstandingBalance = loan.RepaymentSchedules.Sum(schedule => schedule.OutstandingAmount);
        var overdueBalance = loan.RepaymentSchedules
            .Where(schedule => schedule.Status != RepaymentScheduleStatus.Paid && schedule.DueDate < today)
            .Sum(schedule => schedule.OutstandingAmount);
        var nextDueSchedule = loan.RepaymentSchedules
            .Where(schedule => schedule.OutstandingAmount > 0)
            .OrderBy(schedule => schedule.DueDate)
            .ThenBy(schedule => schedule.InstallmentNumber)
            .FirstOrDefault();

        return new CustomerStatementLoanLine(
            loan.Id,
            loan.LoanCode,
            loan.Status,
            loan.PrincipalAmount,
            loan.TotalRepayable,
            totalPaid,
            outstandingBalance,
            overdueBalance,
            loan.StartDate,
            loan.MaturityDate,
            nextDueSchedule?.DueDate,
            nextDueSchedule?.OutstandingAmount);
    }
}
