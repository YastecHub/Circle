using Circle.Application.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Circle.WebApi.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/users")]
public sealed class UsersController(IUserManagementService userManagementService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        return Ok(await userManagementService.GetUsersAsync(cancellationToken));
    }

    [HttpPost("credit-officers")]
    public async Task<IActionResult> InviteCreditOfficer(InviteCreditOfficerRequest request, CancellationToken cancellationToken)
    {
        var response = await userManagementService.InviteCreditOfficerAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetUsers), response);
    }

    [HttpPost("{userId:guid}/activate")]
    public async Task<IActionResult> ActivateUser(Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await userManagementService.ActivateUserAsync(userId, cancellationToken));
    }

    [HttpPost("{userId:guid}/suspend")]
    public async Task<IActionResult> SuspendUser(Guid userId, CancellationToken cancellationToken)
    {
        return Ok(await userManagementService.SuspendUserAsync(userId, cancellationToken));
    }

    [HttpPost("{userId:guid}/password")]
    public async Task<IActionResult> SetPassword(Guid userId, SetUserPasswordRequest request, CancellationToken cancellationToken)
    {
        return Ok(await userManagementService.SetPasswordAsync(userId, request, cancellationToken));
    }
}
