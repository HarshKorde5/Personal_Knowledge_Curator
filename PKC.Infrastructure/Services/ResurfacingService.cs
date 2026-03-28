using Microsoft.EntityFrameworkCore;
using PKC.Application.DTOs;
using PKC.Infrastructure.Data;

namespace PKC.Infrastructure.Services;

public class ResurfacingService
{
    private readonly AppDbContext _context;

    public ResurfacingService(AppDbContext context)
    {
        _context = context;
    }
    public async Task<List<ResurfaceResultDto>> GetRelatedAsync(Guid chunkId, Guid userId)
    {
        var results = await _context.Connections
            .Where(c => c.SourceChunkId == chunkId && c.UserId == userId)
            .OrderBy(c => c.Score)
            .Take(5)
            .Join(
                _context.Chunks.Where(ch => ch.UserId == userId),
                connection => connection.TargetChunkId,
                chunk => chunk.Id,
                (connection, chunk) => new ResurfaceResultDto
                {
                    Content = chunk.Content,
                    Score = connection.Score
                })
            .ToListAsync();

        return results;
    }
}