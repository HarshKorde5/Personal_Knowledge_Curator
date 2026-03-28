using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PKC.Application.DTOs;
using PKC.Infrastructure.Services;
using PKC.Web.Extensions;

namespace PKC.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    private readonly SearchService _searchService;

    public SearchController(SearchService searchService)
    {
        _searchService = searchService;
    }

    [HttpPost]
    public async Task<IActionResult> Search(SearchRequestDto dto)
    {
        try
        {
            var userId = HttpContext.GetUserId();
            var results = await _searchService.SearchAsync(dto.Query, userId);
            return Ok(results);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }
}