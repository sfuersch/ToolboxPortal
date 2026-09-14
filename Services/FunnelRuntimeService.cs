using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ToolboxPortal.Data;
using ToolboxPortal.Models;
namespace ToolboxPortal.Services;

// One instance per public Blazor circuit; no dependency on admin/tenant selection.
public class FunnelRuntimeService(IServiceScopeFactory scopes, IConfiguration configuration)
{
    public Funnel? Funnel { get; private set; }
    public Recipient? Recipient { get; private set; }
    public int Position { get; private set; }
    public bool Complete { get; private set; }
    public FunnelStep? Step => Funnel?.Steps.ElementAtOrDefault(Position);
    public static string[] Options(FunnelStep step) => step.Options.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).Distinct().ToArray();
    public static string NormalizeHost(string host) => host.TrimEnd('.').ToLowerInvariant();
    public async Task<bool> OpenAsync(Uri uri, string? slug, string? publicId, string? first, string? last, string? company)
    {
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Funnel = null; Recipient = null; Position = 0; Complete = false;
        var host = NormalizeHost(uri.IdnHost);
        var mapping = await db.DomainMappings.AsNoTracking().SingleOrDefaultAsync(x => x.Host == host);
        var query = db.Funnels.AsNoTracking().Include(x => x.Steps)
            .Where(x => x.IsPublished && x.Tenant.IsActive && db.TenantModuleAccesses.Any(m => m.TenantId == x.TenantId && m.ModuleKey == "funnel-builder" && m.IsEnabled));
        if (mapping != null)
        {
            if (!mapping.IsEnabled) return false;
            query = query.Where(x => x.Id == mapping.FunnelId);
        }
        else
        {
            var allowed = configuration.GetSection("Funnels:RuntimeHosts").Get<string[]>() ?? [];
            if (!allowed.Any(x => NormalizeHost(x) == host) || string.IsNullOrEmpty(slug)) return false;
            query = query.Where(x => x.Slug == slug);
        }
        Funnel = await query.SingleOrDefaultAsync();
        if (Funnel == null || Funnel.Steps.Count == 0) return false;
        Funnel.Steps = Funnel.Steps.OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToList();
        Recipient? recipient = null;
        if (!string.IsNullOrEmpty(publicId))
        {
            if (!Guid.TryParse(publicId, out var token)) return false;
            recipient = await db.Recipients.Include(x => x.Lead).SingleOrDefaultAsync(x => x.PublicId == token && x.FunnelId == Funnel.Id);
            if (recipient == null) return false;
        }
        recipient ??= new Recipient { FunnelId = Funnel.Id, FirstName = Limit(first, 120), LastName = Limit(last, 120), Company = Limit(company, 200), Lead = new Lead() };
        if (recipient.Id == 0) db.Recipients.Add(recipient);
        recipient.OpenedAt ??= DateTime.UtcNow;
        recipient.LastSeenAt = DateTime.UtcNow;
        Recipient = recipient;
        Complete = recipient.CompletedAt != null;
        await RecordAsync(db, recipient, "open", null);
        if (!Complete) await RecordStepAsync(db, recipient, Step!);
        await db.SaveChangesAsync();
        return true;
    }
    public string Personalize(string value)
    {
        var values = new Dictionary<string, string> {
            ["Vorname"] = Recipient?.FirstName ?? "", ["Nachname"] = Recipient?.LastName ?? "",
            ["Firma"] = Recipient?.Company ?? "", ["EmpfaengerId"] = Recipient?.PublicId.ToString() ?? "",
            ["Empfänger-ID"] = Recipient?.PublicId.ToString() ?? ""
        };
        // Single pass: recipient text never becomes another template expression or HTML.
        return System.Text.RegularExpressions.Regex.Replace(value, @"\{\{([^{}]+)\}\}", m => values.GetValueOrDefault(m.Groups[1].Value, m.Value));
    }
    public async Task NextAsync(string answer, string first, string last, string company, string email, string phone)
    {
        if (Complete || Funnel == null || Recipient == null || Step == null) return;
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!await db.Funnels.AnyAsync(x => x.Id == Funnel.Id && x.UpdatedAt == Funnel.UpdatedAt && x.IsPublished && x.Tenant.IsActive &&
            db.TenantModuleAccesses.Any(m => m.TenantId == x.TenantId && m.ModuleKey == "funnel-builder" && m.IsEnabled)))
            throw new ValidationException("Dieser Funnel wurde geändert oder deaktiviert. Bitte die Seite neu laden.");
        var recipient = await db.Recipients.Include(x => x.Lead).SingleAsync(x => x.Id == Recipient.Id && x.FunnelId == Funnel.Id);
        if (recipient.CompletedAt != null) { Complete = true; return; }
        var lead = recipient.Lead ??= new Lead();
        if (Step.Type == FunnelStepType.Question)
        {
            if (!Options(Step).Contains(answer)) throw new ValidationException("Bitte eine der Antworten auswählen.");
            var answers = JsonSerializer.Deserialize<Dictionary<int, string>>(lead.AnswersJson) ?? [];
            answers[Step.Id] = answer;
            lead.AnswersJson = JsonSerializer.Serialize(answers);
        }
        if (Step.Type == FunnelStepType.Contact)
        {
            if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(last) || string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email) || email.Length > 254)
                throw new ValidationException("Bitte Vorname, Nachname und eine gültige E-Mail-Adresse eingeben.");
            recipient.FirstName = Limit(first, 120); recipient.LastName = Limit(last, 120); recipient.Company = Limit(company, 200);
            lead.Email = email.Trim(); lead.Phone = Limit(phone, 60);
        }
        recipient.LastSeenAt = lead.UpdatedAt = DateTime.UtcNow;
        var next = Position + 1;
        if (next >= Funnel.Steps.Count)
        {
            recipient.CompletedAt = DateTime.UtcNow;
            recipient.Status = "complete";
            await RecordAsync(db, recipient, "complete", null);
        }
        else await RecordStepAsync(db, recipient, Funnel.Steps[next]);
        await db.SaveChangesAsync();
        Position = next; Recipient = recipient; Complete = recipient.CompletedAt != null;
    }
    private static string Limit(string? value, int max) => (value ?? "").Trim()[..Math.Min((value ?? "").Trim().Length, max)];
    private static async Task RecordStepAsync(ApplicationDbContext db, Recipient recipient, FunnelStep step)
    {
        await RecordAsync(db, recipient, "step", step.Id);
        var type = step.Type == FunnelStepType.Reveal ? "reveal" : step.Type == FunnelStepType.Question ? "questions" : "step";
        if (type != "step") await RecordAsync(db, recipient, type, step.Id);
        recipient.Status = type;
    }
    private static async Task RecordAsync(ApplicationDbContext db, Recipient recipient, string type, int? step)
    {
        if (recipient.Id != 0 && await db.FunnelEvents.AnyAsync(x => x.RecipientId == recipient.Id && x.Type == type && x.FunnelStepId == step)) return;
        db.FunnelEvents.Add(new FunnelEvent { Recipient = recipient, Type = type, FunnelStepId = step });
    }
}
