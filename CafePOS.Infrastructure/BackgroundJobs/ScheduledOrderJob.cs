using Microsoft.Extensions.Hosting;

namespace CafePOS.Infrastructure.BackgroundJobs;

public class ScheduledOrderJob : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Placeholder for background job that checks/activates scheduled orders
        return Task.CompletedTask;
    }
}
