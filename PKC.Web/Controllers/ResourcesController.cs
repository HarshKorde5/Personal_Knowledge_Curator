using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PKC.Application.DTOs;
using PKC.Application.Interfaces;
using PKC.Web.Extensions;

namespace PKC.Web.Controllers;

[Authorize]
[ApiController]
[Route("api/resources")]
public class ResourcesController : ControllerBase
{
    private readonly IResourceService _service;
    private readonly IConfiguration _config;
    private readonly ILogger<ResourcesController> _logger;
    private const long MaxPdfSizeBytes = 50 * 1024 * 1024;

    public ResourcesController(
        IResourceService service,
        IConfiguration config,
        ILogger<ResourcesController> logger)
    {
        _service = service;
        _config = config;
        _logger = logger;
    }


    // -------------------------------------------------------------------------
    // GET api/resources
    // -------------------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var userId = HttpContext.GetUserId();
            var resources = await _service.GetUserResourcesAsync(userId);

            // Map to DTOs if necessary, or return the entities
            return Ok(resources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve resources");
            return StatusCode(500, new { message = "Error loading resources." });
        }
    }

    // -------------------------------------------------------------------------
    // POST api/resources/url
    // -------------------------------------------------------------------------
    [HttpPost("url")]
    public async Task<IActionResult> CreateFromUrl(CreateResourceDto dto)
    {
        try
        {
            var userId = HttpContext.GetUserId();
            var resourceId = await _service.CreateFromUrlAsync(userId, dto);
            return Accepted(new { resourceId });
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
    // POST api/resources/note
    // -------------------------------------------------------------------------
    [HttpPost("note")]
    public async Task<IActionResult> CreateNote(CreateResourceDto dto)
    {
        try
        {
            var userId = HttpContext.GetUserId();
            var resourceId = await _service.CreateNoteAsync(userId, dto);
            return Accepted(new { resourceId });
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
    // POST api/resources/pdf
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

            var resourceId = await _service.CreateFromPdfAsync(userId, savedFilePath, title);

            return Accepted(new { resourceId });
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