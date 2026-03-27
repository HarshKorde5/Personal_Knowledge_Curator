using Microsoft.AspNetCore.Mvc;
using PKC.Application.DTOs;
using PKC.Infrastructure.Services;

namespace PKC.Web.Controllers;

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
        var result = await _ragService.AskAsync(dto.Query);

        return Ok(result);
    }
}