using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PKC.Domain.Entities;
using PKC.Infrastructure.Data;

namespace PKC.Infrastructure.Services;
public class ItemProcessingService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ItemProcessingService> _logger;

    public ItemProcessingService(AppDbContext context, ILogger<ItemProcessingService> logger)
    {
        _context = context;
        _logger = logger;
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
            // STEP 1 — Extracting
            item.Status = ItemStatus.Extracting;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Processing item {ItemId}", itemId);

            // TEMP: simulate extraction
            await Task.Delay(2000);

            item.ExtractedText = "Sample extracted content";
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