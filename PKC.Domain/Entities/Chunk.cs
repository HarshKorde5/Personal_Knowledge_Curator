namespace PKC.Domain.Entities;

public class Chunk
{
    public Guid Id { get; set; }

    public Guid ItemId { get; set; }

    public string Content { get; set; } = null!;

    public int Order { get; set; }

    public int WordCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}