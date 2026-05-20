namespace ToolboxPortal.Models;

public class TenantModuleAccess
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    public string ModuleKey { get; set; } = "";

    public bool IsEnabled { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}