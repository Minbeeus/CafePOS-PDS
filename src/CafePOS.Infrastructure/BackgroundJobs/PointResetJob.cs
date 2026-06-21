using Microsoft.Extensions.Hosting;

namespace CafePOS.Infrastructure.BackgroundJobs;

public class PointResetJob : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Placeholder for background job that resets points every 2 months
        return Task.CompletedTask;
    }
}
