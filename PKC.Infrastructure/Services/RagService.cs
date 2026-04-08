using PKC.Application.DTOs;
using PKC.Infrastructure.Data;

namespace PKC.Infrastructure.Services;

public class RagService
{
    private readonly SearchService _searchService;
    private readonly AiService _aiService;
    private readonly ResurfacingService _resurfacingService;
    private readonly AppDbContext _context;

    public RagService(
        SearchService searchService,
        AiService aiService,
        ResurfacingService resurfacingService,
        AppDbContext context)
    {
        _searchService = searchService;
        _aiService = aiService;
        _resurfacingService = resurfacingService;
        _context = context;
    }

    public async Task<object> AskAsync(string query, Guid userId)
    {
        var searchResults = await _searchService.SearchAsync(query, userId);

        if (searchResults == null || !searchResults.Any())
        {
            return new { answer = "No relevant context found.", related = new List<object>() };
        }

        var contextChunks = searchResults
            .Take(3)
            .Select(r => r.Content)
            .ToList();

        var answer = await _aiService.GenerateAnswer(query, contextChunks);

        var topResult = searchResults.First();
        var related = await _resurfacingService.GetRelatedAsync(topResult.Id, userId);

        return new
        {
            answer,
            related,
            topMatchId = topResult.Id,
            sourceResourceId = topResult.ResourceId
        };
    }
}