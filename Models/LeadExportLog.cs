namespace ToolboxPortal.Models;

public class LeadExportLog
{
    public int Id { get; set; }

    public int LeadId { get; set; }

    public int ExportTargetId { get; set; }

    public bool Success { get; set; }

    public string ExportType { get; set; } = "";

    public string? RequestPayload { get; set; }

    public string? ResponseMessage { get; set; }

    public DateTime CreatedAt { get; set; } =
        DateTime.UtcNow;
}