namespace ToolboxPortal.Models;

public class LeadAutomationRule
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    public string Name { get; set; } = "";

    public string TriggerEvent { get; set; } = "LeadQualified";
    // LeadCreated, LeadQualified, StatusChanged, TaskCompleted

    public string ActionType { get; set; } = "CreateTask";
    // CreateTask, später SendEmail, Webhook, AssignUser

    public string? TaskTitle { get; set; }

    public string? TaskDescription { get; set; }

    public string TaskPriority { get; set; } = "Medium";
    // High, Medium, Low

    public int DueOffsetMinutes { get; set; } = 15;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}