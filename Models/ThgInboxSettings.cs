namespace ToolboxPortal.Models;

public class ThgInboxSettings
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    public string? ImapHost { get; set; }

    public int ImapPort { get; set; } = 993;

    public bool UseSsl { get; set; } = true;

    public string? Username { get; set; }

    public string? Password { get; set; }

    public string? AllowedSender { get; set; }

    public bool IsEnabled { get; set; }

    public DateTime? LastCheckUtc { get; set; }

    public DateTime? LastSuccessUtc { get; set; }

    public string? LastError { get; set; }
}