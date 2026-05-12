using ToolboxPortal.Data;
using ToolboxPortal.Models;
using Microsoft.EntityFrameworkCore;

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