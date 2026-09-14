using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ToolboxPortal.Data;
using ToolboxPortal.Models;
using ToolboxPortal.Services;

using var connection = new SqliteConnection("Data Source=:memory:");
connection.Open();
var services = new ServiceCollection();
services.AddLogging();
services.Configure<Microsoft.AspNetCore.Identity.IdentityOptions>(o => o.Stores.SchemaVersion = Microsoft.AspNetCore.Identity.IdentitySchemaVersions.Version3);
services.AddDbContext<ApplicationDbContext>(o => o.UseSqlite(connection));
services.AddSingleton<AuthenticationStateProvider, TestAuthentication>();
services.AddScoped<CurrentTenantService>();
services.AddScoped<ModuleAccessService>();
services.AddScoped<FunnelAdminService>();
services.AddScoped<FunnelRuntimeService>();
services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> {
    ["Funnels:RuntimeHosts:0"] = "localhost", ["Funnels:PublicOrigin"] = "http://localhost:5000"
}).Build());
using var provider = services.BuildServiceProvider();
using var scope = provider.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
await db.Database.EnsureCreatedAsync();
var tenant = new Tenant { Name = "Test", Slug = "test" };
var other = new Tenant { Name = "Other", Slug = "other" };
db.Tenants.AddRange(tenant, other);
await db.SaveChangesAsync();
db.TenantUsers.Add(new TenantUser { UserId = "test-user", TenantId = tenant.Id });
db.TenantModuleAccesses.Add(new TenantModuleAccess { TenantId = tenant.Id, ModuleKey = "funnel-builder", IsEnabled = true });
db.UserModuleAccesses.Add(new UserModuleAccess { UserId = "test-user", ModuleKey = "funnel-builder", IsEnabled = true });
await db.SaveChangesAsync();
var admin = scope.ServiceProvider.GetRequiredService<FunnelAdminService>();
var input = new Funnel { Name = "Pilot", Slug = "pilot", IsPublished = true, Steps = [
    new() { Title = "Hallo {{Vorname}}", Type = FunnelStepType.Text },
    new() { Title = "Reveal", Type = FunnelStepType.Reveal },
    new() { Title = "Frage", Type = FunnelStepType.Question, Options = "A\nB" },
    new() { Title = "Kontakt", Type = FunnelStepType.Contact }
], Domains = [new() { Host = "AKTION.example.com" }] };
var id = await admin.SaveAsync(input);
var saved = await admin.GetAsync(id) ?? throw new Exception("Missing funnel");
Check(saved.Domains.Single().Host == "aktion.example.com" && !saved.Domains.Single().IsEnabled, "Domain normalized and staged");
var foreign = new Funnel { TenantId = other.Id, Name = "Other", Slug = "other", Steps = [new() { Title = "Private" }] };
db.Funnels.Add(foreign); await db.SaveChangesAsync();
Check(await admin.GetAsync(foreign.Id) == null && (await admin.ListAsync()).Count == 1, "Tenant read isolation");
await Throws<UnauthorizedAccessException>(() => admin.SaveAsync(foreign), "Tenant write isolation");
await Throws<ValidationException>(() => admin.SaveAsync(new Funnel { Name = "Dup", Slug = "pilot" }), "Duplicate slug rejected");
var runtime = scope.ServiceProvider.GetRequiredService<FunnelRuntimeService>();
Check(!await runtime.OpenAsync(new Uri("https://unknown.example"), "pilot", null, null, null, null), "Unknown host denied");
Check(!await runtime.OpenAsync(new Uri("https://aktion.example.com"), null, null, null, null, null), "Inactive mapping denied");
Check(!await runtime.OpenAsync(new Uri("http://localhost"), "pilot", "guessable-id", null, null, null), "Invalid recipient token denied");
Check(await runtime.OpenAsync(new Uri("http://localhost"), "pilot", null, "Ada", "Lovelace", "Test GmbH"), "Public slug opens");
Check(runtime.Personalize("{{Vorname}} {{Nachname}} {{Firma}}") == "Ada Lovelace Test GmbH", "Personalization");
var token = runtime.Recipient!.PublicId.ToString();
await runtime.NextAsync("", "", "", "", "", "");
await runtime.NextAsync("", "", "", "", "", "");
await Throws<ValidationException>(() => runtime.NextAsync("C", "", "", "", "", ""), "Invalid option rejected");
await runtime.NextAsync("B", "", "", "", "", "");
await Throws<ValidationException>(() => runtime.NextAsync("", "Ada", "Lovelace", "Test", "bad", ""), "Invalid contact rejected");
await runtime.NextAsync("", "Ada", "Lovelace", "Test", "ada@example.com", "123");
await runtime.NextAsync("", "", "", "", "", "");
Check(runtime.Complete, "Completion");
Check(await db.Recipients.CountAsync() == 1 && await db.FunnelLeads.CountAsync() == 1, "One recipient and lead");
var lead = await db.FunnelLeads.AsNoTracking().SingleAsync();
Check(lead.Email == "ada@example.com" && lead.AnswersJson.Contains("B"), "Answers and contact persisted together");
Check(await db.FunnelEvents.CountAsync() == 8, "Open, four steps, reveal, questions, complete");
Check(await runtime.OpenAsync(new Uri("http://localhost"), "pilot", token, "Override", null, null) && runtime.Complete && runtime.Recipient!.FirstName == "Ada", "Token revisit preserves identity/completion");
Check(await db.FunnelEvents.CountAsync() == 8, "Revisit does not duplicate milestones");
var domain = await db.DomainMappings.SingleAsync(); domain.IsEnabled = true; await db.SaveChangesAsync();
Check(await runtime.OpenAsync(new Uri("https://aktion.example.com"), "other", token, null, null, null) && runtime.Funnel!.Id == id, "Host mapping overrides slug");
saved = (await admin.GetAsync(id))!; saved.IsPublished = false; await admin.SaveAsync(saved);
Check(!await runtime.OpenAsync(new Uri("http://localhost"), "pilot", token, null, null, null), "Unpublished funnel denied");
saved = (await admin.GetAsync(id))!;
saved.Steps.Reverse(); await admin.SaveAsync(saved);
Check((await admin.GetAsync(id))!.Steps.First().Type == FunnelStepType.Contact, "Step order persisted");
saved = (await admin.GetAsync(id))!; saved.Steps.Clear(); await admin.SaveAsync(saved);
Check(await db.FunnelEvents.CountAsync() == 8, "Deleting multiple steps preserves tracking history");
Check(await admin.PublicBaseAsync(id) == "https://aktion.example.com/funnel", "Active custom domain preferred for links");
domain.IsEnabled = false; await db.SaveChangesAsync();
Check(await admin.PublicBaseAsync(id) == "http://localhost:5000/funnel/pilot", "Configured runtime origin preserves port");
var preparedId = await admin.AddRecipientAsync(id, "Grace", "Hopper", "Example");
var prepared = (await admin.RecipientsAsync(id, 0)).Single(x => x.Id == preparedId);
Check(prepared.OpenedAt == null && prepared.LastSeenAt == null && prepared.Status == "pending", "Prepared recipient has no fabricated visit");
Check((await admin.EventsAsync(id, preparedId)).Count == 0, "Prepared recipient has no tracking events");
await Throws<UnauthorizedAccessException>(() => admin.AddRecipientAsync(foreign.Id, "X", "", ""), "Cannot create recipient for foreign tenant");
await Throws<UnauthorizedAccessException>(() => admin.EventsAsync(foreign.Id, preparedId), "Cannot fetch mismatched recipient details");
Check((await admin.RecipientsAsync(foreign.Id, 0)).Count == 0, "Recipient list is tenant scoped");
saved = (await admin.GetAsync(id))!; saved.Steps.Add(new FunnelStep { Title = "Hello" }); saved.IsPublished = true; await admin.SaveAsync(saved);
Check(await runtime.OpenAsync(new Uri("http://localhost"), "pilot", prepared.PublicId.ToString(), null, null, null), "Prepared recipient link opens");
Check(runtime.Recipient!.Id == preparedId && runtime.Recipient.OpenedAt != null && runtime.Personalize("{{Vorname}}") == "Grace", "Opening prepared link reuses recipient and records first visit");
for (var i = 0; i < 50; i++) await admin.AddRecipientAsync(id, "Page", i.ToString(), "");
Check((await admin.RecipientsAsync(id, 0)).Count == 51 && (await admin.RecipientsAsync(id, 1)).Count == 2, "Recipient pagination includes next-page sentinel");
var permission = await db.UserModuleAccesses.SingleAsync(); permission.IsEnabled = false; await db.SaveChangesAsync();
await Throws<UnauthorizedAccessException>(() => admin.ListAsync(), "Module permission enforced");
Console.WriteLine("All funnel smoke checks passed.");
static void Check(bool value, string name) { if (!value) throw new Exception(name); Console.WriteLine("PASS " + name); }
static async Task Throws<T>(Func<Task> run, string name) where T : Exception
{ try { await run(); } catch (T) { Console.WriteLine("PASS " + name); return; } throw new Exception(name); }
sealed class TestAuthentication : AuthenticationStateProvider
{
    public override Task<AuthenticationState> GetAuthenticationStateAsync() => Task.FromResult(new AuthenticationState(
        new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "test-user")], "test"))));
}
