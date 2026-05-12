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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

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