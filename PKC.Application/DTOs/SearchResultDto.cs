namespace PKC.Application.DTOs;

public class SearchResultDto
{
    public Guid Id { get; set; }

    public Guid ItemId { get; set; }

    public string Content { get; set; } = null!;

    public double Score { get; set; }
}