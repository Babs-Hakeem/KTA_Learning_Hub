namespace KTALearningHub.API.DTOs;

// ===== DASHBOARD DTOs =====
public class AdminDashboardResponse
{
    public int TotalStudents { get; set; }
    public int TotalCourses { get; set; }
    public int TotalEnrollments { get; set; }
    public int TotalAssignmentsSubmitted { get; set; }
    public int TotalReflectionsSubmitted { get; set; }
    public double AverageCourseRating { get; set; }
    public List<RecentEnrollmentResponse> RecentEnrollments { get; set; } = new();
    public List<RecentSubmissionResponse> RecentSubmissions { get; set; } = new();
}

public class RecentEnrollmentResponse
{
    public Guid Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class RecentSubmissionResponse
{
    public Guid Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string LessonTitle { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Assignment or Reflection
    public DateTime SubmittedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

// ===== STUDENT MANAGEMENT DTOs =====
public class StudentDetailResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfileImageUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime? LastLogin { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CoursesEnrolled { get; set; }
    public decimal AverageProgress { get; set; }
    public int AssignmentsSubmitted { get; set; }
    public int ReflectionsSubmitted { get; set; }
    public List<EnrollmentResponse> Enrollments { get; set; } = new();
}

public class StudentListResponse
{
    public List<StudentDetailResponse> Students { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class UpdateStudentStatusRequest
{
    public string Status { get; set; } = string.Empty; // Active, Inactive
}

public class CreateStudentRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// ===== ANALYTICS DTOs =====
public class CourseAnalyticsResponse
{
    public Guid CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int TotalEnrollments { get; set; }
    public int ActiveEnrollments { get; set; }
    public int CompletedEnrollments { get; set; }
    public decimal CompletionRate { get; set; }
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public LessonAnalyticsSummary? MostViewedLesson { get; set; }
    public LessonAnalyticsSummary? MostCommentedLesson { get; set; }
    public LessonAnalyticsSummary? MostSubmittedAssignment { get; set; }
    public List<ActiveStudentResponse> MostActiveStudents { get; set; } = new();
}

public class LessonAnalyticsSummary
{
    public Guid LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ActiveStudentResponse
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public decimal ProgressPercentage { get; set; }
    public int AssignmentsSubmitted { get; set; }
    public int CommentsPosted { get; set; }
}

public class PlatformAnalyticsResponse
{
    public int TotalStudents { get; set; }
    public int TotalCourses { get; set; }
    public int TotalEnrollments { get; set; }
    public decimal TotalRevenue { get; set; }
    public double OverallAverageRating { get; set; }
    public decimal OverallCompletionRate { get; set; }
    public List<CourseAnalyticsSummary> CourseBreakdown { get; set; } = new();
    public List<MonthlyStatResponse> MonthlyStats { get; set; } = new();
}

public class CourseAnalyticsSummary
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Enrollments { get; set; }
    public decimal Revenue { get; set; }
    public double AverageRating { get; set; }
}

public class MonthlyStatResponse
{
    public string Month { get; set; } = string.Empty;
    public int NewStudents { get; set; }
    public int NewEnrollments { get; set; }
    public decimal Revenue { get; set; }
}

// ===== SETTINGS DTOs =====
public class UpdatePlatformSettingsRequest
{
    public string? LogoUrl { get; set; }
    public string? PlatformName { get; set; }
    public string? PrimaryColor { get; set; }
    public string? EmailSettingsJson { get; set; }
    public string? PaymentGatewayJson { get; set; }
    public string? NotificationSettingsJson { get; set; }
}

public class PlatformSettingsResponse
{
    public Guid Id { get; set; }
    public string? LogoUrl { get; set; }
    public string PlatformName { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = string.Empty;
    public string? EmailSettingsJson { get; set; }
    public string? PaymentGatewayJson { get; set; }
    public string? NotificationSettingsJson { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// ===== COMMON =====
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> Ok(T data, string message = "Success")
        => new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message)
        => new() { Success = false, Message = message };
}

public class PaginationParams
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    public int Page { get; set; } = 1;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = true;
}

public class FileUploadResponse
{
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public long FileSize { get; set; }
}
