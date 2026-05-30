using Circle.Application.Administration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Circle.WebApi.Controllers;

[ApiController]
[Authorize(Roles = "AppAdmin,Admin,Developer")]
[Route("api/admin")]
public sealed class AdminController(IAdminService adminService) : ControllerBase
{
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
    {
        return Ok(await adminService.GetSettingsAsync(cancellationToken));
    }

    [HttpPut("settings/{key}")]
    public async Task<IActionResult> UpdateSetting(string key, UpdateSystemSettingRequest request, CancellationToken cancellationToken)
    {
        return Ok(await adminService.UpdateSettingAsync(key, request, cancellationToken));
    }

    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? action,
        [FromQuery] string? affectedRecord,
        [FromQuery] Guid? actorUserId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await adminService.GetAuditLogsAsync(action, affectedRecord, actorUserId, fromUtc, toUtc, page, pageSize, cancellationToken));
    }
}


