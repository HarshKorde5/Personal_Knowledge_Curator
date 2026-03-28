using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PKC.Application.DTOs;
using PKC.Application.Interfaces;
using PKC.Web.Extensions;

namespace PKC.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/items")]
public class ItemsController : ControllerBase
{
    private readonly IItemService _service;
    private readonly IConfiguration _config;
    private readonly ILogger<ItemsController> _logger;
    private const long MaxPdfSizeBytes = 50 * 1024 * 1024;

    public ItemsController(
        IItemService service,
        IConfiguration config,
        ILogger<ItemsController> logger)
    {
        _service = service;
        _config = config;
        _logger = logger;
    }

    // -------------------------------------------------------------------------
    // POST api/items/url
    // -------------------------------------------------------------------------
    [HttpPost("url")]
    public async Task<IActionResult> CreateFromUrl(CreateItemDto dto)
    {
        try
        {
            var userId = HttpContext.GetUserId();
            var itemId = await _service.CreateFromUrlAsync(userId, dto);
            return Accepted(new { itemId });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // POST api/items/note
    // -------------------------------------------------------------------------
    [HttpPost("note")]
    public async Task<IActionResult> CreateNote(CreateItemDto dto)
    {
        try
        {
            var userId = HttpContext.GetUserId();
            var itemId = await _service.CreateNoteAsync(userId, dto);
            return Accepted(new { itemId });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // -------------------------------------------------------------------------
    // POST api/items/pdf
    // -------------------------------------------------------------------------
    [HttpPost("pdf")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 50 * 1024 * 1024)]
    public async Task<IActionResult> UploadPdf(
        [FromForm] IFormFile file,
        [FromForm] string? title)
    {
        try
        {
            var userId = HttpContext.GetUserId();

            if (file == null || file.Length == 0)
                return BadRequest(new { message = "No file provided." });

            var isPdfMime = file.ContentType.Equals(
                "application/pdf",
                StringComparison.OrdinalIgnoreCase);

            var isPdfExtension = Path.GetExtension(file.FileName)
                .Equals(".pdf", StringComparison.OrdinalIgnoreCase);

            if (!isPdfMime && !isPdfExtension)
                return BadRequest(new { message = "Only PDF files are accepted." });

            if (file.Length > MaxPdfSizeBytes)
                return BadRequest(new { message = "File size exceeds the 50 MB limit." });


            var uploadRoot = _config["FileStorage:UploadPath"] ?? "uploads";

            if (!Path.IsPathRooted(uploadRoot))
                uploadRoot = Path.Combine(Directory.GetCurrentDirectory(), uploadRoot);

            var userDir = Path.Combine(uploadRoot, userId.ToString());
            Directory.CreateDirectory(userDir);

            var savedFileName = $"{Guid.NewGuid()}.pdf";
            var savedFilePath = Path.Combine(userDir, savedFileName);

            await using (var stream = new FileStream(
                savedFilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                useAsync: true))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation(
                "PDF saved for user {UserId}: {FilePath} ({Bytes} bytes)",
                userId,
                savedFilePath,
                file.Length);

            var itemId = await _service.CreateFromPdfAsync(userId, savedFilePath, title);

            return Accepted(new { itemId });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF upload failed");
            return StatusCode(500, new { message = "An error occurred while uploading the PDF." });
        }
    }
}