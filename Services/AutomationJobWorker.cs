using Microsoft.EntityFrameworkCore;
using ToolboxPortal.Data;
using ToolboxPortal.Services.LeadOptimizer;

namespace ToolboxPortal.Services;

public class AutomationJobWorker
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AutomationJobWorker(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var db = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

                var automationService =
                    scope.ServiceProvider
                        .GetRequiredService<LeadAutomationService>();

                var jobs = await db.AutomationJobs
                    .Where(x => x.Status == "Pending")
                    .OrderBy(x => x.CreatedAt)
                    .Take(10)
                    .ToListAsync(stoppingToken);

                foreach (var job in jobs)
                {
                    try
                    {
                        job.Status = "Running";
                        job.StartedAt = DateTime.UtcNow;

                        await db.SaveChangesAsync(
                            stoppingToken);

                        var lead = await db.LeadOptimizerLeads
                            .FirstOrDefaultAsync(
                                x => x.Id == job.LeadId,
                                stoppingToken);

                        if (lead == null)
                        {
                            job.Status = "Failed";
                            job.ErrorMessage =
                                "Lead nicht gefunden.";

                            continue;
                        }

                        await automationService.RunAsync(
                            job.JobType,
                            lead);

                        job.Status = "Completed";
                        job.FinishedAt = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        job.Status = "Failed";
                        job.ErrorMessage = ex.Message;
                        job.RetryCount++;
                    }
                }

                await db.SaveChangesAsync(
                    stoppingToken);
            }
            catch
            {
            }

            await Task.Delay(
                TimeSpan.FromSeconds(5),
                stoppingToken);
        }
    }
}