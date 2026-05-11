using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace ToolboxPortal.Services;

public class EmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
    {
        var host = _configuration["Smtp:Host"];
        var username = _configuration["Smtp:Username"];
        var password = _configuration["Smtp:Password"];
        var fromEmail = _configuration["Smtp:FromEmail"];
        var fromName = _configuration["Smtp:FromName"] ?? "ToolboxPortal";

        if (string.IsNullOrWhiteSpace(host))
            throw new InvalidOperationException("SMTP Host ist nicht konfiguriert.");

        if (string.IsNullOrWhiteSpace(username))
            throw new InvalidOperationException("SMTP Benutzername ist nicht konfiguriert.");

        if (string.IsNullOrWhiteSpace(password))
            throw new InvalidOperationException("SMTP Passwort ist nicht konfiguriert.");

        if (string.IsNullOrWhiteSpace(fromEmail))
            throw new InvalidOperationException("SMTP Absenderadresse ist nicht konfiguriert.");

        if (string.IsNullOrWhiteSpace(toEmail))
            throw new InvalidOperationException("Empfängeradresse ist leer.");

        var port = int.Parse(_configuration["Smtp:Port"] ?? "587");
        var useSsl = bool.Parse(_configuration["Smtp:UseSsl"] ?? "true");

        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;

        message.Body = new BodyBuilder
        {
            HtmlBody = htmlBody
        }.ToMessageBody();

        using var client = new SmtpClient();

        var secureSocketOptions = port == 465 || port == 4465
            ? SecureSocketOptions.SslOnConnect
            : useSsl
                ? SecureSocketOptions.StartTls
                : SecureSocketOptions.Auto;

        await client.ConnectAsync(host, port, secureSocketOptions);
        await client.AuthenticateAsync(username, password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}