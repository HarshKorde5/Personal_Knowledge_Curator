namespace PKC.Infrastructure.Repositories;

using Microsoft.EntityFrameworkCore;
using PKC.Application.DTOs;
using PKC.Application.Interfaces;
using PKC.Domain.Entities;
using PKC.Infrastructure.Data;

public class ItemRepository : IItemRepository
{
    private readonly AppDbContext _context;

    public ItemRepository(AppDbContext context)
    {
        _context = context;
    }

    // -----------------------------------------------------------------------
    // Write (existing - unchanged)
    // -----------------------------------------------------------------------

    public async Task AddAsync(Item item)
    {
        await _context.Items.AddAsync(item);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    // -----------------------------------------------------------------------
    // Read - GetAllByUserAsync
    // -----------------------------------------------------------------------

    public async Task<List<ItemListDto>> GetAllByUserAsync(Guid userId)
    {
        return await _context.Items
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .Select(i => new ItemListDto
            {
                Id          = i.Id,
                Title       = i.Title,
                Type        = i.Type.ToString(),
                Status      = i.Status.ToString(),
                SourceUrl   = i.SourceUrl,
                WordCount   = i.WordCount,
                CreatedAt   = i.CreatedAt,
                ProcessedAt = i.ProcessedAt,
                Tags        = new List<string>() 
            })
            .ToListAsync();
    }

    // -----------------------------------------------------------------------
    // Read - GetByIdAsync
    // -----------------------------------------------------------------------

    public async Task<ItemDetailDto?> GetByIdAsync(Guid itemId, Guid userId)
    {
        var item = await _context.Items
            .Where(i => i.Id == itemId && i.UserId == userId)
            .Select(i => new
            {
                i.Id,
                i.Title,
                i.Type,
                i.Status,
                i.SourceUrl,
                i.WordCount,
                i.CreatedAt,
                i.ProcessedAt,
                i.FailureReason,
                ContentPreview = i.ExtractedText != null
                    ? i.ExtractedText.Substring(0, Math.Min(600, i.ExtractedText.Length))
                    : i.RawContent != null
                        ? i.RawContent.Substring(0, Math.Min(600, i.RawContent.Length))
                        : null
            })
            .FirstOrDefaultAsync();

        if (item == null)
            return null;

        // Step 2: load connected items.

        var sourceChunkIds = await _context.Chunks
            .Where(c => c.ItemId == itemId && c.UserId == userId)
            .Select(c => c.Id)
            .ToListAsync();

        List<ConnectedItemDto> connectedItems = new();

        if (sourceChunkIds.Count > 0)
        {
            var rawConnections = await (
                from conn in _context.Connections
                where sourceChunkIds.Contains(conn.SourceChunkId)
                   && conn.UserId == userId

                join targetChunk in _context.Chunks
                    on conn.TargetChunkId equals targetChunk.Id

                where targetChunk.ItemId != itemId

                join targetItem in _context.Items
                    on targetChunk.ItemId equals targetItem.Id

                where targetItem.UserId == userId
                   && targetItem.Status == ItemStatus.Ready

                select new
                {
                    targetItem.Id,
                    targetItem.Title,
                    TargetType = targetItem.Type.ToString(),
                    conn.Score
                }
            ).ToListAsync();

            connectedItems = rawConnections
                .GroupBy(x => x.Id)
                .Select(g => new ConnectedItemDto
                {
                    ItemId   = g.Key,
                    Title    = g.First().Title,
                    ItemType = g.First().TargetType,
                    Strength = Math.Round(1.0 - g.Min(x => x.Score), 4)
                })
                .Where(x => x.Strength > 0)        
                .OrderByDescending(x => x.Strength)
                .Take(10)
                .ToList();
        }

        return new ItemDetailDto
        {
            Id             = item.Id,
            Title          = item.Title,
            Type           = item.Type.ToString(),
            Status         = item.Status.ToString(),
            SourceUrl      = item.SourceUrl,
            WordCount      = item.WordCount,
            CreatedAt      = item.CreatedAt,
            ProcessedAt    = item.ProcessedAt,
            FailureReason  = item.FailureReason,
            ContentPreview = item.ContentPreview,
            Tags           = new List<string>(),
            ConnectedItems = connectedItems
        };
    }

    // -----------------------------------------------------------------------
    // Read - GetStatusAsync  (lightweight poll)
    // -----------------------------------------------------------------------

    public async Task<ItemStatusDto?> GetStatusAsync(Guid itemId, Guid userId)
    {
        return await _context.Items
            .Where(i => i.Id == itemId && i.UserId == userId)
            .Select(i => new ItemStatusDto
            {
                Status        = i.Status.ToString(),
                FailureReason = i.FailureReason
            })
            .FirstOrDefaultAsync();
    }

    // -----------------------------------------------------------------------
    // Delete - DeleteAsync
    // -----------------------------------------------------------------------

    public async Task<bool> DeleteAsync(Guid itemId, Guid userId)
    {
        var item = await _context.Items
            .FirstOrDefaultAsync(i => i.Id == itemId && i.UserId == userId);

        if (item == null)
            return false;

        var chunkIds = await _context.Chunks
            .Where(c => c.ItemId == itemId)
            .Select(c => c.Id)
            .ToListAsync();

        if (chunkIds.Count > 0)
        {
            var connections = await _context.Connections
                .Where(c =>
                    chunkIds.Contains(c.SourceChunkId) ||
                    chunkIds.Contains(c.TargetChunkId))
                .ToListAsync();

            _context.Connections.RemoveRange(connections);
        }

        var chunks = await _context.Chunks
            .Where(c => c.ItemId == itemId)
            .ToListAsync();

        _context.Chunks.RemoveRange(chunks);

        _context.Items.Remove(item);

        await _context.SaveChangesAsync();

        return true;
    }
}