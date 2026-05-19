namespace ToolboxPortal.Models;

public class LeadExportTarget
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    public string Name { get; set; } = "";

    public string ExportType { get; set; } = "Catch";

    public bool IsActive { get; set; } = true;

    public bool ExportOnQualified { get; set; } = true;

    public string CatchEmailAddress { get; set; } = "";

    public string? CampaignName { get; set; }

    public string? LeadSource { get; set; }

    public string? LeadChannel { get; set; }

    public string? DealerCode { get; set; }

    public DateTime CreatedAt { get; set; } =
        DateTime.UtcNow;
}