using Circle.Application.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Circle.WebApi.Controllers;

[ApiController]
[Authorize(Roles = "AppAdmin,Admin,Developer,CreditOfficer")]
[Route("api/search")]
public sealed class SearchController(IGlobalSearchService searchService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string query,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        return Ok(await searchService.SearchAsync(query, limit, cancellationToken));
    }
}


