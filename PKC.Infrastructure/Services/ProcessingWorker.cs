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
        _logger.LogInformation("Processing Worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var itemId = await _queue.DequeueAsync(stoppingToken);

                using var scope = _serviceProvider.CreateScope();

                var processor = scope.ServiceProvider.GetRequiredService<ItemProcessingService>();

                await processor.ProcessAsync(itemId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background worker");
            }
        }
    }
}