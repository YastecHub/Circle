using Circle.Domain.Enums;

namespace Circle.Application.Disbursements;

public sealed record RecordDisbursementRequest(
    Guid LoanId,
    decimal Amount,
    DateOnly DisbursementDate,
    DisbursementMethod Method,
    string? Reference,
    string? Notes);

public sealed record DisbursementResponse(
    Guid Id,
    Guid LoanId,
    Guid CustomerId,
    decimal Amount,
    DateOnly DisbursementDate,
    DisbursementMethod Method,
    string? Reference,
    Guid DisbursedByUserId,
    string? Notes,
    DateTime CreatedAtUtc);

public sealed record DisbursementListResponse(
    IReadOnlyCollection<DisbursementResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
