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
            // STATUS : EXTRACTING
            //------------------------------------------------------------------------------------
            item.Status = ItemStatus.Extracting;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Extracting content for item {ItemId}", itemId);

            if (item.Type == ItemType.Url && !string.IsNullOrEmpty(item.SourceUrl))
            {
                var extracted = await _extractor.ExtractFromUrlAsync(item.SourceUrl);


                //------------------------------------------------------------------------------------
                // STATUS : FAILED
                //------------------------------------------------------------------------------------
                if (string.IsNullOrWhiteSpace(extracted))
                {
                    item.Status = ItemStatus.Failed;
                    item.FailureReason = "Failed to extract content";
                    await _context.SaveChangesAsync();
                    return;
                }

                item.ExtractedText = extracted;

                item.WordCount = extracted
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Length;


                //------------------------------------------------------------------------------------
                // STATUS : CHUNKING
                //------------------------------------------------------------------------------------
                item.Status = ItemStatus.Chunking;
                await _context.SaveChangesAsync();

                var chunks = _chunkingService.CreateChunks(item.Id, item.ExtractedText!);

                await _context.Chunks.AddRangeAsync(chunks);
                await _context.SaveChangesAsync();


                //------------------------------------------------------------------------------------
                // STATUS : EMBEDDING
                //------------------------------------------------------------------------------------
                item.Status = ItemStatus.Embedding;
                await _context.SaveChangesAsync();

                var itemChunks = await _context.Chunks
                    .Where(x => x.ItemId == item.Id)
                    .Take(20)
                    .ToListAsync();

                // var tasks = itemChunks.Select(async chunk =>
                // {
                //     chunk.Embedding = await _embeddingService.GenerateEmbeddingAsync(chunk.Content);
                // });

                var semaphore = new SemaphoreSlim(5);

                var tasks = itemChunks.Select(async chunk =>
                {
                    await semaphore.WaitAsync();

                    try
                    {
                        chunk.Embedding = await _embeddingService.GenerateEmbeddingAsync(chunk.Content);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                await Task.WhenAll(tasks);
                await _context.SaveChangesAsync();


                //------------------------------------------------------------------------------------
                // CONNECTION DISCOVERY
                //------------------------------------------------------------------------------------
                await _connectionService.CreateConnectionsAsync(item.Id);
            }

            item.Status = ItemStatus.Ready;
            item.ProcessedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            item.Status = ItemStatus.Failed;
            item.FailureReason = ex.Message;

            await _context.SaveChangesAsync();

            _logger.LogError(ex, "Processing failed for {ItemId}", itemId);
        }
    }
}