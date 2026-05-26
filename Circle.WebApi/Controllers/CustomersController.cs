using Circle.Application.Customers;
using Circle.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Circle.WebApi.Controllers;

[ApiController]
[Authorize(Roles = "Admin,CreditOfficer")]
[Route("api/customers")]
public sealed class CustomersController(ICustomerService customerService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] string? search,
        [FromQuery] CustomerStatus? status,
        [FromQuery] Guid? assignedCreditOfficerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await customerService.GetCustomersAsync(search, status, assignedCreditOfficerId, page, pageSize, cancellationToken));
    }

    [HttpGet("{customerId:guid}")]
    public async Task<IActionResult> GetCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        return Ok(await customerService.GetCustomerAsync(customerId, cancellationToken));
    }

    [HttpGet("{customerId:guid}/statement")]
    public async Task<IActionResult> GetCustomerStatement(Guid customerId, CancellationToken cancellationToken)
    {
        return Ok(await customerService.GetCustomerStatementAsync(customerId, cancellationToken));
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer(CreateCustomerRequest request, CancellationToken cancellationToken)
    {
        var response = await customerService.CreateCustomerAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCustomer), new { customerId = response.Id }, response);
    }

    [HttpPut("{customerId:guid}")]
    public async Task<IActionResult> UpdateCustomer(Guid customerId, UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        return Ok(await customerService.UpdateCustomerAsync(customerId, request, cancellationToken));
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("{customerId:guid}/reassign")]
    public async Task<IActionResult> ReassignCustomer(Guid customerId, ReassignCustomerRequest request, CancellationToken cancellationToken)
    {
        return Ok(await customerService.ReassignCustomerAsync(customerId, request, cancellationToken));
    }

    [HttpPost("{customerId:guid}/archive")]
    public async Task<IActionResult> ArchiveCustomer(Guid customerId, CancellationToken cancellationToken)
    {
        return Ok(await customerService.ArchiveCustomerAsync(customerId, cancellationToken));
    }
}
