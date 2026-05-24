using Circle.Application.Loans;
using Circle.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Circle.WebApi.Controllers;

[ApiController]
[Authorize(Roles = "Admin,CreditOfficer")]
[Route("api/loans")]
public sealed class LoansController(ILoanService loanService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLoans(
        [FromQuery] string? search,
        [FromQuery] LoanStatus? status,
        [FromQuery] Guid? customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await loanService.GetLoansAsync(search, status, customerId, page, pageSize, cancellationToken));
    }

    [HttpGet("{loanId:guid}")]
    public async Task<IActionResult> GetLoan(Guid loanId, CancellationToken cancellationToken)
    {
        return Ok(await loanService.GetLoanAsync(loanId, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> CreateLoan(CreateLoanRequest request, CancellationToken cancellationToken)
    {
        var response = await loanService.CreateLoanAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetLoan), new { loanId = response.Id }, response);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{loanId:guid}/approve")]
    public async Task<IActionResult> ApproveLoan(Guid loanId, ApproveLoanRequest request, CancellationToken cancellationToken)
    {
        return Ok(await loanService.ApproveLoanAsync(loanId, request, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{loanId:guid}/reject")]
    public async Task<IActionResult> RejectLoan(Guid loanId, RejectLoanRequest request, CancellationToken cancellationToken)
    {
        return Ok(await loanService.RejectLoanAsync(loanId, request, cancellationToken));
    }
}
