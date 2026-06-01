using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using ToolboxPortal.Data;

namespace ToolboxPortal.Services;

public class TenantRoleService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly CurrentTenantService _currentTenantService;

    public TenantRoleService(
        IServiceScopeFactory scopeFactory,
        AuthenticationStateProvider authenticationStateProvider,
        CurrentTenantService currentTenantService)
    {
        _scopeFactory = scopeFactory;
        _authenticationStateProvider = authenticationStateProvider;
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
        var userId = await GetCurrentUserIdAsync();

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
        var userId = await GetCurrentUserIdAsync();

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

    private async Task<string?> GetCurrentUserIdAsync()
    {
        var authState =
            await _authenticationStateProvider.GetAuthenticationStateAsync();

        return authState.User.FindFirstValue(
            ClaimTypes.NameIdentifier);
    }
}