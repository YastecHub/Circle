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

public sealed record RefreshOverdueSchedulesResponse(int UpdatedCount);
