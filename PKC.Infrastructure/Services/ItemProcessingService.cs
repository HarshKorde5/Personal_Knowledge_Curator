using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PKC.Domain.Entities;
using PKC.Infrastructure.Data;

namespace PKC.Infrastructure.Services;

public class ItemProcessingService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ItemProcessingService> _logger;
    private readonly ContentExtractor _extractor;
    private readonly ChunkingService _chunkingService;
    private readonly EmbeddingService _embeddingService;
    private readonly ConnectionService _connectionService;

    public ItemProcessingService(
        AppDbContext context,
        ILogger<ItemProcessingService> logger,
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

    public async Task ProcessAsync(Guid itemId)
    {
        var item = await _context.Items.FirstOrDefaultAsync(x => x.Id == itemId);

        if (item == null)
        {
            _logger.LogWarning("Item not found: {ItemId}", itemId);
            return;
        }

        try
        {
            //------------------------------------------------------------------------------------
            // STATUS: EXTRACTING 
            //------------------------------------------------------------------------------------
            item.Status = ItemStatus.Extracting;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Extracting content for item {ItemId} (type: {Type})",
                itemId,
                item.Type);

            string? textToProcess = null;

            if (item.Type == ItemType.Url && !string.IsNullOrEmpty(item.SourceUrl))
            {
                var extracted = await _extractor.ExtractFromUrlAsync(item.SourceUrl);

                if (string.IsNullOrWhiteSpace(extracted))
                {
                    item.Status = ItemStatus.Failed;
                    item.FailureReason = "Failed to extract content from URL";
                    await _context.SaveChangesAsync();
                    return;
                }

                item.ExtractedText = extracted;
                textToProcess = extracted;
            }
            else if (item.Type == ItemType.Pdf && !string.IsNullOrEmpty(item.FilePath))
            {
                if (!File.Exists(item.FilePath))
                {
                    item.Status = ItemStatus.Failed;
                    item.FailureReason = $"PDF file not found on disk: {item.FilePath}";
                    await _context.SaveChangesAsync();

                    _logger.LogError(
                        "PDF file missing for item {ItemId}: {FilePath}",
                        itemId,
                        item.FilePath);

                    return;
                }

                var extracted = _extractor.ExtractFromPdf(item.FilePath);

                if (string.IsNullOrWhiteSpace(extracted))
                {
                    item.Status = ItemStatus.Failed;
                    item.FailureReason = "Failed to extract text from PDF — file may be scanned/image-only";
                    await _context.SaveChangesAsync();
                    return;
                }

                item.ExtractedText = extracted;
                textToProcess = extracted;
            }
            else if (item.Type == ItemType.Note && !string.IsNullOrEmpty(item.RawContent))
            {
                textToProcess = item.RawContent;
            }

            if (string.IsNullOrWhiteSpace(textToProcess))
            {
                item.Status = ItemStatus.Failed;
                item.FailureReason = "No content available to process";
                await _context.SaveChangesAsync();
                return;
            }

            item.WordCount = textToProcess
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Length;

            //------------------------------------------------------------------------------------
            // STATUS: CHUNKING
            //------------------------------------------------------------------------------------
            item.Status = ItemStatus.Chunking;
            await _context.SaveChangesAsync();

            var chunks = _chunkingService.CreateChunks(item.Id, item.UserId, textToProcess);

            await _context.Chunks.AddRangeAsync(chunks);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created {Count} chunks for item {ItemId}", chunks.Count, itemId);

            //------------------------------------------------------------------------------------
            // STATUS: EMBEDDING
            //------------------------------------------------------------------------------------
            item.Status = ItemStatus.Embedding;
            await _context.SaveChangesAsync();

            var itemChunks = await _context.Chunks
                .Where(x => x.ItemId == item.Id)
                .ToListAsync();

            var semaphore = new SemaphoreSlim(3);

            var tasks = itemChunks.Select(async chunk =>
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
                "Saving {Count} embeddings for item {ItemId}",
                itemChunks.Count,
                itemId);

            await _context.SaveChangesAsync();

            //------------------------------------------------------------------------------------
            // CONNECTION DISCOVERY
            //------------------------------------------------------------------------------------
            await _connectionService.CreateConnectionsAsync(item.Id, item.UserId);

            //------------------------------------------------------------------------------------
            // STATUS: READY
            //------------------------------------------------------------------------------------
            item.Status = ItemStatus.Ready;
            item.ProcessedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Item {ItemId} processing complete", itemId);
        }
        catch (Exception ex)
        {
            item.Status = ItemStatus.Failed;
            item.FailureReason = ex.Message;
            await _context.SaveChangesAsync();

            _logger.LogError(ex, "Processing failed for item {ItemId}", itemId);
        }
    }
}