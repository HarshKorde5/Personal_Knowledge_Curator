using PKC.Domain.Entities;

namespace PKC.Infrastructure.Services;

public class ChunkingService
{
    private const int CHUNK_SIZE = 300;

    public List<Chunk> CreateChunks(Guid itemId, string text)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var chunks = new List<Chunk>();

        int order = 0;

        for (int i = 0; i < words.Length; i += CHUNK_SIZE)
        {
            var chunkWords = words.Skip(i).Take(CHUNK_SIZE).ToArray();

            var content = string.Join(" ", chunkWords);

            chunks.Add(new Chunk
            {
                Id = Guid.NewGuid(),
                ItemId = itemId,
                Content = content,
                Order = order++,
                WordCount = chunkWords.Length
            });
        }

        return chunks;
    }
}