using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using ToolboxPortal.Data;
using ToolboxPortal.Models;

namespace ToolboxPortal.Services;

public class ThgInboxImportService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ThgInboxImportService> _logger;

    public ThgInboxImportService(
        IServiceScopeFactory scopeFactory,
        ILogger<ThgInboxImportService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task CheckInboxAsync()
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var csvImportService = scope.ServiceProvider.GetRequiredService<ThgCsvImportService>();

        var settings = await db.ThgInboxSettings
            .FirstOrDefaultAsync();

        if (settings == null || !settings.IsEnabled)
        {
            return;
        }

        var followUpSettings = await db.ThgFollowUpSettings
    .FirstOrDefaultAsync(x => x.UserId == settings.UserId);

        if (followUpSettings?.AutomationPaused == true)
        {
            _logger.LogInformation(
                "THG Postfachimport für User {UserId} ist pausiert.",
                settings.UserId);

            return;
        }

        try
        {
            await CheckInboxInternalAsync(db, csvImportService, settings);
        }
        catch (Exception ex)
        {
            settings.LastCheckUtc = DateTime.UtcNow;
            settings.LastError = ex.Message;

            await db.SaveChangesAsync();

            _logger.LogError(ex, "Fehler beim THG-Postfachimport");
        }
    }

    private async Task CheckInboxInternalAsync(
        ApplicationDbContext db,
        ThgCsvImportService csvImportService,
        ThgInboxSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.ImapHost)
            || string.IsNullOrWhiteSpace(settings.Username)
            || string.IsNullOrWhiteSpace(settings.Password))
        {
            settings.LastCheckUtc = DateTime.UtcNow;
            settings.LastError = "IMAP-Konfiguration ist unvollständig.";

            await db.SaveChangesAsync();

            return;
        }

        using var client = new ImapClient();

        await client.ConnectAsync(
            settings.ImapHost,
            settings.ImapPort,
            settings.UseSsl);

        await client.AuthenticateAsync(
            settings.Username,
            settings.Password);

        var inbox = client.Inbox;

        await inbox.OpenAsync(MailKit.FolderAccess.ReadWrite);

        var results = await inbox.SearchAsync(SearchQuery.NotSeen);

        var imported = 0;
        var skipped = 0;
        var attachmentsFound = 0;

        foreach (var uid in results)
        {
            var message = await inbox.GetMessageAsync(uid);

            if (!string.IsNullOrWhiteSpace(settings.AllowedSender))
            {
                var sender = message.From.Mailboxes.FirstOrDefault()?.Address;

                if (!string.Equals(
                    sender,
                    settings.AllowedSender,
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
            }

            foreach (var attachment in message.Attachments)
            {
                if (attachment is not MimePart part)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(part.FileName)
                    || !part.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                attachmentsFound++;

                using var stream = new MemoryStream();

                await part.Content.DecodeToAsync(stream);

                stream.Position = 0;

                var importedCustomers = await csvImportService.ParseAsync(
                    stream,
                    settings.UserId);

                foreach (var customer in importedCustomers)
                {
                    var exists = await db.ThgCustomers
                        .AnyAsync(x => x.UserId == settings.UserId && x.Vin == customer.Vin);

                    if (exists)
                    {
                        skipped++;
                        continue;
                    }

                    db.ThgCustomers.Add(customer);
                    imported++;
                }
            }

            await inbox.AddFlagsAsync(
                uid,
                MailKit.MessageFlags.Seen,
                true);
        }

        settings.LastSuccessUtc = DateTime.UtcNow;
        settings.LastCheckUtc = DateTime.UtcNow;
        settings.LastError = null;

        await db.SaveChangesAsync();

        await client.DisconnectAsync(true);

        _logger.LogInformation(
            "THG-Postfachimport abgeschlossen: {Attachments} CSV-Anhänge, {Imported} importiert, {Skipped} Dubletten.",
            attachmentsFound,
            imported,
            skipped);
    }
}