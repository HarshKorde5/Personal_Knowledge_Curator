using Microsoft.EntityFrameworkCore;
using PKC.Infrastructure.Data;
using PKC.Application.DTOs;

namespace PKC.Infrastructure.Services;

public class RagService
{
    private readonly SearchService _searchService;
    private readonly AiService _aiService;
    private readonly ResurfacingService _resurfacingService;

    private readonly AppDbContext _context;
    public RagService(SearchService searchService, AiService aiService, ResurfacingService resurfacingService, AppDbContext context)
    {
        _searchService = searchService;
        _aiService = aiService;
        _resurfacingService = resurfacingService;
        _context = context;
    }

    public async Task<object> AskAsync(string query)
    {
        // 1. Get similar chunks via Vector Search
        var searchResults = await _searchService.SearchAsync(query);

        if (searchResults == null || !searchResults.Any())
        {
            return new { answer = "No relevant context found.", related = new List<object>() };
        }

        // 2. Extract top 3 contents for the AI prompt
        var contextChunks = searchResults
            .Take(3)
            .Select(r => r.Content.Length > 800 ? r.Content.Substring(0, 800) : r.Content)
            .ToList();

        // 3. Generate the answer
        var answer = await _aiService.GenerateAnswer(query, contextChunks);

        // 4. Get related content using the ID we already have
        // No more 'FirstOrDefaultAsync' errors or extra DB lookups!
        var topResult = searchResults.First();
        var related = await _resurfacingService.GetRelatedAsync(topResult.Id);

        return new
        {
            answer,
            related,
            topMatchId = topResult.Id
        };
    }
}