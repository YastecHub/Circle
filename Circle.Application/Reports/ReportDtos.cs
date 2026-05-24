using Circle.Domain.Enums;

namespace Circle.Application.Reports;

public sealed record PortfolioSummaryResponse(
    int TotalCustomers,
    int ActiveCustomers,
    int TotalLoans,
    int PendingApprovalLoans,
    int ApprovedLoans,
    int DisbursedLoans,
    int ClosedLoans,
    decimal TotalPrincipal,
    decimal TotalDisbursed,
    decimal TotalRepaid,
    decimal TotalOutstanding,
    int OverdueInstallments,
    decimal OverdueAmount);

public sealed record CashflowSummaryResponse(
    DateOnly From,
    DateOnly To,
    decimal TotalDisbursed,
    decimal TotalRepaid,
    decimal NetCashflow,
    IReadOnlyCollection<CashflowDayResponse> Days);

public sealed record CashflowDayResponse(
    DateOnly Date,
    decimal Disbursed,
    decimal Repaid,
    decimal Net);

public sealed record OverdueInstallmentResponse(
    Guid LoanId,
    string LoanCode,
    Guid CustomerId,
    string CustomerName,
    string CustomerPhone,
    int InstallmentNumber,
    DateOnly DueDate,
    int DaysOverdue,
    decimal TotalAmount,
    decimal AmountPaid,
    decimal OutstandingAmount,
    RepaymentScheduleStatus Status);

public sealed record OfficerPerformanceResponse(
    Guid OfficerId,
    string OfficerName,
    int AssignedCustomers,
    int ActiveLoans,
    decimal TotalDisbursed,
    decimal TotalRepaid,
    decimal OutstandingAmount,
    decimal OverdueAmount);
