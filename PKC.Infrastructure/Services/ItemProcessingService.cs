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

    public ItemProcessingService(
        AppDbContext context,
        ILogger<ItemProcessingService> logger,
        ContentExtractor extractor)
    {
        _context = context;
        _logger = logger;
        _extractor = extractor;
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

            item.Status = ItemStatus.Extracting;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Extracting content for item {ItemId}", itemId);

            if (item.Type == ItemType.Url && !string.IsNullOrEmpty(item.SourceUrl))
            {
                var extracted = await _extractor.ExtractFromUrlAsync(item.SourceUrl);

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