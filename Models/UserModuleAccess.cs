namespace ToolboxPortal.Models;

public class UserModuleAccess
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    public string ModuleKey { get; set; } = "";

    public bool IsEnabled { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}