using Microsoft.EntityFrameworkCore;
using ToolboxPortal.Data;

namespace ToolboxPortal.Services;

public class ModuleAccessService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ModuleAccessService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task<bool> HasAccessAsync(string userId, string moduleKey)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await db.UserModuleAccesses
            .AsNoTracking()
            .AnyAsync(x =>
                x.UserId == userId
                && x.ModuleKey == moduleKey
                && x.IsEnabled);
    }

    public async Task<HashSet<string>> GetAccessesForUserAsync(string userId)
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var moduleKeys = await db.UserModuleAccesses
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.IsEnabled)
            .Select(x => x.ModuleKey)
            .ToListAsync();

        return moduleKeys.ToHashSet();
    }
}