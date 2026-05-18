namespace ToolboxPortal.Models;

public class LeadEmailSettings
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    public string SmtpHost { get; set; } = "";

    public int SmtpPort { get; set; } = 587;

    public bool UseSsl { get; set; } = true;

    public string SmtpUsername { get; set; } = "";

    public string SmtpPassword { get; set; } = "";

    public string SenderName { get; set; } = "";

    public string SenderEmail { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}