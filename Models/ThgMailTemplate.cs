using System.ComponentModel.DataAnnotations;

namespace ToolboxPortal.Models;

public class ThgMailTemplate
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = "";

    [Required]
    public string TemplateKey { get; set; } = "";

    [Required]
    public string Name { get; set; } = "";

    [Required]
    public string Subject { get; set; } = "";

    [Required]
    public string HtmlBody { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}