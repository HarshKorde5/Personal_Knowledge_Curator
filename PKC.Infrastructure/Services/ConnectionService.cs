using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector.EntityFrameworkCore;
using PKC.Domain.Entities;
using PKC.Infrastructure.Data;

namespace PKC.Infrastructure.Services;

public class ConnectionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ConnectionService> _logger;

    public ConnectionService(AppDbContext context, ILogger<ConnectionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task CreateConnectionsAsync(Guid itemId)
    {
        var sourceChunks = await _context.Chunks
            .Where(c => c.ItemId == itemId && c.Embedding != null)
            .Take(20)
            .ToListAsync();

        if (sourceChunks.Count == 0)
        {
            _logger.LogWarning("No chunks found for item {ItemId}", itemId);
            return;
        }

        var connections = new List<Connection>();

        foreach (var sourceChunk in sourceChunks)
        {
            var sourceEmbedding = sourceChunk.Embedding!;

            var similarChunks = await _context.Chunks
                .Where(target =>
                    target.Id != sourceChunk.Id &&
                    target.Embedding != null
                )
                .OrderBy(target =>
                    target.Embedding!.CosineDistance(sourceEmbedding)
                )
                .Take(3)
                .Select(target => new
                {
                    target.Id,
                    Score = target.Embedding!.CosineDistance(sourceEmbedding)
                })
                .ToListAsync();

            foreach (var match in similarChunks)
            {
                if (match.Score < 0.3)
                {
                    connections.Add(new Connection
                    {
                        Id = Guid.NewGuid(),
                        SourceChunkId = sourceChunk.Id,
                        TargetChunkId = match.Id,
                        Score = match.Score,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        if (connections.Count > 0)
        {
            await _context.Connections.AddRangeAsync(connections);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Created {Count} connections for item {ItemId}",
                connections.Count,
                itemId
            );
        }
        else
        {
            _logger.LogInformation("No connections created for item {ItemId}", itemId);
        }
    }
}