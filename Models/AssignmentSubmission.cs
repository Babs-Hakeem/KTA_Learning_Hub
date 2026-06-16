using KTALearningHub.API.Models.Enums;

namespace KTALearningHub.API.Models;

public class AssignmentSubmission
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LessonId { get; set; }
    public string? TextSubmission { get; set; }
    public string? DocumentUrl { get; set; }
    public string? DocumentFileName { get; set; }
    public AssignmentReviewStatus Status { get; set; } = AssignmentReviewStatus.Pending;
    public string? Feedback { get; set; }
    public Guid? ReviewedById { get; set; }
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public User User { get; set; } = null!;
    public Lesson Lesson { get; set; } = null!;
    public User? ReviewedBy { get; set; }
}
