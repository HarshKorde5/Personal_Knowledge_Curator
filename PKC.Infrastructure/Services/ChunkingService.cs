using PKC.Domain.Entities;

namespace PKC.Infrastructure.Services;

public class ChunkingService
{
    private const int ChunkSize = 500;
    private const int Overlap = 50;

    public List<Chunk> CreateChunks(Guid resourceId, Guid userId, string text)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var chunks = new List<Chunk>();
        int order = 0;
        int step = ChunkSize - Overlap;

        for (int i = 0; i < words.Length; i += step)
        {
            var chunkWords = words.Skip(i).Take(ChunkSize).ToArray();

            // Skip near-empty tail chunks (less than 10% of ChunkSize)
            if (chunkWords.Length < ChunkSize / 10)
                break;

            var content = string.Join(" ", chunkWords);

            chunks.Add(new Chunk
            {
                Id = Guid.NewGuid(),
                ResourceId = resourceId,
                UserId = userId,
                Content = content,
                Order = order++,
                WordCount = chunkWords.Length
            });
        }

        return chunks;
    }
}