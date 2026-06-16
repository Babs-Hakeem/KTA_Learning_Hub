using KTALearningHub.API.Models.Enums;

namespace KTALearningHub.API.Models;

public class Lesson
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public int OrderIndex { get; set; }

    // Content Section
    public string? VideoUrl { get; set; }
    public string? LessonNotes { get; set; }  // Rich text / HTML content
    public string? DownloadableResourceUrls { get; set; }  // JSON array of URLs

    // Audio Section
    public string? AudioUrl { get; set; }

    // Assignment Section
    public string? AssignmentTitle { get; set; }
    public string? AssignmentInstructions { get; set; }
    public SubmissionType AssignmentSubmissionType { get; set; } = SubmissionType.Both;
    public bool HasAssignment { get; set; }

    // Reflection Section
    public bool EnableReflection { get; set; } = true;
    public bool AllowTextReflection { get; set; } = true;
    public bool AllowVoiceReflection { get; set; } = true;
    public bool AllowDocumentReflection { get; set; } = true;

    // Community Section
    public bool EnableDiscussion { get; set; } = true;
    public bool AllowReplies { get; set; } = true;
    public bool AllowLikes { get; set; } = true;

    // Rating Section
    public bool EnableRating { get; set; } = true;

    // Status
    public LessonStatus Status { get; set; } = LessonStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public Module Module { get; set; } = null!;
    public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    public ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();
    public ICollection<ReflectionSubmission> ReflectionSubmissions { get; set; } = new List<ReflectionSubmission>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<LessonRating> LessonRatings { get; set; } = new List<LessonRating>();
}
