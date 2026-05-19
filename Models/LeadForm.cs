namespace ToolboxPortal.Models;

public class LeadForm
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    public string Name { get; set; } = "";

    public string FormKey { get; set; } = "QualificationForm";

    public string Headline { get; set; } = "Anfrage vervollständigen";

    public string IntroText { get; set; } = "Bitte ergänzen Sie noch einige Informationen für Ihr persönliches Angebot.";

    public string SuccessText { get; set; } = "Vielen Dank! Ihre Angaben wurden erfolgreich übermittelt.";

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<LeadFormField> Fields { get; set; } = new();
}