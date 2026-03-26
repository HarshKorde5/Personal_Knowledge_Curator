using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace PKC.Infrastructure.Services;

public class AiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiService> _logger;

    public AiService(HttpClient httpClient, ILogger<AiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string> GenerateAnswer(string query, List<string> contextChunks)
    {
        var context = string.Join("\n\n", contextChunks);

        var prompt = $"""
            You are a helpful AI assistant.

            Answer the question using only the context.

            Keep the answer short and precise.

            Context:
            {context}

            Question:
            {query}

            Answer:
            """;

        var requestBody = new
        {
            model = "gemma3:1b",
            prompt = prompt,
            stream = false,
            options = new { num_predict = 150 }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            var response = await _httpClient.PostAsync("http://localhost:11434/api/generate", content);

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            return doc.RootElement.GetProperty("response").GetString() ?? "";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ollama request failed");
            return "Error generating response";
        }
    }
}