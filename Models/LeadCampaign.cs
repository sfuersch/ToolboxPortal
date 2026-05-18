namespace ToolboxPortal.Models;

public class LeadCampaign
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    public int LeadSourceId { get; set; }

    public LeadSource? LeadSource { get; set; }

    public string Name { get; set; } = "";

    public string? UtmSource { get; set; }

    public string? UtmMedium { get; set; }

    public string? UtmCampaign { get; set; }

    public string? VehicleFocus { get; set; }

    public string? DefaultSalesUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}