using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PKC.Application.Interfaces;

namespace PKC.Infrastructure.Services;

public class ProcessingWorker : BackgroundService
{
    private readonly IBackgroundTaskQueue _queue;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProcessingWorker> _logger;
    private const int MaxConcurrentResources = 3;

    public ProcessingWorker(
        IBackgroundTaskQueue queue,
        IServiceProvider serviceProvider,
        ILogger<ProcessingWorker> logger)
    {
        _queue = queue;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Processing Worker started (max concurrency: {Max})", MaxConcurrentResources);

        var semaphore = new SemaphoreSlim(MaxConcurrentResources);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var resourceId = await _queue.DequeueAsync(stoppingToken);

                await semaphore.WaitAsync(stoppingToken);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var processor = scope.ServiceProvider.GetRequiredService<ResourceProcessingService>();
                        await processor.ProcessAsync(resourceId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing resource {ResourceId}", resourceId);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                }, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error in background worker loop");
            }
        }

        _logger.LogInformation("Processing Worker stopped");
    }
}