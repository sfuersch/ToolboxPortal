using ToolboxPortal.Models;

namespace ToolboxPortal.Services.LeadOptimizer;

public class LeadScoringService
{
    public void CalculateScore(LeadOptimizerLead lead)
    {
        var score = 0;

        if (!string.IsNullOrWhiteSpace(lead.CustomerPhone))
        {
            score += 20;
        }

        if (!string.IsNullOrWhiteSpace(lead.CustomerEmail))
        {
            score += 10;
        }

        if (!string.IsNullOrWhiteSpace(lead.VehicleTitle))
        {
            score += 10;
        }

        if (!string.IsNullOrWhiteSpace(lead.OriginalMessage))
        {
            score += 15;
        }

        if (lead.SourceType == "MobileDe")
        {
            score += 15;
        }

        if (lead.SourceType == "Facebook")
        {
            score += 10;
        }

        if (lead.SourceType == "WebForm")
        {
            score += 20;
        }

        lead.Score = score;

        if (score >= 70)
        {
            lead.ScoreLabel = "HOT";
        }
        else if (score >= 40)
        {
            lead.ScoreLabel = "WARM";
        }
        else
        {
            lead.ScoreLabel = "COLD";
        }
    }
}