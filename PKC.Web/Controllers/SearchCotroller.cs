using Microsoft.AspNetCore.Mvc;
using PKC.Application.DTOs;
using PKC.Infrastructure.Services;

namespace PKC.Web.Controllers;

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
        var results = await _searchService.SearchAsync(dto.Query);

        return Ok(results);
    }
}