using TreasureHuntApp.Infrastructure.Services;

namespace TreasureHuntApp.API.Services;

public class EventStatusBackgroundService(
    IServiceProvider serviceProvider,
    ILogger<EventStatusBackgroundService> logger)
    : BackgroundService
{
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // Check every minute

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Event Status Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
                    await eventService.CheckAndUpdateEventStatusesAsync();
                }

                logger.LogDebug("Event status check completed at {Time}", DateTimeOffset.Now);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while checking event statuses");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        logger.LogInformation("Event Status Background Service stopped");
    }
}
