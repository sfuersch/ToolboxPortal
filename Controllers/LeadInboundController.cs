using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToolboxPortal.Data;
using ToolboxPortal.Models;
using ToolboxPortal.Services;
using ToolboxPortal.Services.LeadOptimizer;

namespace ToolboxPortal.Controllers;

[ApiController]
[Route("api/leads")]



public class LeadInboundController : ControllerBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly LeadScoringService _leadScoringService;
    private readonly LeadAutomationService _leadAutomationService;
    private readonly CurrentTenantService _currentTenantService;

    public LeadInboundController(
        IServiceScopeFactory scopeFactory,
        LeadScoringService leadScoringService,
        LeadAutomationService leadAutomationService,
        CurrentTenantService currentTenantService)
    {
        _scopeFactory = scopeFactory;
        _leadScoringService = leadScoringService;
        _leadAutomationService = leadAutomationService;
        _currentTenantService = currentTenantService;
    }

    [HttpPost("inbound")]
    public async Task<IActionResult> CreateLead(
    [FromBody] LeadInboundRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ApiKey))
        {
            return Unauthorized("ApiKey fehlt.");
        }

        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var source = await db.LeadSources
            .FirstOrDefaultAsync(x =>
                x.ApiKey == request.ApiKey &&
                x.IsActive);

        if (source == null)
        {
            return Unauthorized("Ungültiger ApiKey.");
        }

        var lead = new LeadOptimizerLead
        {
            TenantId = source.TenantId,

            UserId = source.UserId,
            LeadSourceId = source.Id,
            SourceType = request.SourceType,
            ExternalLeadId = request.ExternalLeadId,

            CustomerName = request.CustomerName,
            CustomerFirstName = request.CustomerFirstName,
            CustomerLastName = request.CustomerLastName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,

            Salutation = request.Salutation,
            Company = request.Company,
            Street = request.Street,
            Zip = request.Zip,
            City = request.City,

            VehicleMake = request.VehicleMake,
            VehicleModel = request.VehicleModel,
            VehicleTitle = request.VehicleTitle,
            VehicleUrl = request.VehicleUrl,
            VehiclePrice = request.VehiclePrice,
            VehicleVin = request.VehicleVin,
            VehicleMileage = request.VehicleMileage,
            VehicleFirstRegistration = request.VehicleFirstRegistration,
            VehicleConditionType = request.VehicleConditionType,

            OriginalMessage = request.OriginalMessage,

            UtmSource = request.UtmSource,
            UtmMedium = request.UtmMedium,
            UtmCampaign = request.UtmCampaign,
            ReferrerUrl = request.ReferrerUrl,
            LandingPageUrl = request.LandingPageUrl,

            Status = "New",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _leadScoringService.CalculateScore(lead);

        db.LeadOptimizerLeads.Add(lead);

        await db.SaveChangesAsync();

        db.LeadOptimizerLeadEvents.Add(new LeadOptimizerLeadEvent
        {
            LeadId = lead.Id,
            EventType = "lead_created",
            Title = "Lead eingegangen",
            Description = $"Quelle: {source.Name}",
            CreatedAt = DateTime.UtcNow
        });

        db.AutomationJobs.Add(new AutomationJob
        {
            JobType = "LeadCreated",
            LeadId = lead.Id,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        return Ok(new
        {
            lead.Id,
            lead.QualificationToken,
            QualificationUrl =
                $"{Request.Scheme}://{Request.Host}/lead/q/{lead.QualificationToken}"
        });
    }
}