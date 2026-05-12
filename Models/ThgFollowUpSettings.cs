using System.ComponentModel.DataAnnotations;

namespace ToolboxPortal.Models;

public class ThgFollowUpSettings
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = "";

    public TimeSpan MailSendWindowStart { get; set; } = new(8, 0, 0);

    public TimeSpan MailSendWindowEnd { get; set; } = new(18, 0, 0);

    public bool AutomationPaused { get; set; }

    public bool FollowUpsEnabled { get; set; } = true;

    public int FollowUp1AfterDays { get; set; } = 3;

    public int FollowUp2AfterDays { get; set; } = 7;

    public int FollowUp3AfterDays { get; set; } = 14;

    public int MaxFollowUps { get; set; } = 3;

    public string NotificationEmail { get; set; } = "";

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool AutoRegistrationCheckEnabled { get; set; } = true;

    public int BackgroundRunEveryHours { get; set; } = 12;

    public int MaxMailsPerRun { get; set; } = 50;

    public DateTime? LastAutomationRunAt { get; set; }
}