namespace ToolboxPortal.Models;

public class LeadSource
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    public string Name { get; set; } = "";

    public string SourceType { get; set; } = "Manual";
    // MobileDe, WebForm, Facebook, Instagram, GoogleAds, Manual, Csv, ApiWebhook

    public string? ApiKey { get; set; }

    public string? WebhookSecret { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}