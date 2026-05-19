namespace ToolboxPortal.Models;

public class LeadFormField
{
    public int Id { get; set; }

    public int LeadFormId { get; set; }

    public LeadForm? LeadForm { get; set; }

    public string Label { get; set; } = "";

    public string FieldKey { get; set; } = "";

    public string FieldType { get; set; } = "Text";
    // Text, Textarea, Email, Phone, Select, Checkbox, Date, Number

    public string? Placeholder { get; set; }

    public string? OptionsJson { get; set; }

    public bool IsRequired { get; set; }

    public int SortOrder { get; set; }

    public string? MapsToLeadProperty { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}