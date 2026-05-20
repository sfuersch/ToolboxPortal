using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ToolboxPortal.Data;
using ToolboxPortal.Models;

namespace ToolboxPortal.Services;

public class CurrentTenantService
{
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IServiceScopeFactory _scopeFactory;

    public CurrentTenantService(
        AuthenticationStateProvider authenticationStateProvider,
        IServiceScopeFactory scopeFactory)
    {
        _authenticationStateProvider = authenticationStateProvider;
        _scopeFactory = scopeFactory;
    }

    public async Task<string?> GetCurrentUserIdAsync()
    {
        var authState =
            await _authenticationStateProvider.GetAuthenticationStateAsync();

        return authState.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    public async Task<bool> IsSuperAdminAsync()
    {
        var authState =
            await _authenticationStateProvider.GetAuthenticationStateAsync();

        return authState.User.IsInRole("SuperAdmin");
    }

    public async Task<Tenant?> GetCurrentTenantAsync()
    {
        var userId = await GetCurrentUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var isSuperAdmin = await IsSuperAdminAsync();

        if (isSuperAdmin)
        {
            var selectedTenant = await db.UserTenantSelections
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Select(x => x.Tenant)
                .FirstOrDefaultAsync(x => x != null && x.IsActive);

            if (selectedTenant != null)
            {
                return selectedTenant;
            }

            return await db.Tenants
                .AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.Name)
                .FirstOrDefaultAsync();
        }

        return await db.TenantUsers
            .AsNoTracking()
            .Where(x =>
                x.UserId == userId &&
                x.IsActive &&
                x.Tenant != null &&
                x.Tenant.IsActive)
            .Select(x => x.Tenant)
            .FirstOrDefaultAsync();
    }

    public async Task SetCurrentTenantAsync(int tenantId)
    {
        var userId = await GetCurrentUserIdAsync();

        if (string.IsNullOrWhiteSpace(userId))
        {
            return;
        }

        if (!await IsSuperAdminAsync())
        {
            return;
        }

        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var tenantExists = await db.Tenants
            .AnyAsync(x => x.Id == tenantId && x.IsActive);

        if (!tenantExists)
        {
            return;
        }

        var selection = await db.UserTenantSelections
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (selection == null)
        {
            selection = new UserTenantSelection
            {
                UserId = userId,
                TenantId = tenantId,
                UpdatedAt = DateTime.UtcNow
            };

            db.UserTenantSelections.Add(selection);
        }
        else
        {
            selection.TenantId = tenantId;
            selection.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    public async Task<int?> GetCurrentTenantIdAsync()
    {
        var tenant = await GetCurrentTenantAsync();

        return tenant?.Id;
    }
}