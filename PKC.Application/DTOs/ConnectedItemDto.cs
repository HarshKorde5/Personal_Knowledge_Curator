namespace PKC.Application.DTOs;

public class ConnectedItemDto
{
    public Guid ItemId { get; set; }

    public string? Title { get; set; }

    public string ItemType { get; set; } = null!;

    public double Strength { get; set; }
}