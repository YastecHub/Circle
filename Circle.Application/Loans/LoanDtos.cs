using Circle.Domain.Enums;

namespace Circle.Application.Loans;

public sealed record CreateLoanRequest(
    Guid CustomerId,
    decimal PrincipalAmount,
    decimal InterestRatePercent,
    int TermCount,
    RepaymentFrequency RepaymentFrequency,
    DateOnly FirstRepaymentDate,
    string? Purpose);

public sealed record ApproveLoanRequest(string? Note);

public sealed record RejectLoanRequest(string Reason);

public sealed record LoanResponse(
    Guid Id,
    string LoanCode,
    Guid CustomerId,
    decimal PrincipalAmount,
    decimal InterestRatePercent,
    decimal InterestAmount,
    decimal TotalRepayable,
    int TermCount,
    RepaymentFrequency RepaymentFrequency,
    InterestCalculationMethod InterestCalculationMethod,
    LoanStatus Status,
    DateOnly FirstRepaymentDate,
    DateOnly MaturityDate,
    Guid RequestedByUserId,
    Guid? ApprovedByUserId,
    DateTime? ApprovedAtUtc,
    string? ApprovalNote,
    string? RejectionReason,
    string? Purpose,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    IReadOnlyCollection<LoanRepaymentScheduleResponse> RepaymentSchedules);

public sealed record LoanRepaymentScheduleResponse(
    Guid Id,
    int InstallmentNumber,
    DateOnly DueDate,
    decimal PrincipalAmount,
    decimal InterestAmount,
    decimal TotalAmount,
    decimal AmountPaid,
    RepaymentScheduleStatus Status,
    DateTime? PaidAtUtc);

public sealed record LoanListResponse(
    IReadOnlyCollection<LoanResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
