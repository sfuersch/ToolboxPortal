using System.ComponentModel.DataAnnotations;

namespace ToolboxPortal.Models;

public class ThgMailLog
{
    public int Id { get; set; }

    public int ThgCustomerId { get; set; }

    public ThgCustomer? Customer { get; set; }

    [Required]
    public string MailType { get; set; } = "";

    [Required]
    public string RecipientEmail { get; set; } = "";

    public string Subject { get; set; } = "";

    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public bool Success { get; set; }

    public string ErrorMessage { get; set; } = "";
}