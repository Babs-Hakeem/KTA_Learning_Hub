using KTALearningHub.API.Models.Enums;

namespace KTALearningHub.API.Models;

public class User
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Student;
    public string? ProfileImageUrl { get; set; }
    public DateTime? LastLogin { get; set; }
    public UserStatus Status { get; set; } = UserStatus.Active;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    public ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();
    public ICollection<ReflectionSubmission> ReflectionSubmissions { get; set; } = new List<ReflectionSubmission>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<CommentLike> CommentLikes { get; set; } = new List<CommentLike>();
    public ICollection<LessonRating> LessonRatings { get; set; } = new List<LessonRating>();
    public ICollection<Course> InstructedCourses { get; set; } = new List<Course>();
}
