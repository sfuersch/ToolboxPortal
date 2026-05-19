namespace ToolboxPortal.Models;

public class KnowledgeBaseArticle
{
    public int Id { get; set; }

    public string UserId { get; set; } = "";

    public string Category { get; set; } = "";

    public string Title { get; set; } = "";

    public string Slug { get; set; } = "";

    public string ContentHtml { get; set; } = "";

    public int SortOrder { get; set; }

    public bool IsPublished { get; set; } = true;

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; }
        = DateTime.UtcNow;
}