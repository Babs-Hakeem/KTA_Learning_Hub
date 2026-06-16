using System.ComponentModel.DataAnnotations;

namespace KTALearningHub.API.DTOs;

// ===== LESSON REQUEST DTOs =====
public class CreateLessonRequest
{
    [Required]
    public Guid ModuleId { get; set; }

    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; set; }

    public int EstimatedDurationMinutes { get; set; }
    public int OrderIndex { get; set; }
}

public class UpdateLessonRequest
{
    [MaxLength(300)]
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? EstimatedDurationMinutes { get; set; }
    public int? OrderIndex { get; set; }
}

public class UpdateLessonContentRequest
{
    public string? VideoUrl { get; set; }
    public string? LessonNotes { get; set; }
    public string? DownloadableResourceUrls { get; set; }  // JSON array
}

public class UpdateLessonAudioRequest
{
    public string? AudioUrl { get; set; }
}

public class UpdateLessonAssignmentRequest
{
    public bool HasAssignment { get; set; }
    public string? AssignmentTitle { get; set; }
    public string? AssignmentInstructions { get; set; }
    public string SubmissionType { get; set; } = "Both";
}

public class UpdateLessonReflectionRequest
{
    public bool EnableReflection { get; set; }
    public bool AllowTextReflection { get; set; } = true;
    public bool AllowVoiceReflection { get; set; } = true;
    public bool AllowDocumentReflection { get; set; } = true;
}

public class UpdateLessonCommunityRequest
{
    public bool EnableDiscussion { get; set; }
    public bool AllowReplies { get; set; } = true;
    public bool AllowLikes { get; set; } = true;
}

public class UpdateLessonRatingSettingsRequest
{
    public bool EnableRating { get; set; }
}

public class PublishLessonRequest
{
    [Required]
    public string Status { get; set; } = "Published"; // Draft, Preview, Published
}

public class ReorderLessonsRequest
{
    [Required]
    public List<LessonOrderItem> Lessons { get; set; } = new();
}

public class LessonOrderItem
{
    public Guid LessonId { get; set; }
    public int OrderIndex { get; set; }
}

// ===== LESSON RESPONSE DTOs =====
public class LessonDetailResponse
{
    public Guid Id { get; set; }
    public Guid ModuleId { get; set; }
    public string ModuleTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public int OrderIndex { get; set; }

    // Content
    public string? VideoUrl { get; set; }
    public string? LessonNotes { get; set; }
    public string? DownloadableResourceUrls { get; set; }
    public string? AudioUrl { get; set; }

    // Assignment
    public bool HasAssignment { get; set; }
    public string? AssignmentTitle { get; set; }
    public string? AssignmentInstructions { get; set; }
    public string AssignmentSubmissionType { get; set; } = string.Empty;

    // Reflection Settings
    public bool EnableReflection { get; set; }
    public bool AllowTextReflection { get; set; }
    public bool AllowVoiceReflection { get; set; }
    public bool AllowDocumentReflection { get; set; }

    // Community Settings
    public bool EnableDiscussion { get; set; }
    public bool AllowReplies { get; set; }
    public bool AllowLikes { get; set; }

    // Rating Settings
    public bool EnableRating { get; set; }

    // Status
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Stats
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public int TotalComments { get; set; }
    public int TotalAssignmentSubmissions { get; set; }
    public int TotalReflectionSubmissions { get; set; }
}

public class StudentLessonResponse : LessonDetailResponse
{
    public LessonProgressResponse? Progress { get; set; }
    public AssignmentSubmissionResponse? MyAssignment { get; set; }
    public ReflectionSubmissionResponse? MyReflection { get; set; }
    public LessonRatingResponse? MyRating { get; set; }
}
