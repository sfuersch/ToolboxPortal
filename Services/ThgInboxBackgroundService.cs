namespace ToolboxPortal.Services;

public class ThgInboxBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ThgInboxBackgroundService> _logger;

    public ThgInboxBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ThgInboxBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();

                var service =
                    scope.ServiceProvider
                        .GetRequiredService<ThgInboxImportService>();

                await service.CheckInboxAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Fehler beim THG-Mailimport");
            }

            await Task.Delay(
                TimeSpan.FromMinutes(15),
                stoppingToken);
        }
    }
}