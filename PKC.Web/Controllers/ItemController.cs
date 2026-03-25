using Microsoft.AspNetCore.Mvc;
using PKC.Application.DTOs;
using PKC.Application.Interfaces;

namespace PKC.Web.Controllers;

[ApiController]
[Route("api/items")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _service;

    public ItemsController(IItemService service)
    {
        _service = service;
    }

    // TEMP (will replace with JWT later)
    private Guid GetUserId() => Guid.Parse("11111111-1111-1111-1111-111111111111");

    [HttpPost("url")]
    public async Task<IActionResult> CreateUrl(CreateItemDto dto)
    {
        var id = await _service.CreateUrlAsync(GetUserId(), dto);
        return Accepted(new { itemId = id });
    }

    [HttpPost("note")]
    public async Task<IActionResult> CreateNote(CreateItemDto dto)
    {
        var id = await _service.CreateNoteAsync(GetUserId(), dto);
        return Accepted(new { itemId = id });
    }
}