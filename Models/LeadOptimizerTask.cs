namespace ToolboxPortal.Models;

public class LeadOptimizerTask
{
    public int Id { get; set; }

    public int LeadId { get; set; }

    public LeadOptimizerLead? Lead { get; set; }

    public string Title { get; set; } = "";

    public string? Description { get; set; }

    public string Priority { get; set; } = "Medium";

    public DateTime DueAt { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string? AssignedUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}