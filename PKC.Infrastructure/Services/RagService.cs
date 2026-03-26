namespace PKC.Infrastructure.Services;

public class RagService
{
    private readonly SearchService _searchService;
    private readonly AiService _aiService;

    public RagService(SearchService searchService, AiService aiService)
    {
        _searchService = searchService;
        _aiService = aiService;
    }

    public async Task<string> AskAsync(string query)
    {
        var results = await _searchService.SearchAsync(query);

        // var contextChunks = results
        //     .Select(r => r.Content)
        //     .ToList();

        var contextChunks = results.Take(2).Select(r => r.Content.Length > 500 ? r.Content.Substring(0,500) : r.Content).ToList();

        return await _aiService.GenerateAnswer(query, contextChunks);
    }
}