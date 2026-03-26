namespace PKC.Domain.Entities;

public class Connection
{
    public Guid Id { get; set; }

    public Guid SourceChunkId { get; set; }

    public Guid TargetChunkId { get; set; }

    public double Score { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}