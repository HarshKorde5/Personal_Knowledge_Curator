using Microsoft.Extensions.Logging;
using Pgvector;

namespace PKC.Infrastructure.Services;

public class EmbeddingService
{
    private readonly ILogger<EmbeddingService> _logger;

    public EmbeddingService(ILogger<EmbeddingService> logger)
    {
        _logger = logger;
    }

    // TEMP: fake embedding (we'll replace with real AI next)
    public Vector GenerateEmbedding(string text)
    {
        var random = new Random(text.GetHashCode());

        var values = new float[1536];

        for (int i = 0; i < values.Length; i++)
        {
            values[i] = (float)(random.NextDouble() * 2 - 1);
        }

        return new Vector(values);
    }
}