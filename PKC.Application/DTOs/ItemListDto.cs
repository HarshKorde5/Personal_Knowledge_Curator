namespace PKC.Application.DTOs;

public class ItemListDto
{
    public Guid Id { get; set; }

    public string? Title { get; set; }

    public string Type { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? SourceUrl { get; set; }

    public int? WordCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public List<string> Tags { get; set; } = new();
}