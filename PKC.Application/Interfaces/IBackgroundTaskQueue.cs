namespace PKC.Application.Interfaces;

public interface IBackgroundTaskQueue
{
    void QueueResource(Guid resourceId);
    Task<Guid> DequeueAsync(CancellationToken cancellationToken);
}