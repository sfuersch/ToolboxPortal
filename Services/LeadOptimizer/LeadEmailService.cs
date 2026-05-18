using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using ToolboxPortal.Data;
using ToolboxPortal.Models;

namespace ToolboxPortal.Services.LeadOptimizer;

public class LeadEmailService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public LeadEmailService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task SendQualificationEmailAsync(
        LeadOptimizerLead lead,
        string qualificationUrl)
    {
        if (string.IsNullOrWhiteSpace(lead.CustomerEmail))
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var settings = await db.LeadEmailSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == lead.UserId);

        if (settings == null || string.IsNullOrWhiteSpace(settings.SmtpHost))
        {
            throw new InvalidOperationException("Lead-Mail-Einstellungen fehlen.");
        }

        var subject = "Ihre Fahrzeuganfrage vervollständigen";

        var greeting = string.IsNullOrWhiteSpace(lead.CustomerName)
            ? "Guten Tag,"
            : $"Guten Tag {lead.CustomerName},";

        var vehicleText = string.IsNullOrWhiteSpace(lead.VehicleTitle)
            ? "Ihr gewünschtes Fahrzeug"
            : lead.VehicleTitle;

        var body = $@"
<p>{greeting}</p>

<p>vielen Dank für Ihre Anfrage zu <strong>{vehicleText}</strong>.</p>

<p>
Damit wir Ihr persönliches Angebot optimal vorbereiten können,
bitten wir Sie um ein paar kurze Angaben.
</p>

<p>Das dauert weniger als eine Minute.</p>

<p>
<a href=""{qualificationUrl}""
   style=""display:inline-block;padding:12px 18px;background:#0d6efd;color:#ffffff;text-decoration:none;border-radius:6px;"">
    Anfrage vervollständigen
</a>
</p>

<p>
Alternativ können Sie diesen Link öffnen:<br>
{qualificationUrl}
</p>

<p>
Freundliche Grüße<br>
{settings.SenderName}
</p>";

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

        var secureSocketOptions = settings.SmtpPort == 465 || settings.SmtpPort == 4465
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
    }
}