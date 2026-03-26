using Microsoft.Extensions.Logging;
using Pgvector;
using System.Text;
using System.Text.Json;

namespace PKC.Infrastructure.Services;

public class EmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(HttpClient httpClient, ILogger<EmbeddingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Vector> GenerateEmbeddingAsync(string text)
    {
        var requestBody = new
        {
            model = "nomic-embed-text",
            prompt = text
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            var response = await _httpClient.PostAsync("http://localhost:11434/api/embeddings", content);

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            var embeddingArray = doc.RootElement
                .GetProperty("embedding")
                .EnumerateArray()
                .Select(x => x.GetSingle())
                .ToArray();
                
            _logger.LogInformation("Generated embedding with dimension: {Dimension}", embeddingArray.Length);
            return new Vector(embeddingArray);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Embedding generation failed");
            throw;
        }
    }
}