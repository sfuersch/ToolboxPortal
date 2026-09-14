using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ToolboxPortal.Data;
using ToolboxPortal.Models;
namespace ToolboxPortal.Services;

public class FunnelAdminService(IServiceScopeFactory scopes, CurrentTenantService tenants, ModuleAccessService access, IConfiguration configuration)
{
    public async Task<int> RequireTenantAsync()
    {
        var user = await tenants.GetCurrentUserIdAsync();
        var tenant = await tenants.GetCurrentTenantIdAsync();
        if (user == null || tenant == null || !await access.HasAccessAsync(user, "funnel-builder"))
            throw new UnauthorizedAccessException("Keine Freigabe für Funnel Builder im aktuellen Mandanten.");
        return tenant.Value;
    }
    public async Task<List<Funnel>> ListAsync()
    {
        var tenant = await RequireTenantAsync();
        using var scope = scopes.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Funnels
            .AsNoTracking().Where(x => x.TenantId == tenant).OrderByDescending(x => x.UpdatedAt).ToListAsync();
    }
    public async Task<Funnel?> GetAsync(int id)
    {
        var tenant = await RequireTenantAsync();
        using var scope = scopes.CreateScope();
        var funnel = await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Funnels
            .AsNoTracking().Include(x => x.Steps).Include(x => x.Domains)
            .SingleOrDefaultAsync(x => x.Id == id && x.TenantId == tenant);
        if (funnel != null) funnel.Steps = funnel.Steps.OrderBy(x => x.SortOrder).ThenBy(x => x.Id).ToList();
        return funnel;
    }
    public async Task<List<Recipient>> RecipientsAsync(int funnelId, int page)
    {
        var tenant = await RequireTenantAsync();
        using var scope = scopes.CreateScope();
        return await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Recipients
            .AsNoTracking().Include(x => x.Lead)
            .Where(x => x.FunnelId == funnelId && x.Funnel.TenantId == tenant)
            .OrderByDescending(x => x.Id).Skip(Math.Max(0, page) * 50).Take(51).ToListAsync();
    }
    public async Task<int> AddRecipientAsync(int funnelId, string first, string last, string company)
    {
        var tenant = await RequireTenantAsync();
        var recipient = new Recipient { FunnelId = funnelId, FirstName = first.Trim(), LastName = last.Trim(), Company = company.Trim(), Lead = new Lead() };
        Validator.ValidateObject(recipient, new ValidationContext(recipient), true);
        if (string.IsNullOrWhiteSpace(recipient.FirstName) && string.IsNullOrWhiteSpace(recipient.Company))
            throw new ValidationException("Bitte mindestens Vorname oder Firma angeben.");
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!await db.Funnels.AnyAsync(x => x.Id == funnelId && x.TenantId == tenant)) throw new UnauthorizedAccessException();
        db.Recipients.Add(recipient);
        await db.SaveChangesAsync();
        return recipient.Id;
    }
    public async Task<List<FunnelEvent>> EventsAsync(int funnelId, int recipientId)
    {
        var tenant = await RequireTenantAsync();
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (!await db.Recipients.AnyAsync(x => x.Id == recipientId && x.FunnelId == funnelId && x.Funnel.TenantId == tenant))
            throw new UnauthorizedAccessException();
        return await db.FunnelEvents
            .AsNoTracking().Include(x => x.FunnelStep)
            .Where(x => x.RecipientId == recipientId && x.Recipient.FunnelId == funnelId && x.Recipient.Funnel.TenantId == tenant)
            .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).ToListAsync();
    }
    public async Task<string?> PublicBaseAsync(int funnelId)
    {
        var funnel = await GetAsync(funnelId);
        if (funnel == null) return null;
        var domain = funnel.Domains.Where(x => x.IsEnabled).OrderBy(x => x.Host).FirstOrDefault();
        if (domain != null) return $"https://{domain.Host}/funnel";
        // Local development needs its actual port; production links always use explicit configuration.
        var origin = configuration["Funnels:PublicOrigin"];
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
            (uri.Scheme != "https" && !(uri.Scheme == "http" && uri.IsLoopback)) ||
            !string.IsNullOrEmpty(uri.UserInfo) || uri.AbsolutePath != "/" || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment)) return null;
        var hosts = configuration.GetSection("Funnels:RuntimeHosts").Get<string[]>() ?? [];
        if (!hosts.Any(x => FunnelRuntimeService.NormalizeHost(x) == FunnelRuntimeService.NormalizeHost(uri.IdnHost))) return null;
        return $"{uri.GetLeftPart(UriPartial.Authority)}/funnel/{funnel.Slug}";
    }
    public async Task<int> SaveAsync(Funnel input)
    {
        var tenant = await RequireTenantAsync();
        Validator.ValidateObject(input, new ValidationContext(input), true);
        if (input.Steps.Count > 30) throw new ValidationException("Maximal 30 Schritte.");
        foreach (var step in input.Steps)
        {
            Validator.ValidateObject(step, new ValidationContext(step), true);
            if (!Enum.IsDefined(step.Type)) throw new ValidationException("Ungültiger Schritttyp.");
            if (step.Type == FunnelStepType.Question && !FunnelRuntimeService.Options(step).Any())
                throw new ValidationException("Auswahlfragen benötigen mindestens eine Option.");
        }
        if (input.IsPublished && input.Steps.Count == 0) throw new ValidationException("Mindestens ein Schritt ist erforderlich.");
        using var scope = scopes.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        if (await db.Funnels.AnyAsync(x => x.Slug == input.Slug && x.Id != input.Id))
            throw new ValidationException("Dieser Slug wird bereits verwendet.");
        var saved = input.Id == 0 ? new Funnel { TenantId = tenant } :
            await db.Funnels.Include(x => x.Steps).Include(x => x.Domains)
                .SingleOrDefaultAsync(x => x.Id == input.Id && x.TenantId == tenant)
                ?? throw new UnauthorizedAccessException();
        if (input.Id == 0) db.Funnels.Add(saved);
        saved.Name = input.Name; saved.Slug = input.Slug; saved.Description = input.Description;
        saved.IsPublished = input.IsPublished; saved.UpdatedAt = DateTime.UtcNow;
        foreach (var removed in saved.Steps.Where(x => !input.Steps.Any(y => y.Id == x.Id)).ToList()) db.FunnelSteps.Remove(removed);
        for (var i = 0; i < input.Steps.Count; i++)
        {
            var item = input.Steps[i];
            var step = item.Id == 0 ? new FunnelStep() : saved.Steps.Single(x => x.Id == item.Id);
            if (item.Id == 0) saved.Steps.Add(step);
            step.Title = item.Title; step.Content = item.Content; step.Options = item.Options;
            step.Type = item.Type; step.SortOrder = i;
        }
        // Host claims are staged only; the operator enables a verified mapping separately.
        foreach (var removed in saved.Domains.Where(x => !input.Domains.Any(y => y.Id == x.Id)).ToList()) db.DomainMappings.Remove(removed);
        foreach (var item in input.Domains.Where(x => x.Id == 0))
        {
            var host = new IdnMapping().GetAscii(item.Host.Trim().TrimEnd('.')).ToLowerInvariant();
            if (Uri.CheckHostName(host) != UriHostNameType.Dns || host.Length > 253 || !host.Contains('.'))
                throw new ValidationException("Bitte einen Domainnamen ohne Protokoll und Pfad eingeben.");
            if (await db.DomainMappings.AnyAsync(x => x.Host == host) || saved.Domains.Any(x => x.Host == host))
                throw new ValidationException("Domain ist bereits hinterlegt.");
            saved.Domains.Add(new DomainMapping { Host = host });
        }
        await db.SaveChangesAsync();
        return saved.Id;
    }
}
