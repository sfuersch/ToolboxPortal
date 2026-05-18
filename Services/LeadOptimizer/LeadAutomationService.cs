using Microsoft.EntityFrameworkCore;
using ToolboxPortal.Data;
using ToolboxPortal.Models;

namespace ToolboxPortal.Services.LeadOptimizer;

public class LeadAutomationService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly LeadEmailService _leadEmailService;

    public LeadAutomationService(
        IServiceScopeFactory scopeFactory,
        LeadEmailService leadEmailService)
    {
        _scopeFactory = scopeFactory;
        _leadEmailService = leadEmailService;
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
                x.TriggerEvent == triggerEvent &&
                x.UserId == lead.UserId)
            .ToListAsync();

        if (!rules.Any())
        {
            db.LeadOptimizerLeadEvents.Add(new LeadOptimizerLeadEvent
            {
                LeadId = lead.Id,
                EventType = "automation_no_rule",
                Title = "Keine Automation gefunden",
                Description = $"Trigger: {triggerEvent}"
            });

            await db.SaveChangesAsync();
            return;
        }

        foreach (var rule in rules)
        {
            db.LeadOptimizerLeadEvents.Add(new LeadOptimizerLeadEvent
            {
                LeadId = lead.Id,
                EventType = "automation_started",
                Title = "Automation gestartet",
                Description = $"{rule.Name} / {rule.ActionType}"
            });

            if (rule.ActionType == "CreateTask")
            {
                db.LeadOptimizerTasks.Add(new LeadOptimizerTask
                {
                    LeadId = lead.Id,
                    Title = rule.TaskTitle ?? "Aufgabe",
                    Description = rule.TaskDescription,
                    Priority = rule.TaskPriority,
                    DueAt = DateTime.UtcNow.AddMinutes(rule.DueOffsetMinutes),
                    CreatedAt = DateTime.UtcNow,
                    IsCompleted = false
                });
            }

            if (rule.ActionType == "SendQualificationMail")
            {
                try
                {
                    var qualificationUrl =
                        $"https://toolbox.promotekk.com/lead/q/{lead.QualificationToken}";

                    await _leadEmailService.SendQualificationEmailAsync(
                        lead,
                        qualificationUrl);

                    var dbLead = await db.LeadOptimizerLeads
                        .FirstOrDefaultAsync(x => x.Id == lead.Id);

                    if (dbLead != null)
                    {
                        dbLead.QualificationEmailSentAt = DateTime.UtcNow;
                        dbLead.Status = "email_sent";
                        dbLead.UpdatedAt = DateTime.UtcNow;
                    }

                    db.LeadOptimizerLeadEvents.Add(new LeadOptimizerLeadEvent
                    {
                        LeadId = lead.Id,
                        EventType = "mail_sent",
                        Title = "Qualifizierungs-Mail gesendet",
                        Description = lead.CustomerEmail
                    });
                }
                catch (Exception ex)
                {
                    db.LeadOptimizerLeadEvents.Add(new LeadOptimizerLeadEvent
                    {
                        LeadId = lead.Id,
                        EventType = "mail_failed",
                        Title = "Qualifizierungs-Mail fehlgeschlagen",
                        Description = ex.Message
                    });
                }
            }

            db.LeadOptimizerLeadEvents.Add(new LeadOptimizerLeadEvent
            {
                LeadId = lead.Id,
                EventType = "automation_finished",
                Title = "Automation beendet",
                Description = rule.Name
            });
        }

        await db.SaveChangesAsync();
    }
}