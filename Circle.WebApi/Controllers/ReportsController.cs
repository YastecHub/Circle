using Circle.Application.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Circle.WebApi.Controllers;

[ApiController]
[Authorize(Roles = "AppAdmin,Admin,Developer,CreditOfficer")]
[Route("api/reports")]
public sealed class ReportsController(IReportService reportService) : ControllerBase
{
    [HttpGet("portfolio-summary")]
    public async Task<IActionResult> GetPortfolioSummary(CancellationToken cancellationToken)
    {
        return Ok(await reportService.GetPortfolioSummaryAsync(cancellationToken));
    }

    [HttpGet("cashflow")]
    public async Task<IActionResult> GetCashflowSummary(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CancellationToken cancellationToken)
    {
        return Ok(await reportService.GetCashflowSummaryAsync(from, to, cancellationToken));
    }

    [HttpGet("overdue-installments")]
    public async Task<IActionResult> GetOverdueInstallments(CancellationToken cancellationToken)
    {
        return Ok(await reportService.GetOverdueInstallmentsAsync(cancellationToken));
    }

    [HttpGet("arrears-aging")]
    public async Task<IActionResult> GetArrearsAging(CancellationToken cancellationToken)
    {
        return Ok(await reportService.GetArrearsAgingAsync(cancellationToken));
    }

    [HttpGet("officer-performance")]
    public async Task<IActionResult> GetOfficerPerformance(CancellationToken cancellationToken)
    {
        return Ok(await reportService.GetOfficerPerformanceAsync(cancellationToken));
    }
}


