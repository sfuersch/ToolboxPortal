using System.ComponentModel.DataAnnotations;

namespace ToolboxPortal.Models;

public class AiChatSession
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = "";

    [Required]
    public string Title { get; set; } = "Neuer Chat";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<AiChatMessage> Messages { get; set; } = new();
}