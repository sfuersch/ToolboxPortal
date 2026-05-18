namespace ToolboxPortal.Models;

public class LeadInboundRequest
{
    public string? ApiKey { get; set; }

    public string SourceType { get; set; } = "ApiWebhook";

    public string? SourceName { get; set; }

    public string? CampaignName { get; set; }

    public string? ExternalLeadId { get; set; }

    public string? CustomerName { get; set; }

    public string? CustomerFirstName { get; set; }

    public string? CustomerLastName { get; set; }

    public string? CustomerEmail { get; set; }

    public string? CustomerPhone { get; set; }

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
}