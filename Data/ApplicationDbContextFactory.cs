using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Design;

namespace ToolboxPortal.Data;

// EF tooling must not run Program.cs (startup migration, mail workers and role seeding).
public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Host=localhost;Database=toolbox_design;Username=toolbox";
        var services = new ServiceCollection();
        services.Configure<IdentityOptions>(options => options.Stores.SchemaVersion = IdentitySchemaVersions.Version3);
        var provider = services.BuildServiceProvider();
        return new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseNpgsql(connection).UseApplicationServiceProvider(provider).Options);
    }
}
