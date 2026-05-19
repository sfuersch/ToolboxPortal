using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text;
using System.Xml.Linq;
using Microsoft.EntityFrameworkCore;
using ToolboxPortal.Data;
using ToolboxPortal.Models;

namespace ToolboxPortal.Services.LeadOptimizer;

public class CatchLeadExportService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public CatchLeadExportService(
        IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task ExportLeadAsync(
        int leadId,
        int exportTargetId)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var lead = await db.LeadOptimizerLeads
            .FirstOrDefaultAsync(x => x.Id == leadId);

        if (lead == null)
        {
            throw new Exception("Lead nicht gefunden.");
        }

        var target = await db.LeadExportTargets
            .FirstOrDefaultAsync(x =>
                x.Id == exportTargetId
                && x.IsActive);

        if (target == null)
        {
            throw new Exception("Exportziel nicht gefunden.");
        }

        var mailSettings = await db.LeadEmailSettings
            .FirstOrDefaultAsync(x =>
                x.UserId == lead.UserId);

        if (mailSettings == null)
        {
            throw new Exception("SMTP Einstellungen fehlen.");
        }

        var xml = BuildXml(
            lead,
            target);

        try
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(
                mailSettings.SenderName,
                mailSettings.SenderEmail));

            message.To.Add(MailboxAddress.Parse(target.CatchEmailAddress));

            message.Subject = "CATCH LEADS XML API";

            message.Body = new TextPart("plain")
            {
                Text = xml
            };

            using var smtp = new SmtpClient();

            var secureSocketOptions =
                mailSettings.SmtpPort == 465 || mailSettings.SmtpPort == 4465
                    ? SecureSocketOptions.SslOnConnect
                    : mailSettings.UseSsl
                        ? SecureSocketOptions.StartTls
                        : SecureSocketOptions.None;

            await smtp.ConnectAsync(
                mailSettings.SmtpHost,
                mailSettings.SmtpPort,
                secureSocketOptions);

            await smtp.AuthenticateAsync(
                mailSettings.SmtpUsername,
                mailSettings.SmtpPassword);

            await smtp.SendAsync(message);

            await smtp.DisconnectAsync(true);

            db.LeadExportLogs.Add(
                new LeadExportLog
                {
                    LeadId = lead.Id,
                    ExportTargetId = target.Id,
                    Success = true,
                    ExportType = "Catch",
                    RequestPayload = xml,
                    ResponseMessage = "Export erfolgreich"
                });

            db.LeadOptimizerLeadEvents.Add(
                new LeadOptimizerLeadEvent
                {
                    LeadId = lead.Id,
                    EventType = "catch_export_success",
                    Title = "CATCH Export erfolgreich",
                    Description =
                        $"Exportiert an {target.CatchEmailAddress}",
                    CreatedAt = DateTime.UtcNow
                });

            lead.ExportedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            db.LeadExportLogs.Add(
                new LeadExportLog
                {
                    LeadId = lead.Id,
                    ExportTargetId = target.Id,
                    Success = false,
                    ExportType = "Catch",
                    RequestPayload = xml,
                    ResponseMessage = ex.Message
                });

            db.LeadOptimizerLeadEvents.Add(
                new LeadOptimizerLeadEvent
                {
                    LeadId = lead.Id,
                    EventType = "catch_export_failed",
                    Title = "CATCH Export fehlgeschlagen",
                    Description = ex.Message,
                    CreatedAt = DateTime.UtcNow
                });

            await db.SaveChangesAsync();

            throw;
        }
    }

    private string BuildXml(
    LeadOptimizerLead lead,
    LeadExportTarget target)
    {
        var xml =
            new XDocument(
                new XDeclaration(
                    "1.0",
                    "UTF-8",
                    null),

                new XElement("lead",

                    new XElement("vehicle",

                        new XElement(
                            "internalId",
                            lead.ExternalLeadId ?? ""),

                        new XElement(
                            "make",
                            lead.VehicleMake ?? ""),

                        new XElement(
                            "model",
                            lead.VehicleModel ?? ""),

                        new XElement(
                            "firstRegistration",
                            lead.VehicleFirstRegistration ?? ""),

                        new XElement(
                            "mileage",
                            lead.VehicleMileage ?? 0),

                        new XElement(
                            "price",
                            lead.VehiclePrice ?? 0),

                        new XElement(
                            "conditionType",
                            string.IsNullOrWhiteSpace(lead.VehicleConditionType)
                                ? ""
                                : lead.VehicleConditionType.ToLower()),

                        new XElement(
                            "type",
                            "PKW"),

                        new XElement(
                            "vin",
                            lead.VehicleVin ?? "")
                    ),

                    new XElement("potentialBuyer",

                        new XElement(
                            "company",
                            lead.Company ?? ""),

                        new XElement(
                            "salutation",
                            lead.Salutation ?? ""),

                        new XElement(
                            "title",
                            ""),

                        new XElement(
                            "firstname",
                            lead.CustomerFirstName ?? ""),

                        new XElement(
                            "lastname",
                            lead.CustomerLastName ?? ""),

                        new XElement(
                            "street",
                            lead.Street ?? ""),

                        new XElement(
                            "zip",
                            lead.Zip ?? ""),

                        new XElement(
                            "city",
                            lead.City ?? ""),

                        new XElement(
                            "email",
                            lead.CustomerEmail ?? ""),

                        new XElement(
                            "additionalData",
                            BuildAdditionalData(lead)),

                        new XElement(
                            "message",
                            lead.OriginalMessage ?? ""),

                        new XElement("add_fields",
                            BuildAddFields(lead))
                    ),

                    new XElement(
                        "subject",
                        "Lead Anfrage"),

                    new XElement(
                        "crm_dealercode",
                        target.DealerCode ?? ""),

                    new XElement(
                        "crm_dealerid",
                        ""),

                    new XElement(
                        "dms_dealercode",
                        ""),

                    new XElement(
                        "locationString",
                        ""),

                    new XElement(
                        "campaign_name",
                        target.CampaignName ?? ""),

                    new XElement(
                        "lead_source",
                        target.LeadSource ?? ""),

                    new XElement(
                        "lead_source2",
                        ""),

                    new XElement(
                        "lead_type",
                        "Online Lead"),

                    new XElement(
                        "lead_channel",
                        target.LeadChannel ?? ""),

                    new XElement(
                        "marketing_campaign",
                        target.CampaignName ?? ""),

                    new XElement("files")
                )
            );

        return xml.ToString();
    }

    private object[] BuildAddFields(
    LeadOptimizerLead lead)
    {
        var fields = new List<object>();

        void AddField(string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            fields.Add(
                new XElement("field",
                    new XElement("key", key),
                    new XElement("value", value)));
        }

        AddField("Zahlungsart", lead.PaymentType);
        AddField("Kaufzeitpunkt", lead.PurchaseTimeframe);
        AddField("Kontaktweg", lead.ContactPreference);
        AddField("Inzahlungnahme Fahrzeug", lead.TradeInVehicle);
        AddField("Qualifizierungsnotiz", lead.QualificationNotes);
        AddField("UTM Source", lead.UtmSource);
        AddField("UTM Medium", lead.UtmMedium);
        AddField("UTM Campaign", lead.UtmCampaign);
        AddField("Landing Page", lead.LandingPageUrl);
        AddField("Referrer", lead.ReferrerUrl);

        if (lead.WantsTradeIn)
        {
            AddField("Inzahlungnahme", "Ja");
        }

        return fields.ToArray();
    }

    private string BuildAdditionalData(
        LeadOptimizerLead lead)
    {
        var sb = new StringBuilder();

        if (!string.IsNullOrWhiteSpace(
            lead.PaymentType))
        {
            sb.AppendLine(
                $"Zahlungsart: {lead.PaymentType}");
        }

        if (!string.IsNullOrWhiteSpace(
            lead.PurchaseTimeframe))
        {
            sb.AppendLine(
                $"Kaufzeitpunkt: {lead.PurchaseTimeframe}");
        }

        if (!string.IsNullOrWhiteSpace(
            lead.ContactPreference))
        {
            sb.AppendLine(
                $"Kontaktweg: {lead.ContactPreference}");
        }

        if (lead.WantsTradeIn)
        {
            sb.AppendLine(
                $"Inzahlungnahme: Ja");
        }

        if (!string.IsNullOrWhiteSpace(
            lead.TradeInVehicle))
        {
            sb.AppendLine(
                $"Inzahlungnahme Fahrzeug: {lead.TradeInVehicle}");
        }

        return sb.ToString();
    }
}