namespace ToolboxPortal.Models;

public class LeadMailTemplate
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    public string Name { get; set; } = "";

    public string TemplateKey { get; set; } = "QualificationMail";
    // QualificationMail, FollowUp1, FollowUp2, SalesNotification

    public string Subject { get; set; } = "";

    public string HtmlBody { get; set; } = "";

    public string? TextBody { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}