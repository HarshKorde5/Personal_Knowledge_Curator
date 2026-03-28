using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PKC.Application.DTOs;
using PKC.Infrastructure.Services;
using PKC.Web.Extensions;

namespace PKC.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly RagService _ragService;

    public AiController(RagService ragService)
    {
        _ragService = ragService;
    }

    [HttpPost("ask")]
    public async Task<IActionResult> Ask(SearchRequestDto dto)
    {
        try
        {
            var userId = HttpContext.GetUserId();
            var result = await _ragService.AskAsync(dto.Query, userId);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }
}