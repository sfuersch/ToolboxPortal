namespace ToolboxPortal.Models;

public class LeadOptimizerLeadEvent
{
    public int Id { get; set; }

    public int LeadId { get; set; }

    public LeadOptimizerLead? Lead { get; set; }

    public string EventType { get; set; } = "";

    public string Title { get; set; } = "";

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}