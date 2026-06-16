namespace KTALearningHub.API.Models;

public class LessonProgress
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LessonId { get; set; }

    // Step completion tracking
    public bool VideoCompleted { get; set; }
    public DateTime? VideoCompletedAt { get; set; }

    public bool NotesCompleted { get; set; }
    public DateTime? NotesCompletedAt { get; set; }

    public bool AudioCompleted { get; set; }
    public DateTime? AudioCompletedAt { get; set; }

    public bool AssignmentCompleted { get; set; }
    public DateTime? AssignmentCompletedAt { get; set; }

    public bool ReflectionCompleted { get; set; }
    public DateTime? ReflectionCompletedAt { get; set; }

    public bool DiscussionCompleted { get; set; }
    public DateTime? DiscussionCompletedAt { get; set; }

    public bool RatingCompleted { get; set; }
    public DateTime? RatingCompletedAt { get; set; }

    public bool IsFullyCompleted { get; set; }
    public DateTime? FullyCompletedAt { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User User { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
}
