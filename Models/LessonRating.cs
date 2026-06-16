namespace KTALearningHub.API.Models;

public class LessonRating
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LessonId { get; set; }
    public int Rating { get; set; }  // 1 to 5 stars
    public string? Feedback { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User User { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
}
