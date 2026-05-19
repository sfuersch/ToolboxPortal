using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
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

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            settings.SenderName,
            settings.SenderEmail));

        message.To.Add(MailboxAddress.Parse(lead.CustomerEmail));

        message.Subject = subject;

        message.Body = new BodyBuilder
        {
            HtmlBody = body
        }.ToMessageBody();

        using var smtp = new SmtpClient();

        var secureSocketOptions =
            settings.SmtpPort == 465 || settings.SmtpPort == 4465
                ? SecureSocketOptions.SslOnConnect
                : settings.UseSsl
                    ? SecureSocketOptions.StartTls
                    : SecureSocketOptions.None;

        await smtp.ConnectAsync(
            settings.SmtpHost,
            settings.SmtpPort,
            secureSocketOptions);

        await smtp.AuthenticateAsync(
            settings.SmtpUsername,
            settings.SmtpPassword);

        await smtp.SendAsync(message);

        await smtp.DisconnectAsync(true);

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