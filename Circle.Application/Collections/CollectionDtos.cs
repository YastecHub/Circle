using Circle.Domain.Enums;

namespace Circle.Application.Collections;

public sealed record RecordCollectionActivityRequest(
    Guid CustomerId,
    Guid? LoanId,
    Guid? RepaymentScheduleId,
    CollectionActivityType ActivityType,
    CollectionOutcome Outcome,
    DateOnly? PromisedPaymentDate,
    decimal? PromisedAmount,
    string Notes);

public sealed record CollectionActivityResponse(
    Guid Id,
    Guid CustomerId,
    Guid? LoanId,
    Guid? RepaymentScheduleId,
    Guid OfficerUserId,
    CollectionActivityType ActivityType,
    CollectionOutcome Outcome,
    DateOnly? PromisedPaymentDate,
    decimal? PromisedAmount,
    string Notes,
    DateTime CreatedAtUtc);

public sealed record CollectionActivityListResponse(
    IReadOnlyCollection<CollectionActivityResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record CollectionWorklistResponse(
    DateOnly WorkDate,
    int DueTodayCount,
    int OverdueCount,
    int PromiseToPayCount,
    decimal DueTodayAmount,
    decimal OverdueAmount,
    decimal PromiseToPayAmount,
    IReadOnlyCollection<CollectionWorklistItemResponse> Items);

public sealed record CollectionWorklistItemResponse(
    string ItemType,
    Guid CustomerId,
    string CustomerCode,
    string CustomerName,
    string CustomerPhone,
    Guid AssignedCreditOfficerId,
    string AssignedCreditOfficerName,
    Guid? LoanId,
    string? LoanCode,
    Guid? RepaymentScheduleId,
    int? InstallmentNumber,
    DateOnly? DueDate,
    int? DaysOverdue,
    decimal AmountDue,
    Guid? CollectionActivityId,
    DateOnly? PromisedPaymentDate,
    decimal? PromisedAmount,
    string? LastNote,
    DateTime? LastActivityAtUtc);

public sealed record RefreshOverdueSchedulesResponse(int UpdatedCount);
