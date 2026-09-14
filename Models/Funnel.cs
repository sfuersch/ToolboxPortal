using System.ComponentModel.DataAnnotations;
namespace ToolboxPortal.Models;

public enum FunnelStepType { Text, Question, Contact, Reveal }
public class Funnel
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;
    [Required, MaxLength(160)] public string Name { get; set; } = "";
    [Required, MaxLength(100), RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    public string Slug { get; set; } = "";
    [MaxLength(2000)] public string Description { get; set; } = "";
    public bool IsPublished { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public List<FunnelStep> Steps { get; set; } = [];
    public List<DomainMapping> Domains { get; set; } = [];
}
public class FunnelStep
{
    public int Id { get; set; }
    public int FunnelId { get; set; }
    public Funnel Funnel { get; set; } = null!;
    public int SortOrder { get; set; }
    public FunnelStepType Type { get; set; }
    [Required, MaxLength(200)] public string Title { get; set; } = "Neuer Schritt";
    [MaxLength(10000)] public string Content { get; set; } = "";
    [MaxLength(4000)] public string Options { get; set; } = "";
}
public class DomainMapping
{
    public int Id { get; set; }
    public int FunnelId { get; set; }
    public Funnel Funnel { get; set; } = null!;
    [MaxLength(253)] public string Host { get; set; } = "";
    // Activation is an operator action after domain ownership/DNS verification.
    public bool IsEnabled { get; set; }
}
public class Recipient
{
    public int Id { get; set; }
    public int FunnelId { get; set; }
    public Funnel Funnel { get; set; } = null!;
    public Guid PublicId { get; set; } = Guid.NewGuid();
    [MaxLength(120)] public string FirstName { get; set; } = "";
    [MaxLength(120)] public string LastName { get; set; } = "";
    [MaxLength(200)] public string Company { get; set; } = "";
    public DateTime? OpenedAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    [MaxLength(20)] public string Status { get; set; } = "pending";
    public Lead? Lead { get; set; }
}
public class FunnelEvent
{
    public long Id { get; set; }
    public int RecipientId { get; set; }
    public Recipient Recipient { get; set; } = null!;
    // Nullable reference preserves history when an editor removes a step.
    public int? FunnelStepId { get; set; }
    public FunnelStep? FunnelStep { get; set; }
    [MaxLength(20)] public string Type { get; set; } = "open";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
public class Lead
{
    public int Id { get; set; }
    public int RecipientId { get; set; }
    public Recipient Recipient { get; set; } = null!;
    [MaxLength(254), EmailAddress] public string Email { get; set; } = "";
    [MaxLength(60)] public string Phone { get; set; } = "";
    public string AnswersJson { get; set; } = "{}";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
public class VideoBinding
{
    public int Id { get; set; }
    public int FunnelStepId { get; set; }
    public FunnelStep FunnelStep { get; set; } = null!;
    [MaxLength(200)] public string PvsPublicId { get; set; } = "";
    public string VariablesJson { get; set; } = "{}";
}
