namespace PKC.Application.DTOs;

public class ItemStatusDto
{
    public string Status { get; set; } = null!;

    public string? FailureReason { get; set; }
}