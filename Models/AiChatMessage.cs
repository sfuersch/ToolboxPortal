using System.ComponentModel.DataAnnotations;

namespace ToolboxPortal.Models;

public class AiChatMessage
{
    public int Id { get; set; }

    public int AiChatSessionId { get; set; }

    public AiChatSession? Session { get; set; }

    [Required]
    public string Role { get; set; } = "";

    [Required]
    public string Content { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}