using Microsoft.EntityFrameworkCore;
using ToolboxPortal.Data;
using ToolboxPortal.Models;

namespace ToolboxPortal.Services;

public class ThgFollowUpService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly EmailService _emailService;
    private readonly ThgMailTemplateService _templateService;

    public bool AutomationPaused { get; set; }

    public ThgFollowUpService(
        ApplicationDbContext dbContext,
        EmailService emailService,
        ThgMailTemplateService templateService)
    {
        _dbContext = dbContext;
        _emailService = emailService;
        _templateService = templateService;
    }

    public async Task<(int sent, int failed)> RunFollowUpsAsync(
        string userId,
        int maxMailsPerRun = 50)
    {
        var settings = await _dbContext.ThgFollowUpSettings
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (settings == null || settings.AutomationPaused)
        {
            return (0, 0);
        }

        var berlinTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Berlin");

        var berlinNow = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.UtcNow,
            berlinTimeZone);

        var localNow = berlinNow.TimeOfDay;

        if (localNow < settings.MailSendWindowStart
            || localNow > settings.MailSendWindowEnd)
        {
            return (0, 0);
        }

        var customers = await _dbContext.ThgCustomers
    .Where(x =>
        x.UserId == userId
        && !x.IsRegistered
        && !string.IsNullOrWhiteSpace(x.Email)
        && (
            !x.InitialMailSent
            || x.FollowUpCount < settings.MaxFollowUps
        ))
    .OrderBy(x => x.CreatedAt)
    .ToListAsync();

        var sent = 0;
        var failed = 0;
        var processed = 0;
        var now = DateTime.UtcNow;

        foreach (var customer in customers)
        {
            if (processed >= maxMailsPerRun)
            {
                break;
            }

            var nextFollowUp = GetNextFollowUp(customer, settings, now);

            if (nextFollowUp == null)
            {
                continue;
            }

            var template = await _templateService.GetTemplateAsync(userId, nextFollowUp.TemplateKey);

            if (template == null)
            {
                failed++;
                processed++;
                continue;
            }

            try
            {
                var subject = _templateService.RenderTemplate(template.Subject, customer);
                var body = _templateService.RenderTemplate(template.HtmlBody, customer);

                await _emailService.SendEmailAsync(customer.Email, subject, body);

                if (nextFollowUp.Number == 0)
                {
                    customer.InitialMailSent = true;
                    customer.InitialMailSentAt = now;
                }
                else
                {
                    customer.FollowUpCount++;

                    if (nextFollowUp.Number == 1)
                    {
                        customer.FollowUp1SentAt = now;
                    }
                    else if (nextFollowUp.Number == 2)
                    {
                        customer.FollowUp2SentAt = now;
                    }
                    else if (nextFollowUp.Number == 3)
                    {
                        customer.FollowUp3SentAt = now;
                    }
                }

                customer.LastMailSentAt = now;

                _dbContext.ThgMailLogs.Add(new ThgMailLog
                {
                    ThgCustomerId = customer.Id,
                    RecipientEmail = customer.Email,
                    MailType = nextFollowUp.TemplateKey,
                    Subject = subject,
                    SentAt = now,
                    Success = true
                });

                sent++;
                processed++;

                await Task.Delay(2000);
            }
            catch (Exception ex)
            {
                _dbContext.ThgMailLogs.Add(new ThgMailLog
                {
                    ThgCustomerId = customer.Id,
                    RecipientEmail = customer.Email,
                    MailType = nextFollowUp.TemplateKey,
                    Subject = nextFollowUp.TemplateKey,
                    SentAt = now,
                    Success = false,
                    ErrorMessage = ex.Message
                });

                failed++;
                processed++;
            }
        }

        await _dbContext.SaveChangesAsync();

        return (sent, failed);
    }

    private static FollowUpStep? GetNextFollowUp(
    ThgCustomer customer,
    ThgFollowUpSettings settings,
    DateTime now)
    {
        if (!customer.InitialMailSent)
        {
            return new FollowUpStep(0, "Initial");
        }

        if (customer.InitialMailSentAt == null)
        {
            return null;
        }

        if (customer.FollowUpCount == 0
            && customer.FollowUp1SentAt == null
            && customer.InitialMailSentAt.Value.AddDays(settings.FollowUp1AfterDays) <= now)
        {
            return new FollowUpStep(1, "FollowUp1");
        }

        if (customer.FollowUpCount == 1
            && customer.FollowUp1SentAt != null
            && customer.FollowUp1SentAt.Value.AddDays(settings.FollowUp2AfterDays) <= now)
        {
            return new FollowUpStep(2, "FollowUp2");
        }

        if (customer.FollowUpCount == 2
            && customer.FollowUp2SentAt != null
            && customer.FollowUp2SentAt.Value.AddDays(settings.FollowUp3AfterDays) <= now)
        {
            return new FollowUpStep(3, "FollowUp3");
        }

        return null;
    }

    private sealed record FollowUpStep(int Number, string TemplateKey);
}