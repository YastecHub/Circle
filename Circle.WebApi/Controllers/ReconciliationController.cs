using Circle.Application.Reconciliation;
using Circle.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Circle.WebApi.Controllers;

[ApiController]
[Authorize(Roles = "Admin,CreditOfficer")]
[Route("api/reconciliation")]
public sealed class ReconciliationController(IReconciliationService reconciliationService) : ControllerBase
{
    [HttpGet("inbound-payments")]
    public async Task<IActionResult> GetInboundPayments(
        [FromQuery] string? search,
        [FromQuery] InboundPaymentStatus? status,
        [FromQuery] Guid? customerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await reconciliationService.GetInboundPaymentsAsync(search, status, customerId, page, pageSize, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("inbound-payments")]
    public async Task<IActionResult> RecordInboundPayment(RecordInboundPaymentRequest request, CancellationToken cancellationToken)
    {
        return Ok(await reconciliationService.RecordInboundPaymentAsync(request, cancellationToken));
    }
}
