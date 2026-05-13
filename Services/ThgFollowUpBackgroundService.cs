using Microsoft.EntityFrameworkCore;
using ToolboxPortal.Data;
using ToolboxPortal.Models;

namespace ToolboxPortal.Services;

public class ThgFollowUpBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ThgFollowUpBackgroundService> _logger;

    public ThgFollowUpBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ThgFollowUpBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("THG Follow-up Background Service gestartet.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunAutomationForAllUsers(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler im THG Follow-up Background Service.");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    private async Task RunAutomationForAllUsers(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var followUpService = scope.ServiceProvider.GetRequiredService<ThgFollowUpService>();
        var scrapingService = scope.ServiceProvider.GetRequiredService<ThgScrapingService>();

        var settingsList = await dbContext.ThgFollowUpSettings
    .Where(x => !x.AutomationPaused)
    .ToListAsync(stoppingToken);

        foreach (var settings in settingsList)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }

            _logger.LogInformation(
    "THG Automation geprüft für User {UserId}. Letzter Lauf: {LastRun}, Intervall: {Interval} Stunden.",
    settings.UserId,
    settings.LastAutomationRunAt,
    settings.BackgroundRunEveryHours);

            if (!ShouldRun(settings))
            {
                continue;
            }

            try
            {
                if (settings.AutoRegistrationCheckEnabled)
                {
                    var matched = await RunRegistrationCheck(
                        dbContext,
                        scrapingService,
                        settings.UserId,
                        stoppingToken);

                    _logger.LogInformation(
                        "THG Registrierungsprüfung für User {UserId}: {Matched} Treffer.",
                        settings.UserId,
                        matched);
                }

                var result = await followUpService.RunFollowUpsAsync(
    settings.UserId,
    settings.MaxMailsPerRun);

                _logger.LogInformation(
                    "THG Follow-ups für User {UserId}: {Sent} gesendet, {Failed} Fehler.",
                    settings.UserId,
                    result.sent,
                    result.failed);

                settings.LastAutomationRunAt = DateTime.UtcNow;

                await dbContext.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Fehler bei THG Automation für User {UserId}.",
                    settings.UserId);
            }
        }
    }

    private static bool ShouldRun(ThgFollowUpSettings settings)
    {
        if (settings.BackgroundRunEveryHours <= 0)
        {
            return false;
        }

        if (settings.LastAutomationRunAt == null)
        {
            return true;
        }

        return settings.LastAutomationRunAt.Value
            .AddHours(settings.BackgroundRunEveryHours) <= DateTime.UtcNow;
    }

    private static async Task<int> RunRegistrationCheck(
        ApplicationDbContext dbContext,
        ThgScrapingService scrapingService,
        string userId,
        CancellationToken stoppingToken)
    {
        var entries = await scrapingService.GetReferralEntriesAsync();

        var referralNames = entries
            .Select(x => scrapingService.NormalizeName(x.Name))
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToHashSet();

        var customers = await dbContext.ThgCustomers
            .Where(x => x.UserId == userId && !x.IsRegistered)
            .ToListAsync(stoppingToken);

        var matched = 0;

        foreach (var customer in customers)
        {
            var customerName = scrapingService.NormalizeName(
                $"{customer.FirstName} {customer.LastName}");

            if (referralNames.Contains(customerName))
            {
                customer.IsRegistered = true;
                matched++;
            }
        }

        await dbContext.SaveChangesAsync(stoppingToken);

        return matched;
    }
}