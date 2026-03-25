namespace PKC.Application.Interfaces;

public interface IBackgroundTaskQueue
{
    void QueueItem(Guid itemId);
    Task<Guid> DequeueAsync(CancellationToken cancellationToken);
}