using ToolboxPortal.Data;
using ToolboxPortal.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ToolboxPortal.Services;

public class ThgMailTemplateService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public ThgMailTemplateService(
        ApplicationDbContext dbContext,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task EnsureDefaultTemplatesAsync(string userId)
    {
        var existingKeys = await _dbContext.ThgMailTemplates
            .Where(x => x.UserId == userId)
            .Select(x => x.TemplateKey)
            .ToListAsync();

        var defaults = GetDefaultTemplates(userId);

        foreach (var template in defaults)
        {
            if (existingKeys.Contains(template.TemplateKey))
            {
                continue;
            }

            _dbContext.ThgMailTemplates.Add(template);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<ThgMailTemplate?> GetTemplateAsync(string userId, string templateKey)
    {
        return await _dbContext.ThgMailTemplates
            .FirstOrDefaultAsync(x => x.UserId == userId && x.TemplateKey == templateKey);
    }

    public string RenderTemplate(string template, ThgCustomer customer)
    {
        template = RenderConditionals(template, customer);

        var registrationLink =
            _configuration["ThgSettings:RegistrationLink"]
            ?? "https://geld-fuer-eauto.de";

        return template
            .Replace("{{Anrede}}", customer.Salutation ?? "")
            .Replace("{{Vorname}}", customer.FirstName ?? "")
            .Replace("{{Nachname}}", customer.LastName ?? "")
            .Replace("{{Firma}}", customer.Company ?? "")
            .Replace("{{Email}}", customer.Email ?? "")
            .Replace("{{Kennzeichen}}", customer.LicensePlate ?? "")
            .Replace("{{VIN}}", customer.Vin ?? "")
            .Replace("{{Erstzulassung}}", customer.FirstRegistrationDate?.ToString("dd.MM.yyyy") ?? "")
            .Replace("{{THGLink}}", registrationLink);
    }

    private static string RenderConditionals(string template, ThgCustomer customer)
    {
        return Regex.Replace(
            template,
            @"\{\{#if\s+(.*?)\}\}(.*?)(?:\{\{#elseif\s+(.*?)\}\}(.*?))?(?:\{\{else\}\}(.*?))?\{\{/if\}\}",
            match =>
            {
                var ifCondition = match.Groups[1].Value;
                var ifContent = match.Groups[2].Value;

                var elseifCondition = match.Groups[3].Success
                    ? match.Groups[3].Value
                    : "";

                var elseifContent = match.Groups[4].Success
                    ? match.Groups[4].Value
                    : "";

                var elseContent = match.Groups[5].Success
                    ? match.Groups[5].Value
                    : "";

                if (EvaluateCondition(ifCondition, customer))
                {
                    return ifContent;
                }

                if (!string.IsNullOrWhiteSpace(elseifCondition)
                    && EvaluateCondition(elseifCondition, customer))
                {
                    return elseifContent;
                }

                return elseContent;
            },
            RegexOptions.Singleline);
    }

    private static bool EvaluateCondition(string condition, ThgCustomer customer)
    {
        var fields = condition
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return fields.All(field => !string.IsNullOrWhiteSpace(GetFieldValue(field, customer)));
    }

    private static string GetFieldValue(string field, ThgCustomer customer)
    {
        return field.ToLowerInvariant() switch
        {
            "anrede" => customer.Salutation ?? "",
            "vorname" => customer.FirstName ?? "",
            "nachname" => customer.LastName ?? "",
            "firma" => customer.Company ?? "",
            "email" => customer.Email ?? "",
            "kennzeichen" => customer.LicensePlate ?? "",
            "vin" => customer.Vin ?? "",
            "erstzulassung" => customer.FirstRegistrationDate?.ToString("dd.MM.yyyy") ?? "",
            _ => ""
        };
    }
    private static List<ThgMailTemplate> GetDefaultTemplates(string userId)
    {
        return new List<ThgMailTemplate>
        {
            new()
            {
                UserId = userId,
                TemplateKey = "Initial",
                Name = "Erstmail",
                Subject = "Jetzt THG-Prämie für Ihr E-Auto sichern",
                HtmlBody = """
                <h2>Hallo {{Vorname}},</h2>

                <p>
                    sichern Sie sich jetzt Ihre THG-Prämie für Ihr Elektrofahrzeug.
                </p>

                <p>
                    Die Registrierung dauert nur wenige Minuten.
                </p>

                <p>
                    <a href="{{THGLink}}"
                       style="display:inline-block;background:#10a37f;color:white;padding:12px 18px;border-radius:10px;text-decoration:none;font-weight:700;">
                        Jetzt THG-Prämie sichern
                    </a>
                </p>

                <p>
                    Viele Grüße<br>
                    Autohaus
                </p>
                """
            },
            new()
            {
                UserId = userId,
                TemplateKey = "FollowUp1",
                Name = "Follow-up 1",
                Subject = "Kurze Erinnerung: THG-Prämie noch sichern",
                HtmlBody = """
                <h2>Hallo {{Vorname}},</h2>

                <p>
                    wir wollten Sie kurz daran erinnern, dass Sie Ihre THG-Prämie für Ihr E-Auto noch beantragen können.
                </p>

                <p>
                    Hier geht es direkt zur Registrierung:
                </p>

                <p>
                    <a href="{{THGLink}}"
                       style="display:inline-block;background:#10a37f;color:white;padding:12px 18px;border-radius:10px;text-decoration:none;font-weight:700;">
                        THG-Prämie beantragen
                    </a>
                </p>

                <p>
                    Viele Grüße<br>
                    Autohaus
                </p>
                """
            },
            new()
            {
                UserId = userId,
                TemplateKey = "FollowUp2",
                Name = "Follow-up 2",
                Subject = "Ihre THG-Prämie wartet noch",
                HtmlBody = """
                <h2>Hallo {{Vorname}},</h2>

                <p>
                    falls Sie Ihre THG-Prämie noch nicht beantragt haben: Die Registrierung ist weiterhin möglich.
                </p>

                <p>
                    <a href="{{THGLink}}"
                       style="display:inline-block;background:#10a37f;color:white;padding:12px 18px;border-radius:10px;text-decoration:none;font-weight:700;">
                        Jetzt registrieren
                    </a>
                </p>

                <p>
                    Viele Grüße<br>
                    Autohaus
                </p>
                """
            },
            new()
            {
                UserId = userId,
                TemplateKey = "FollowUp3",
                Name = "Follow-up 3",
                Subject = "Letzte Erinnerung zur THG-Prämie",
                HtmlBody = """
                <h2>Hallo {{Vorname}},</h2>

                <p>
                    dies ist unsere letzte Erinnerung zur THG-Prämie für Ihr Elektrofahrzeug.
                </p>

                <p>
                    <a href="{{THGLink}}"
                       style="display:inline-block;background:#10a37f;color:white;padding:12px 18px;border-radius:10px;text-decoration:none;font-weight:700;">
                        THG-Prämie sichern
                    </a>
                </p>

                <p>
                    Viele Grüße<br>
                    Autohaus
                </p>
                """
            }
        };
    }
}