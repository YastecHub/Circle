using Circle.Domain.Enums;

namespace Circle.Application.Dashboard;

public sealed record DashboardOverviewResponse(
    DateOnly BusinessDate,
    DashboardPortfolioSnapshot Portfolio,
    DashboardCollectionSnapshot Collections,
    DashboardCashSnapshot Cash,
    IReadOnlyCollection<DashboardRecentRepaymentResponse> RecentRepayments,
    IReadOnlyCollection<DashboardRecentCollectionActivityResponse> RecentCollectionActivities);

public sealed record DashboardPortfolioSnapshot(
    int TotalCustomers,
    int ActiveCustomers,
    int ActiveLoans,
    int PendingApprovalLoans,
    decimal TotalOutstanding,
    decimal TotalOverdue);

public sealed record DashboardCollectionSnapshot(
    int DueTodayCount,
    decimal DueTodayAmount,
    int OverdueCount,
    decimal OverdueAmount,
    int PromiseToPayDueCount,
    decimal PromiseToPayDueAmount);

public sealed record DashboardCashSnapshot(
    decimal DisbursedToday,
    decimal CollectedToday,
    decimal NetCashToday);

public sealed record DashboardRecentRepaymentResponse(
    Guid Id,
    Guid LoanId,
    string LoanCode,
    Guid CustomerId,
    string CustomerName,
    decimal Amount,
    DateOnly PaymentDate,
    RepaymentMethod Method,
    string? Reference,
    DateTime CreatedAtUtc);

public sealed record DashboardRecentCollectionActivityResponse(
    Guid Id,
    Guid CustomerId,
    string CustomerName,
    Guid? LoanId,
    string? LoanCode,
    CollectionActivityType ActivityType,
    CollectionOutcome Outcome,
    DateOnly? PromisedPaymentDate,
    decimal? PromisedAmount,
    string Notes,
    DateTime CreatedAtUtc);
