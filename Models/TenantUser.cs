namespace ToolboxPortal.Models;

public class TenantUser
{
    public int Id { get; set; }

    public int TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    public string UserId { get; set; } = "";

    public string Role { get; set; } = "Viewer";
    // Owner, Admin, Sales, Marketing, Viewer

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}