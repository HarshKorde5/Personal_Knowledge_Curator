namespace PKC.Infrastructure.Services;

using System.Threading.Channels;
using PKC.Application.Interfaces;



public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Guid> _queue;

    public BackgroundTaskQueue()
    {
        _queue = Channel.CreateUnbounded<Guid>();
    }

    public void QueueResource(Guid resourceId)
    {
        _queue.Writer.TryWrite(resourceId);
    }

    public async Task<Guid> DequeueAsync(CancellationToken cancellationToken)
    {
        return await _queue.Reader.ReadAsync(cancellationToken);
    }
}