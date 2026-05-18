using Microsoft.EntityFrameworkCore;
using ToolboxPortal.Data;
using ToolboxPortal.Models;

namespace ToolboxPortal.Services.LeadOptimizer;

public class LeadAutomationService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public LeadAutomationService(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task RunAsync(
        string triggerEvent,
        LeadOptimizerLead lead)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var rules = await db.LeadAutomationRules
            .Where(x =>
                x.IsActive &&
                x.TriggerEvent == triggerEvent)
            .ToListAsync();

        foreach (var rule in rules)
        {
            if (rule.ActionType == "CreateTask")
            {
                db.LeadOptimizerTasks.Add(
                    new LeadOptimizerTask
                    {
                        LeadId = lead.Id,

                        Title = rule.TaskTitle ?? "Aufgabe",

                        Description = rule.TaskDescription,

                        Priority = rule.TaskPriority,

                        DueAt = DateTime.UtcNow
                            .AddMinutes(rule.DueOffsetMinutes),

                        CreatedAt = DateTime.UtcNow,

                        IsCompleted = false
                    });
            }

            db.LeadOptimizerLeadEvents.Add(
                new LeadOptimizerLeadEvent
                {
                    LeadId = lead.Id,

                    EventType = "automation",

                    Title = "Automation ausgeführt",

                    Description = rule.Name
                });
        }

        await db.SaveChangesAsync();
    }
}