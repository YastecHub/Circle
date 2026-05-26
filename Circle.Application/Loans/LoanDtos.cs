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

public sealed record LoanStatementResponse(
    Guid LoanId,
    string LoanCode,
    Guid CustomerId,
    string CustomerCode,
    string CustomerName,
    LoanStatus Status,
    decimal PrincipalAmount,
    decimal InterestAmount,
    decimal TotalRepayable,
    decimal TotalDisbursed,
    decimal TotalPaid,
    decimal OutstandingBalance,
    decimal OverdueBalance,
    DateOnly? NextDueDate,
    decimal? NextDueAmount,
    DateOnly? LastPaymentDate,
    IReadOnlyCollection<LoanStatementScheduleLine> RepaymentSchedules,
    IReadOnlyCollection<LoanStatementRepaymentLine> Repayments,
    IReadOnlyCollection<LoanStatementDisbursementLine> Disbursements);

public sealed record LoanStatementScheduleLine(
    Guid Id,
    int InstallmentNumber,
    DateOnly DueDate,
    decimal PrincipalAmount,
    decimal InterestAmount,
    decimal TotalAmount,
    decimal AmountPaid,
    decimal OutstandingAmount,
    RepaymentScheduleStatus Status,
    bool IsOverdue,
    DateTime? PaidAtUtc);

public sealed record LoanStatementRepaymentLine(
    Guid Id,
    decimal Amount,
    DateOnly PaymentDate,
    RepaymentMethod Method,
    string? Reference,
    Guid? RecordedByUserId,
    string? Notes,
    DateTime CreatedAtUtc);

public sealed record LoanStatementDisbursementLine(
    Guid Id,
    decimal Amount,
    DateOnly DisbursementDate,
    DisbursementMethod Method,
    string? Reference,
    Guid DisbursedByUserId,
    string? Notes,
    DateTime CreatedAtUtc);

public sealed record PendingApprovalLoanResponse(
    Guid Id,
    string LoanCode,
    Guid CustomerId,
    string CustomerCode,
    string CustomerName,
    string CustomerPhone,
    Guid RequestedByUserId,
    string RequestedByName,
    decimal PrincipalAmount,
    decimal InterestRatePercent,
    decimal InterestAmount,
    decimal TotalRepayable,
    int TermCount,
    RepaymentFrequency RepaymentFrequency,
    DateOnly FirstRepaymentDate,
    DateOnly MaturityDate,
    string? Purpose,
    decimal AutoApprovalThreshold,
    decimal AmountAboveThreshold,
    int DaysPending,
    DateTime CreatedAtUtc);

public sealed record PendingApprovalLoanListResponse(
    IReadOnlyCollection<PendingApprovalLoanResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);

public sealed record LoanListResponse(
    IReadOnlyCollection<LoanResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
