using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ToolboxPortal.Data;

namespace ToolboxPortal.Services;

public class TenantRoleService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AuthenticationStateService _authenticationStateService;
    private readonly CurrentTenantService _currentTenantService;

    public TenantRoleService(
        IServiceScopeFactory scopeFactory,
        AuthenticationStateService authenticationStateService,
        CurrentTenantService currentTenantService)
    {
        _scopeFactory = scopeFactory;
        _authenticationStateService = authenticationStateService;
        _currentTenantService = currentTenantService;
    }

    public async Task<bool> IsTenantOwnerAsync()
    {
        return await HasRoleAsync("Owner");
    }

    public async Task<bool> IsTenantAdminAsync()
    {
        return await HasAnyRoleAsync(
            "Owner",
            "Admin");
    }

    public async Task<bool> IsTenantSalesAsync()
    {
        return await HasAnyRoleAsync(
            "Owner",
            "Admin",
            "Sales");
    }

    public async Task<bool> HasRoleAsync(string role)
    {
        var userId =
            await _authenticationStateService.GetCurrentUserIdAsync();

        var tenantId =
            await _currentTenantService.GetCurrentTenantIdAsync();

        if (string.IsNullOrWhiteSpace(userId)
            || tenantId == null)
        {
            return false;
        }

        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        return await db.TenantUsers
            .AsNoTracking()
            .AnyAsync(x =>
                x.UserId == userId
                && x.TenantId == tenantId
                && x.Role == role
                && x.IsActive);
    }

    public async Task<bool> HasAnyRoleAsync(
        params string[] roles)
    {
        var userId =
            await _authenticationStateService.GetCurrentUserIdAsync();

        var tenantId =
            await _currentTenantService.GetCurrentTenantIdAsync();

        if (string.IsNullOrWhiteSpace(userId)
            || tenantId == null)
        {
            return false;
        }

        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        return await db.TenantUsers
            .AsNoTracking()
            .AnyAsync(x =>
                x.UserId == userId
                && x.TenantId == tenantId
                && roles.Contains(x.Role)
                && x.IsActive);
    }
}