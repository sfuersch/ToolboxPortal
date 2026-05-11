using System.ComponentModel.DataAnnotations;

namespace ToolboxPortal.Models;

public class ThgCustomer
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = "";

    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public string Company { get; set; } = "";

    public string Email { get; set; } = "";

    public string EmailSource { get; set; } = "";

    public string LicensePlate { get; set; } = "";

    public string Vin { get; set; } = "";

    public DateTime? FirstRegistrationDate { get; set; }

    public bool IsRegistered { get; set; }

    public int FollowUpCount { get; set; }

    public bool InitialMailSent { get; set; }

    public DateTime? InitialMailSentAt { get; set; }

    public DateTime? LastMailSentAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? FollowUp1SentAt { get; set; }

    public DateTime? FollowUp2SentAt { get; set; }

    public DateTime? FollowUp3SentAt { get; set; }
}