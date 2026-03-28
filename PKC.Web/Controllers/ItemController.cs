using Microsoft.AspNetCore.Mvc;
using PKC.Application.DTOs;
using PKC.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using PKC.Web.Extensions;

namespace PKC.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/items")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _service;

    public ItemsController(IItemService service)
    {
        _service = service;
    }

    [HttpPost("url")]
    public async Task<IActionResult> CreateFromUrl(CreateItemDto dto)
    {
        try
        {
            // The logic: middleware has already validated the token by now
            var userId = HttpContext.GetUserId();

            var itemId = await _service.CreateFromUrlAsync(userId, dto);

            return Accepted(new { itemId });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
    }

    [HttpPost("note")]
    public async Task<IActionResult> CreateNote(CreateItemDto dto)
    {
        try
        {
            var userId = HttpContext.GetUserId();
            var id = await _service.CreateNoteAsync(userId, dto);
            return Accepted(new { itemId = id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}