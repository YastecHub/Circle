using Circle.Application.Disbursements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Circle.WebApi.Controllers;

[ApiController]
[Authorize(Roles = "Admin,CreditOfficer")]
[Route("api/disbursements")]
public sealed class DisbursementsController(IDisbursementService disbursementService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetDisbursements(
        [FromQuery] Guid? loanId,
        [FromQuery] Guid? customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await disbursementService.GetDisbursementsAsync(loanId, customerId, page, pageSize, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> RecordDisbursement(RecordDisbursementRequest request, CancellationToken cancellationToken)
    {
        return Ok(await disbursementService.RecordDisbursementAsync(request, cancellationToken));
    }
}
