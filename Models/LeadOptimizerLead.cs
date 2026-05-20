namespace ToolboxPortal.Models;

public class LeadOptimizerLead
{
    public int Id { get; set; }

    public int? TenantId { get; set; }

    public Tenant? Tenant { get; set; }

    public string UserId { get; set; } = "";

    public int? LeadSourceId { get; set; }

    public LeadSource? LeadSource { get; set; }

    public int? LeadCampaignId { get; set; }

    public LeadCampaign? LeadCampaign { get; set; }

    public string SourceType { get; set; } = "Manual";

    public string? ExternalLeadId { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerFirstName { get; set; }

    public string? CustomerLastName { get; set; }

    public string? CustomerEmail { get; set; }

    public string? CustomerPhone { get; set; }

    public string? PurchaseTimeframe { get; set; }

    public string? PaymentType { get; set; }

    public bool WantsTradeIn { get; set; }

    public string? TradeInVehicle { get; set; }

    public string? ContactPreference { get; set; }

    public string? QualificationNotes { get; set; }

    public string? VehicleMake { get; set; }

    public string? VehicleModel { get; set; }

    public string? VehicleTitle { get; set; }

    public string? VehicleUrl { get; set; }

    public decimal? VehiclePrice { get; set; }

    public string? OriginalMessage { get; set; }

    public string? UtmSource { get; set; }

    public string? UtmMedium { get; set; }

    public string? UtmCampaign { get; set; }

    public string? ReferrerUrl { get; set; }

    public string? LandingPageUrl { get; set; }

    public bool PrivacyAccepted { get; set; }

    public DateTime? PrivacyAcceptedAt { get; set; }

    public string Status { get; set; } = "New";
    // New, EmailSent, FormOpened, Qualified, Exported, Closed

    public string QualificationToken { get; set; } = Guid.NewGuid().ToString("N");

    public DateTime? QualificationEmailSentAt { get; set; }

    public DateTime? QualificationOpenedAt { get; set; }

    public DateTime? QualifiedAt { get; set; }

    public int Score { get; set; }

    public string ScoreLabel { get; set; } = "Cold";

    public string? AssignedSalesUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ExportedAt { get; set; }

    public ICollection<LeadOptimizerTask> Tasks { get; set; }
    = new List<LeadOptimizerTask>();

    public ICollection<LeadOptimizerLeadEvent> Events { get; set; }
    = new List<LeadOptimizerLeadEvent>();

    public string? Salutation { get; set; }

    public string? Company { get; set; }

    public string? Street { get; set; }

    public string? Zip { get; set; }

    public string? City { get; set; }

    public string? VehicleVin { get; set; }

    public int? VehicleMileage { get; set; }

    public string? VehicleFirstRegistration { get; set; }

    public string? VehicleConditionType { get; set; }
}