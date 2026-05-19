using System.Net;
using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using ToolboxPortal.Data;
using ToolboxPortal.Models;

namespace ToolboxPortal.Services.LeadOptimizer;

public class LeadEmailService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public LeadEmailService(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task SendQualificationEmailAsync(
    LeadOptimizerLead lead,
    string qualificationUrl)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var settings = await db.LeadEmailSettings
            .FirstOrDefaultAsync(x => x.UserId == lead.UserId);

        if (settings == null)
        {
            throw new Exception("Keine SMTP Einstellungen gefunden.");
        }

        if (string.IsNullOrWhiteSpace(lead.CustomerEmail))
        {
            throw new Exception("Lead besitzt keine E-Mail-Adresse.");
        }

        var template = await db.LeadMailTemplates
            .FirstOrDefaultAsync(x =>
                x.UserId == lead.UserId
                && x.TemplateKey == "QualificationMail");

        
        string subject;
        string body;

        if (template != null)
        {
            subject = ReplaceVariables(
                template.Subject,
                lead,
                qualificationUrl);

            body = ReplaceVariables(
                template.HtmlBody,
                lead,
                qualificationUrl);
        }
        else
        {
            subject = "Weitere Informationen zu Ihrer Anfrage";

            body = $@"
<h2>Hallo {lead.CustomerFirstName},</h2>

<p>
vielen Dank für Ihre Anfrage.
</p>

<p>
Bitte ergänzen Sie noch einige Informationen:
</p>

<p>
<a href='{qualificationUrl}'>
Jetzt Daten ergänzen
</a>
</p>";
        }

        using var client = new SmtpClient(
            settings.SmtpHost,
            settings.SmtpPort);

        client.EnableSsl = settings.UseSsl;

        client.Credentials = new NetworkCredential(
            settings.SmtpUsername,
            settings.SmtpPassword);

        var mail = new MailMessage
        {
            From = new MailAddress(
                settings.SenderEmail,
                settings.SenderName),

            Subject = subject,
            Body = body,
            IsBodyHtml = true
        };

        mail.To.Add(lead.CustomerEmail);

        await client.SendMailAsync(mail);

        lead.QualificationEmailSentAt = DateTime.UtcNow;

        db.LeadOptimizerLeadEvents.Add(
            new LeadOptimizerLeadEvent
            {
                LeadId = lead.Id,
                EventType = "QualificationMailSent",
                Title = "Qualifizierungs-Mail versendet",
                Description = $"Versendet an {lead.CustomerEmail}",
                CreatedAt = DateTime.UtcNow
            });

        await db.SaveChangesAsync();
    }

    private string ReplaceVariables(
        string? content,
        LeadOptimizerLead lead,
        string qualificationUrl)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return "";
        }

        return content
            .Replace("{FirstName}", lead.CustomerFirstName ?? "")
            .Replace("{LastName}", lead.CustomerLastName ?? "")
            .Replace("{FullName}", lead.CustomerName ?? "")
            .Replace("{VehicleTitle}", lead.VehicleTitle ?? "")
            .Replace("{QualificationUrl}", qualificationUrl)
            .Replace("{CurrentDate}",
                DateTime.Now.ToString("dd.MM.yyyy"));
    }
}