namespace KTALearningHub.API.Models;

public class Module
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public Course Course { get; set; } = null!;
    public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
