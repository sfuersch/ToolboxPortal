using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ToolboxPortal.Models;

namespace ToolboxPortal.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<AiGeneration> AiGenerations { get; set; }

        public DbSet<UserModuleAccess> UserModuleAccesses { get; set; }

        public DbSet<AiChatSession> AiChatSessions { get; set; }

        public DbSet<AiChatMessage> AiChatMessages { get; set; }

        public DbSet<ThgCustomer> ThgCustomers { get; set; }

        public DbSet<ThgMailLog> ThgMailLogs { get; set; }

        public DbSet<ThgMailTemplate> ThgMailTemplates { get; set; }

        public DbSet<ThgFollowUpSettings> ThgFollowUpSettings { get; set; }

        public DbSet<ThgInboxSettings> ThgInboxSettings { get; set; }

        public DbSet<LeadSource> LeadSources => Set<LeadSource>();

        public DbSet<LeadCampaign> LeadCampaigns => Set<LeadCampaign>();

        public DbSet<LeadOptimizerLead> LeadOptimizerLeads => Set<LeadOptimizerLead>();

        public DbSet<LeadOptimizerLeadEvent> LeadOptimizerLeadEvents => Set<LeadOptimizerLeadEvent>();

        public DbSet<LeadOptimizerTask> LeadOptimizerTasks => Set<LeadOptimizerTask>();

        public DbSet<LeadAutomationRule> LeadAutomationRules => Set<LeadAutomationRule>();

        public DbSet<LeadEmailSettings> LeadEmailSettings => Set<LeadEmailSettings>();

        public DbSet<LeadMailTemplate> LeadMailTemplates => Set<LeadMailTemplate>();

        public DbSet<LeadForm> LeadForms => Set<LeadForm>();
        public DbSet<LeadFormField> LeadFormFields => Set<LeadFormField>();

        public DbSet<LeadExportTarget> LeadExportTargets => Set<LeadExportTarget>();

        public DbSet<LeadExportLog> LeadExportLogs => Set<LeadExportLog>();

        public DbSet<KnowledgeBaseArticle> KnowledgeBaseArticles
    => Set<KnowledgeBaseArticle>();

        public DbSet<AutomationJob> AutomationJobs
    => Set<AutomationJob>();

        public DbSet<Tenant> Tenants => Set<Tenant>();

        public DbSet<TenantUser> TenantUsers => Set<TenantUser>();

        public DbSet<UserTenantSelection> UserTenantSelections
    => Set<UserTenantSelection>();

        public DbSet<TenantModuleAccess> TenantModuleAccesses
    => Set<TenantModuleAccess>();

        public DbSet<Funnel> Funnels => Set<Funnel>();
        public DbSet<FunnelStep> FunnelSteps => Set<FunnelStep>();
        public DbSet<DomainMapping> DomainMappings => Set<DomainMapping>();
        public DbSet<Recipient> Recipients => Set<Recipient>();
        public DbSet<FunnelEvent> FunnelEvents => Set<FunnelEvent>();
        public DbSet<Lead> FunnelLeads => Set<Lead>();
        public DbSet<VideoBinding> VideoBindings => Set<VideoBinding>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Funnel>().HasIndex(x => x.Slug).IsUnique();
            builder.Entity<DomainMapping>().HasIndex(x => x.Host).IsUnique();
            builder.Entity<Recipient>().Property(x => x.LastSeenAt).IsConcurrencyToken();
            builder.Entity<FunnelEvent>().HasIndex(x => new { x.RecipientId, x.Type }).IsUnique().HasFilter("\"FunnelStepId\" IS NULL AND \"Type\" IN ('open', 'complete')");
            builder.Entity<Recipient>().HasIndex(x => x.PublicId).IsUnique();
            builder.Entity<FunnelStep>().HasIndex(x => new { x.FunnelId, x.SortOrder });
            builder.Entity<FunnelEvent>().HasIndex(x => new { x.RecipientId, x.Type, x.FunnelStepId }).IsUnique();
            builder.Entity<FunnelEvent>().HasOne(x => x.FunnelStep).WithMany()
                .HasForeignKey(x => x.FunnelStepId).OnDelete(DeleteBehavior.SetNull);
            builder.Entity<Lead>().ToTable("FunnelLeads").HasOne(x => x.Recipient)
                .WithOne(x => x.Lead).HasForeignKey<Lead>(x => x.RecipientId);
            builder.Entity<VideoBinding>().HasIndex(x => x.FunnelStepId).IsUnique();

            builder.Entity<IdentityUserPasskey<string>>()
                .OwnsOne(x => x.Data, owned =>
                {
                    owned.ToJson("Data");
                });

            builder.Entity<ThgCustomer>()
                .Property(x => x.FirstRegistrationDate)
                .HasColumnType("date");
        }
    }
}