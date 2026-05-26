using Circle.Application.Abstractions;
using Circle.Application.Common.Exceptions;
using Circle.Domain.Entities;
using Circle.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Circle.Application.Collections;

public sealed class CollectionService(
    IApplicationDbContext dbContext,
    ICurrentUserService currentUserService,
    IDateTimeProvider dateTimeProvider) : ICollectionService
{
    private const int MaximumPageSize = 100;

    public async Task<CollectionActivityListResponse> GetActivitiesAsync(
        Guid? customerId,
        Guid? loanId,
        Guid? officerUserId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize <= 0 ? 20 : pageSize, 1, MaximumPageSize);

        var query = ApplyActivityScope(dbContext.CollectionActivities.AsNoTracking(), currentUser);

        if (customerId is not null)
        {
            query = query.Where(activity => activity.CustomerId == customerId);
        }

        if (loanId is not null)
        {
            query = query.Where(activity => activity.LoanId == loanId);
        }

        if (officerUserId is not null)
        {
            query = query.Where(activity => activity.OfficerUserId == officerUserId);
        }

        if (fromUtc is not null)
        {
            query = query.Where(activity => activity.CreatedAtUtc >= fromUtc);
        }

        if (toUtc is not null)
        {
            query = query.Where(activity => activity.CreatedAtUtc <= toUtc);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(activity => activity.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(activity => ToResponse(activity))
            .ToListAsync(cancellationToken);

        return new CollectionActivityListResponse(items, page, pageSize, totalCount);
    }

    public async Task<CollectionWorklistResponse> GetWorklistAsync(
        DateOnly? workDate,
        Guid? officerUserId,
        CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var targetDate = workDate ?? DateOnly.FromDateTime(dateTimeProvider.UtcNow);
        var scopedOfficerUserId = currentUser.Role == UserRole.CreditOfficer ? currentUser.Id : officerUserId;

        var scheduleItems = await (
            from schedule in ApplyScheduleScope(dbContext.LoanRepaymentSchedules.AsNoTracking(), currentUser)
            join loan in dbContext.Loans.AsNoTracking() on schedule.LoanId equals loan.Id
            join customer in dbContext.Customers.AsNoTracking() on loan.CustomerId equals customer.Id
            join officer in dbContext.Users.AsNoTracking() on customer.AssignedCreditOfficerId equals officer.Id
            where schedule.Status != RepaymentScheduleStatus.Paid &&
                  schedule.DueDate <= targetDate &&
                  (scopedOfficerUserId == null || customer.AssignedCreditOfficerId == scopedOfficerUserId)
            select new
            {
                CustomerId = customer.Id,
                customer.CustomerCode,
                CustomerName = customer.FullName,
                CustomerPhone = customer.Phone,
                customer.AssignedCreditOfficerId,
                AssignedCreditOfficerName = officer.FullName,
                LoanId = loan.Id,
                loan.LoanCode,
                RepaymentScheduleId = schedule.Id,
                schedule.InstallmentNumber,
                schedule.DueDate,
                AmountDue = schedule.TotalAmount - schedule.AmountPaid
            })
            .ToListAsync(cancellationToken);

        var promiseItems = await (
            from activity in ApplyActivityScope(dbContext.CollectionActivities.AsNoTracking(), currentUser)
            join customer in dbContext.Customers.AsNoTracking() on activity.CustomerId equals customer.Id
            join officer in dbContext.Users.AsNoTracking() on customer.AssignedCreditOfficerId equals officer.Id
            join loan in dbContext.Loans.AsNoTracking() on activity.LoanId equals loan.Id into loanGroup
            from loan in loanGroup.DefaultIfEmpty()
            where activity.Outcome == CollectionOutcome.PromiseToPay &&
                  activity.PromisedPaymentDate != null &&
                  activity.PromisedPaymentDate <= targetDate &&
                  (scopedOfficerUserId == null || customer.AssignedCreditOfficerId == scopedOfficerUserId)
            select new
            {
                ActivityId = activity.Id,
                CustomerId = customer.Id,
                customer.CustomerCode,
                CustomerName = customer.FullName,
                CustomerPhone = customer.Phone,
                customer.AssignedCreditOfficerId,
                AssignedCreditOfficerName = officer.FullName,
                activity.LoanId,
                LoanCode = loan == null ? null : loan.LoanCode,
                activity.RepaymentScheduleId,
                activity.PromisedPaymentDate,
                activity.PromisedAmount,
                activity.Notes,
                activity.CreatedAtUtc
            })
            .ToListAsync(cancellationToken);

        var dueItems = scheduleItems
            .Select(item => new CollectionWorklistItemResponse(
                item.DueDate < targetDate ? "Overdue" : "DueToday",
                item.CustomerId,
                item.CustomerCode,
                item.CustomerName,
                item.CustomerPhone,
                item.AssignedCreditOfficerId,
                item.AssignedCreditOfficerName,
                item.LoanId,
                item.LoanCode,
                item.RepaymentScheduleId,
                item.InstallmentNumber,
                item.DueDate,
                item.DueDate < targetDate ? targetDate.DayNumber - item.DueDate.DayNumber : null,
                item.AmountDue,
                null,
                null,
                null,
                null,
                null))
            .ToList();

        var promiseWorkItems = promiseItems
            .Select(item => new CollectionWorklistItemResponse(
                "PromiseToPay",
                item.CustomerId,
                item.CustomerCode,
                item.CustomerName,
                item.CustomerPhone,
                item.AssignedCreditOfficerId,
                item.AssignedCreditOfficerName,
                item.LoanId,
                item.LoanCode,
                item.RepaymentScheduleId,
                null,
                null,
                item.PromisedPaymentDate < targetDate ? targetDate.DayNumber - item.PromisedPaymentDate.Value.DayNumber : null,
                item.PromisedAmount ?? 0,
                item.ActivityId,
                item.PromisedPaymentDate,
                item.PromisedAmount,
                item.Notes,
                item.CreatedAtUtc))
            .ToList();

        var items = dueItems
            .Concat(promiseWorkItems)
            .OrderBy(item => item.ItemType == "Overdue" ? 0 : item.ItemType == "DueToday" ? 1 : 2)
            .ThenByDescending(item => item.DaysOverdue ?? 0)
            .ThenBy(item => item.CustomerName)
            .ToList();

        return new CollectionWorklistResponse(
            targetDate,
            dueItems.Count(item => item.ItemType == "DueToday"),
            dueItems.Count(item => item.ItemType == "Overdue"),
            promiseWorkItems.Count,
            dueItems.Where(item => item.ItemType == "DueToday").Sum(item => item.AmountDue),
            dueItems.Where(item => item.ItemType == "Overdue").Sum(item => item.AmountDue),
            promiseWorkItems.Sum(item => item.AmountDue),
            items);
    }

    public async Task<CollectionActivityResponse> RecordActivityAsync(RecordCollectionActivityRequest request, CancellationToken cancellationToken = default)
    {
        ValidateRequest(request);

        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var customer = await dbContext.Customers.SingleOrDefaultAsync(candidate => candidate.Id == request.CustomerId, cancellationToken)
            ?? throw new AppException("Customer was not found.");

        if (currentUser.Role == UserRole.CreditOfficer && customer.AssignedCreditOfficerId != currentUser.Id)
        {
            throw new AppException("Credit officers can only record collection activities for assigned customers.");
        }

        if (request.LoanId is not null)
        {
            var loanBelongsToCustomer = await dbContext.Loans.AnyAsync(loan => loan.Id == request.LoanId && loan.CustomerId == customer.Id, cancellationToken);
            if (!loanBelongsToCustomer)
            {
                throw new AppException("Loan does not belong to this customer.");
            }
        }

        if (request.RepaymentScheduleId is not null)
        {
            var scheduleBelongsToLoan = await dbContext.LoanRepaymentSchedules.AnyAsync(schedule =>
                schedule.Id == request.RepaymentScheduleId &&
                request.LoanId != null &&
                schedule.LoanId == request.LoanId,
                cancellationToken);

            if (!scheduleBelongsToLoan)
            {
                throw new AppException("Repayment schedule does not belong to the selected loan.");
            }
        }

        var activity = new CollectionActivity(
            customer.Id,
            request.LoanId,
            request.RepaymentScheduleId,
            currentUser.Id,
            request.ActivityType,
            request.Outcome,
            request.PromisedPaymentDate,
            request.PromisedAmount,
            request.Notes);

        dbContext.CollectionActivities.Add(activity);
        dbContext.AuditLogs.Add(new AuditLog(currentUser.Id, "RecordCollectionActivity", $"Customer:{customer.CustomerCode}", null, request.Outcome.ToString(), currentUserService.IpAddress, request.Notes));
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResponse(activity);
    }

    public async Task<RefreshOverdueSchedulesResponse> RefreshOverdueSchedulesAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        var today = DateOnly.FromDateTime(dateTimeProvider.UtcNow);

        var query = dbContext.LoanRepaymentSchedules
            .Where(schedule => schedule.Status == RepaymentScheduleStatus.Pending && schedule.DueDate < today);

        if (currentUser.Role == UserRole.CreditOfficer)
        {
            query = query.Where(schedule => dbContext.Loans.Any(loan =>
                loan.Id == schedule.LoanId &&
                dbContext.Customers.Any(customer =>
                    customer.Id == loan.CustomerId &&
                    customer.AssignedCreditOfficerId == currentUser.Id)));
        }

        var schedules = await query.ToListAsync(cancellationToken);
        var updatedCount = 0;

        foreach (var schedule in schedules)
        {
            if (schedule.MarkOverdue(today))
            {
                updatedCount++;
            }
        }

        if (updatedCount > 0)
        {
            dbContext.AuditLogs.Add(new AuditLog(currentUser.Id, "RefreshOverdueSchedules", "LoanRepaymentSchedules", null, updatedCount.ToString(), currentUserService.IpAddress, null));
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return new RefreshOverdueSchedulesResponse(updatedCount);
    }

    private async Task<User> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var currentUserId = currentUserService.UserId
            ?? throw new AppException("Current user was not found.");

        return await dbContext.Users.SingleOrDefaultAsync(user => user.Id == currentUserId, cancellationToken)
            ?? throw new AppException("Current user was not found.");
    }

    private IQueryable<CollectionActivity> ApplyActivityScope(IQueryable<CollectionActivity> query, User currentUser)
    {
        if (currentUser.Role == UserRole.Admin)
        {
            return query;
        }

        return query.Where(activity => dbContext.Customers.Any(customer =>
            customer.Id == activity.CustomerId &&
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

    private static void ValidateRequest(RecordCollectionActivityRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Notes))
        {
            throw new AppException("Collection activity notes are required.");
        }

        if (request.PromisedAmount is < 0)
        {
            throw new AppException("Promised amount cannot be negative.");
        }

        if (request.RepaymentScheduleId is not null && request.LoanId is null)
        {
            throw new AppException("Loan is required when a repayment schedule is selected.");
        }
    }

    private static CollectionActivityResponse ToResponse(CollectionActivity activity)
    {
        return new CollectionActivityResponse(
            activity.Id,
            activity.CustomerId,
            activity.LoanId,
            activity.RepaymentScheduleId,
            activity.OfficerUserId,
            activity.ActivityType,
            activity.Outcome,
            activity.PromisedPaymentDate,
            activity.PromisedAmount,
            activity.Notes,
            activity.CreatedAtUtc);
    }
}
