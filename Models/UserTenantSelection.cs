namespace ToolboxPortal.Models;

public class UserTenantSelection
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    public int TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}