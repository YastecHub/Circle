namespace Circle.Application.Reports;

public interface IReportService
{
    Task<PortfolioSummaryResponse> GetPortfolioSummaryAsync(CancellationToken cancellationToken = default);

    Task<CashflowSummaryResponse> GetCashflowSummaryAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OverdueInstallmentResponse>> GetOverdueInstallmentsAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<OfficerPerformanceResponse>> GetOfficerPerformanceAsync(CancellationToken cancellationToken = default);
}
