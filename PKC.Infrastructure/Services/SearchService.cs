using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using PKC.Application.DTOs;
using PKC.Infrastructure.Data;

namespace PKC.Infrastructure.Services;

public class SearchService
{
    private readonly AppDbContext _context;
    private readonly EmbeddingService _embeddingService;

    public SearchService(AppDbContext context, EmbeddingService embeddingService)
    {
        _context = context;
        _embeddingService = embeddingService;
    }

    public async Task<List<SearchResultDto>> SearchAsync(string query)
    {
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query);

        var results = await _context.Chunks
            .Where(c => c.Embedding != null)
            .OrderBy(c => c.Embedding!.CosineDistance(queryEmbedding))
            .Take(5)
            .Select(c => new SearchResultDto
            {
                Content = c.Content,
                Score = (Double)c.Embedding!.CosineDistance(queryEmbedding)
            })
            .ToListAsync();

        return results;
    }
}