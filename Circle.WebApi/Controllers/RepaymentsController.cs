using Circle.Application.Repayments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Circle.WebApi.Controllers;

[ApiController]
[Authorize(Roles = "AppAdmin,Admin,Developer,CreditOfficer")]
[Route("api/repayments")]
public sealed class RepaymentsController(IRepaymentService repaymentService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRepayments(
        [FromQuery] Guid? loanId,
        [FromQuery] Guid? customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await repaymentService.GetRepaymentsAsync(loanId, customerId, page, pageSize, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> RecordRepayment(RecordRepaymentRequest request, CancellationToken cancellationToken)
    {
        return Ok(await repaymentService.RecordRepaymentAsync(request, cancellationToken));
    }
}


