using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using PKC.Application.DTOs;
using PKC.Domain.Entities;
using PKC.Infrastructure.Data;

namespace PKC.Infrastructure.Services;

public class SearchService
{
    private readonly AppDbContext _context;
    private readonly EmbeddingService _embeddingService;

    // FIX: Cosine distance threshold. Values >= this are considered semantically
    // unrelated and excluded from results. Range is 0 (identical) to 2 (opposite).
    // 0.5 is a practical cutoff - anything further is noise, not signal.
    private const double SimilarityThreshold = 0.5;

    public SearchService(AppDbContext context, EmbeddingService embeddingService)
    {
        _context = context;
        _embeddingService = embeddingService;
    }

    public async Task<List<SearchResultDto>> SearchAsync(string query, Guid userId)
    {
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);

        var candidates = await _context.Chunks
            .Where(c => c.UserId == userId && c.Embedding != null)
            .OrderBy(c => c.Embedding!.CosineDistance(queryEmbedding))
            .Take(10)
            .Select(c => new SearchResultDto
            {
                Id = c.Id,
                ResourceId = c.ResourceId,
                Content = c.Content,
                Score = (double)c.Embedding!.CosineDistance(queryEmbedding)
            })
            .ToListAsync();

        return candidates
            .Where(r => r.Score < SimilarityThreshold)
            .Take(5)
            .ToList();
    }
}