namespace ToolboxPortal.Models;

public class AutomationJob
{
    public int Id { get; set; }

    public string JobType { get; set; } = "";

    public int LeadId { get; set; }

    public string Payload { get; set; } = "";

    public string Status { get; set; } = "Pending";

    public int RetryCount { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime? StartedAt { get; set; }

    public DateTime? FinishedAt { get; set; }
}