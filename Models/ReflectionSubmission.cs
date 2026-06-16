using KTALearningHub.API.Models.Enums;

namespace KTALearningHub.API.Models;

public class ReflectionSubmission
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LessonId { get; set; }
    public ReflectionType ReflectionType { get; set; }
    public string? TextContent { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public string? AdminComment { get; set; }
    public Guid? ReviewedById { get; set; }
    public bool IsReviewed { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User User { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
    public User? ReviewedBy { get; set; }
}
