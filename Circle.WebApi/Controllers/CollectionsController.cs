using Circle.Application.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Circle.WebApi.Controllers;

[ApiController]
[Authorize(Roles = "AppAdmin,Admin,Developer,CreditOfficer")]
[Route("api/collections")]
public sealed class CollectionsController(ICollectionService collectionService) : ControllerBase
{
    [HttpGet("activities")]
    public async Task<IActionResult> GetActivities(
        [FromQuery] Guid? customerId,
        [FromQuery] Guid? loanId,
        [FromQuery] Guid? officerUserId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await collectionService.GetActivitiesAsync(customerId, loanId, officerUserId, fromUtc, toUtc, page, pageSize, cancellationToken));
    }

    [HttpGet("worklist")]
    public async Task<IActionResult> GetWorklist(
        [FromQuery] DateOnly? workDate,
        [FromQuery] Guid? officerUserId,
        CancellationToken cancellationToken = default)
    {
        return Ok(await collectionService.GetWorklistAsync(workDate, officerUserId, cancellationToken));
    }

    [HttpPost("activities")]
    public async Task<IActionResult> RecordActivity(RecordCollectionActivityRequest request, CancellationToken cancellationToken)
    {
        return Ok(await collectionService.RecordActivityAsync(request, cancellationToken));
    }

    [HttpPost("refresh-overdue")]
    public async Task<IActionResult> RefreshOverdueSchedules(CancellationToken cancellationToken)
    {
        return Ok(await collectionService.RefreshOverdueSchedulesAsync(cancellationToken));
    }
}


