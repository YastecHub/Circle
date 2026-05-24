using Circle.Application.VirtualAccounts;
using Circle.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Circle.WebApi.Controllers;

[ApiController]
[Authorize(Roles = "Admin,CreditOfficer")]
public sealed class VirtualAccountsController(IVirtualAccountService virtualAccountService) : ControllerBase
{
    [HttpGet("api/virtual-accounts")]
    public async Task<IActionResult> GetVirtualAccounts(
        [FromQuery] string? search,
        [FromQuery] Guid? customerId,
        [FromQuery] VirtualAccountStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await virtualAccountService.GetVirtualAccountsAsync(search, customerId, status, page, pageSize, cancellationToken));
    }

    [HttpGet("api/customers/{customerId:guid}/virtual-account")]
    public async Task<IActionResult> GetCustomerVirtualAccount(Guid customerId, CancellationToken cancellationToken)
    {
        return Ok(await virtualAccountService.GetCustomerVirtualAccountAsync(customerId, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("api/customers/{customerId:guid}/virtual-account")]
    public async Task<IActionResult> RegisterCustomerVirtualAccount(Guid customerId, RegisterVirtualAccountRequest request, CancellationToken cancellationToken)
    {
        return Ok(await virtualAccountService.RegisterCustomerVirtualAccountAsync(customerId, request, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("api/customers/{customerId:guid}/virtual-account/deactivate")]
    public async Task<IActionResult> DeactivateCustomerVirtualAccount(Guid customerId, CancellationToken cancellationToken)
    {
        return Ok(await virtualAccountService.DeactivateCustomerVirtualAccountAsync(customerId, cancellationToken));
    }
}
