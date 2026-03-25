namespace PKC.Domain.Entities;

public enum ItemType
{
    Url,
    Pdf,
    Note
}

public enum ItemStatus
{
    Pending,
    Extracting,
    Chunking,
    Embedding,
    Tagging,
    Ready,
    Failed
}

public class Item
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public ItemType Type { get; set; }

    public string? Title { get; set; }

    public string? SourceUrl { get; set; }

    public string? FilePath { get; set; }

    public string? RawContent { get; set; }

    public string? ExtractedText { get; set; }

    public ItemStatus Status { get; set; } = ItemStatus.Pending;

    public string? FailureReason { get; set; }

    public int? WordCount { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ProcessedAt { get; set; }
}