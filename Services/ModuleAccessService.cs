using Microsoft.EntityFrameworkCore;
using ToolboxPortal.Data;

namespace ToolboxPortal.Services;

public class ModuleAccessService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly CurrentTenantService _currentTenantService;

    public ModuleAccessService(
        IServiceScopeFactory scopeFactory,
        CurrentTenantService currentTenantService)
    {
        _scopeFactory = scopeFactory;
        _currentTenantService = currentTenantService;
    }

    public async Task<bool> HasAccessAsync(
        string userId,
        string moduleKey)
    {
        var tenantId =
            await _currentTenantService.GetCurrentTenantIdAsync();

        if (tenantId == null)
        {
            return false;
        }

        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var tenantAccess =
            await db.TenantModuleAccesses
                .AsNoTracking()
                .AnyAsync(x =>
                    x.TenantId == tenantId
                    && x.ModuleKey == moduleKey
                    && x.IsEnabled);

        if (!tenantAccess)
        {
            return false;
        }

        var userAccess =
            await db.UserModuleAccesses
                .AsNoTracking()
                .AnyAsync(x =>
                    x.UserId == userId
                    && x.ModuleKey == moduleKey
                    && x.IsEnabled);

        return userAccess;
    }

    public async Task<HashSet<string>> GetAccessesForUserAsync(
        string userId)
    {
        var tenantId =
            await _currentTenantService.GetCurrentTenantIdAsync();

        if (tenantId == null)
        {
            return new HashSet<string>();
        }

        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var moduleKeys = await db.UserModuleAccesses
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId
                && x.IsEnabled
                && db.TenantModuleAccesses.Any(t =>
                    t.TenantId == tenantId
                    && t.ModuleKey == x.ModuleKey
                    && t.IsEnabled))
            .Select(x => x.ModuleKey)
            .ToListAsync();

        return moduleKeys.ToHashSet();
    }
}