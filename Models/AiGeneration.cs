using System.ComponentModel.DataAnnotations;

namespace ToolboxPortal.Models;

public class AiGeneration
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = "";

    [Required]
    public string Prompt { get; set; } = "";

    [Required]
    public string Result { get; set; } = "";

    public string Model { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}