using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PKC.Domain.Entities;
using PKC.Infrastructure.Data;

namespace PKC.Infrastructure.Services;

public class ResourceProcessingService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ResourceProcessingService> _logger;
    private readonly ContentExtractor _extractor;
    private readonly ChunkingService _chunkingService;
    private readonly EmbeddingService _embeddingService;
    private readonly ConnectionService _connectionService;

    public ResourceProcessingService(
        AppDbContext context,
        ILogger<ResourceProcessingService> logger,
        ContentExtractor extractor,
        ChunkingService chunkingService,
        EmbeddingService embeddingService,
        ConnectionService connectionService)
    {
        _context = context;
        _logger = logger;
        _extractor = extractor;
        _chunkingService = chunkingService;
        _embeddingService = embeddingService;
        _connectionService = connectionService;
    }

    public async Task ProcessAsync(Guid resourceId)
    {
        var resource = await _context.Resources.FirstOrDefaultAsync(x => x.Id == resourceId);

        if (resource == null)
        {
            _logger.LogWarning("Resource not found: {ResourceId}", resourceId);
            return;
        }

        try
        {
            //------------------------------------------------------------------------------------
            // STATUS: EXTRACTING 
            //------------------------------------------------------------------------------------
            resource.Status = ResourceStatus.Extracting;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Extracting content for resource {ResourceId} (type: {Type})",
                resourceId,
                resource.Type);

            string? textToProcess = null;

            if (resource.Type == ResourceType.Url && !string.IsNullOrEmpty(resource.SourceUrl))
            {
                var extracted = await _extractor.ExtractFromUrlAsync(resource.SourceUrl);

                if (string.IsNullOrWhiteSpace(extracted))
                {
                    resource.Status = ResourceStatus.Failed;
                    resource.FailureReason = "Failed to extract content from URL";
                    await _context.SaveChangesAsync();
                    return;
                }

                resource.ExtractedText = extracted;
                textToProcess = extracted;
            }
            else if (resource.Type == ResourceType.Pdf && !string.IsNullOrEmpty(resource.FilePath))
            {
                if (!File.Exists(resource.FilePath))
                {
                    resource.Status = ResourceStatus.Failed;
                    resource.FailureReason = $"PDF file not found on disk: {resource.FilePath}";
                    await _context.SaveChangesAsync();

                    _logger.LogError(
                        "PDF file missing for resource {ResourceId}: {FilePath}",
                        resourceId,
                        resource.FilePath);

                    return;
                }

                var extracted = _extractor.ExtractFromPdf(resource.FilePath);

                if (string.IsNullOrWhiteSpace(extracted))
                {
                    resource.Status = ResourceStatus.Failed;
                    resource.FailureReason = "Failed to extract text from PDF — file may be scanned/image-only";
                    await _context.SaveChangesAsync();
                    return;
                }

                resource.ExtractedText = extracted;
                textToProcess = extracted;
            }
            else if (resource.Type == ResourceType.Note && !string.IsNullOrEmpty(resource.RawContent))
            {
                textToProcess = resource.RawContent;
            }

            if (string.IsNullOrWhiteSpace(textToProcess))
            {
                resource.Status = ResourceStatus.Failed;
                resource.FailureReason = "No content available to process";
                await _context.SaveChangesAsync();
                return;
            }

            resource.WordCount = textToProcess
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Length;

            //------------------------------------------------------------------------------------
            // STATUS: CHUNKING
            //------------------------------------------------------------------------------------
            resource.Status = ResourceStatus.Chunking;
            await _context.SaveChangesAsync();

            var chunks = _chunkingService.CreateChunks(resource.Id, resource.UserId, textToProcess);

            await _context.Chunks.AddRangeAsync(chunks);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created {Count} chunks for resource {ResourceId}", chunks.Count, resourceId);

            //------------------------------------------------------------------------------------
            // STATUS: EMBEDDING
            //------------------------------------------------------------------------------------
            resource.Status = ResourceStatus.Embedding;
            await _context.SaveChangesAsync();

            var resourceChunks = await _context.Chunks
                .Where(x => x.ResourceId == resource.Id)
                .ToListAsync();

            var semaphore = new SemaphoreSlim(3);

            var tasks = resourceChunks.Select(async chunk =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var vector = await _embeddingService.GenerateEmbeddingAsync(chunk.Content);
                    if (vector != null)
                    {
                        chunk.Embedding = vector;
                        _context.Entry(chunk).Property(x => x.Embedding).IsModified = true;
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(tasks);

            _logger.LogInformation(
                "Saving {Count} embeddings for resource {ResourceId}",
                resourceChunks.Count,
                resourceId);

            await _context.SaveChangesAsync();

            //------------------------------------------------------------------------------------
            // CONNECTION DISCOVERY
            //------------------------------------------------------------------------------------
            await _connectionService.CreateConnectionsAsync(resource.Id, resource.UserId);

            //------------------------------------------------------------------------------------
            // STATUS: READY
            //------------------------------------------------------------------------------------
            resource.Status = ResourceStatus.Ready;
            resource.ProcessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Resource {ResourceId} processing complete", resourceId);
        }
        catch (Exception ex)
        {
            resource.Status = ResourceStatus.Failed;
            resource.FailureReason = ex.Message;
            await _context.SaveChangesAsync();

            _logger.LogError(ex, "Processing failed for resource {ResourceId}", resourceId);
        }
    }
}