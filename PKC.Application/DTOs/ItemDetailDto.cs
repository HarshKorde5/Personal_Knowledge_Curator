
namespace PKC.Application.DTOs;

public class ItemDetailDto
{
    public Guid Id { get; set; }

    public string? Title { get; set; }

    public string Type { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? SourceUrl { get; set; }

    public int? WordCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ProcessedAt { get; set; }

    public string? FailureReason { get; set; }

    public string? ContentPreview { get; set; }

    public List<string> Tags { get; set; } = new();

    public List<ConnectedItemDto> ConnectedItems { get; set; } = new();
}