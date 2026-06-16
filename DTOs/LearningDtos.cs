using System.ComponentModel.DataAnnotations;

namespace KTALearningHub.API.DTOs;

// ===== ENROLLMENT & PAYMENT DTOs =====
public class EnrollCourseRequest
{
    [Required]
    public Guid CourseId { get; set; }
}

public class ProcessPaymentRequest
{
    [Required]
    public Guid EnrollmentId { get; set; }

    public string? PaymentMethod { get; set; }
    public string? PaymentReference { get; set; }
    public string? TransactionId { get; set; }
}

public class EnrollmentResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal ProgressPercentage { get; set; }
    public decimal CoursePrice { get; set; }
    public DateTime EnrolledAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsPaid { get; set; }
}

public class PaymentResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public Guid EnrollmentId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PaymentReference { get; set; }
    public string? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ===== LESSON PROGRESS DTOs =====
public class MarkStepCompleteRequest
{
    [Required]
    public string Step { get; set; } = string.Empty;
    // Valid steps: Video, Notes, Audio, Assignment, Reflection, Discussion, Rating
}

public class LessonProgressResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid LessonId { get; set; }
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
    public decimal CompletionPercentage { get; set; }
}

// ===== ASSIGNMENT SUBMISSION DTOs =====
public class SubmitAssignmentRequest
{
    [Required]
    public Guid LessonId { get; set; }

    public string? TextSubmission { get; set; }
    public string? DocumentUrl { get; set; }
    public string? DocumentFileName { get; set; }
}

public class ReviewAssignmentRequest
{
    [Required]
    public string Status { get; set; } = string.Empty; // Approved, NeedsRevision

    public string? Feedback { get; set; }
}

public class AssignmentSubmissionResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public Guid LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public string? CourseName { get; set; }
    public string? TextSubmission { get; set; }
    public string? DocumentUrl { get; set; }
    public string? DocumentFileName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Feedback { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class AssignmentListResponse
{
    public List<AssignmentSubmissionResponse> Submissions { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

// ===== REFLECTION SUBMISSION DTOs =====
public class SubmitReflectionRequest
{
    [Required]
    public Guid LessonId { get; set; }

    [Required]
    public string ReflectionType { get; set; } = "Text"; // Text, Voice, Document

    public string? TextContent { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
}

public class ReviewReflectionRequest
{
    public string? AdminComment { get; set; }
}

public class ReflectionSubmissionResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public Guid LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public string? CourseName { get; set; }
    public string ReflectionType { get; set; } = string.Empty;
    public string? TextContent { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public string? AdminComment { get; set; }
    public string? ReviewedByName { get; set; }
    public bool IsReviewed { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}

public class ReflectionListResponse
{
    public List<ReflectionSubmissionResponse> Submissions { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

// ===== RATING DTOs =====
public class SubmitRatingRequest
{
    [Required]
    public Guid LessonId { get; set; }

    [Required, Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(2000)]
    public string? Feedback { get; set; }
}

public class LessonRatingResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Feedback { get; set; }
    public DateTime CreatedAt { get; set; }
}

// ===== DISCUSSION DTOs =====
public class CreateCommentRequest
{
    [Required]
    public Guid LessonId { get; set; }

    public Guid? ParentCommentId { get; set; }

    [Required, MaxLength(5000)]
    public string Content { get; set; } = string.Empty;
}

public class UpdateCommentRequest
{
    [Required, MaxLength(5000)]
    public string Content { get; set; } = string.Empty;
}

public class CommentResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserProfileImage { get; set; }
    public string UserRole { get; set; } = string.Empty;
    public Guid LessonId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsPinned { get; set; }
    public bool IsHidden { get; set; }
    public int LikesCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<CommentResponse> Replies { get; set; } = new();
}

public class CommentListResponse
{
    public List<CommentResponse> Comments { get; set; } = new();
    public int TotalCount { get; set; }
}
