using Microsoft.EntityFrameworkCore;
using Pgvector.EntityFrameworkCore;
using PKC.Domain.Entities;
using PKC.Infrastructure.Data;

namespace PKC.Infrastructure.Services;

public class ConnectionService
{
    private readonly AppDbContext _context;

    public ConnectionService(AppDbContext context)
    {
        _context = context;
    }
public async Task CreateConnectionsAsync(Guid itemId)
{
    // 1. Get the IDs of the chunks we just processed
    var sourceChunkIds = await _context.Chunks
        .Where(c => c.ItemId == itemId && c.Embedding != null)
        .Select(c => c.Id)
        .ToListAsync();

    foreach (var sourceId in sourceChunkIds)
    {
        // 2. Fetch the source embedding first
        var sourceChunk = await _context.Chunks.FindAsync(sourceId);
        if (sourceChunk?.Embedding == null) continue;

        // 3. Let the Database find similar chunks (Server-side evaluation)
        var relatedConnections = await _context.Chunks
            .Where(target => target.Id != sourceId && target.Embedding != null)
            // This translates to SQL <=> operator
            .Where(target => target.Embedding!.CosineDistance(sourceChunk.Embedding) < 0.2) 
            .Select(target => new Connection
            {
                Id = Guid.NewGuid(),
                SourceChunkId = sourceId,
                TargetChunkId = target.Id,
                Score = (double)target.Embedding!.CosineDistance(sourceChunk.Embedding)
            })
            .ToListAsync();

        await _context.Connections.AddRangeAsync(relatedConnections);
    }

    await _context.SaveChangesAsync();
}

/*
    public async Task CreateConnectionsAsync(Guid itemId)
    {
        var chunks = await _context.Chunks
            .Where(c => c.ItemId == itemId && c.Embedding != null)
            .ToListAsync();

        var allChunks = await _context.Chunks
            .Where(c => c.Embedding != null)
            .ToListAsync();

        var connections = new List<Connection>();

        foreach (var source in chunks)
        {
            foreach (var target in allChunks)
            {
                if (source.Id == target.Id) continue;

                var score = source.Embedding!.CosineDistance(target.Embedding!);

                if (score < 0.2) // similarity threshold
                {
                    connections.Add(new Connection
                    {
                        Id = Guid.NewGuid(),
                        SourceChunkId = source.Id,
                        TargetChunkId = target.Id,
                        Score = score
                    });
                }
            }
        }

        await _context.Connections.AddRangeAsync(connections);
        await _context.SaveChangesAsync();
    }

    */
}