using System.ComponentModel.DataAnnotations;

namespace KTALearningHub.API.DTOs;

// ===== COURSE REQUEST DTOs =====
public class CreateCourseRequest
{
    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    [Required, Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }

    public Guid? InstructorId { get; set; }

    [MaxLength(50)]
    public string? Duration { get; set; }

    public string Level { get; set; } = "Beginner";
    public string Status { get; set; } = "Draft";
}

public class UpdateCourseRequest
{
    [MaxLength(300)]
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public decimal? Price { get; set; }
    public string? Category { get; set; }
    public Guid? InstructorId { get; set; }
    public string? Duration { get; set; }
    public string? Level { get; set; }
    public string? Status { get; set; }
}

// ===== MODULE REQUEST DTOs =====
public class CreateModuleRequest
{
    [Required]
    public Guid CourseId { get; set; }

    [Required, MaxLength(300)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int OrderIndex { get; set; }
}

public class UpdateModuleRequest
{
    [MaxLength(300)]
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? OrderIndex { get; set; }
}

public class ReorderModulesRequest
{
    [Required]
    public List<ModuleOrderItem> Modules { get; set; } = new();
}

public class ModuleOrderItem
{
    public Guid ModuleId { get; set; }
    public int OrderIndex { get; set; }
}

// ===== COURSE RESPONSE DTOs =====
public class CourseResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public decimal Price { get; set; }
    public string? Category { get; set; }
    public Guid? InstructorId { get; set; }
    public string? InstructorName { get; set; }
    public string? Duration { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalModules { get; set; }
    public int TotalLessons { get; set; }
    public int TotalEnrollments { get; set; }
    public double AverageRating { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CourseDetailResponse : CourseResponse
{
    public List<ModuleResponse> Modules { get; set; } = new();
}

public class ModuleResponse
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public int TotalLessons { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<LessonSummaryResponse> Lessons { get; set; } = new();
}

public class LessonSummaryResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int EstimatedDurationMinutes { get; set; }
    public int OrderIndex { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool HasAssignment { get; set; }
    public bool EnableReflection { get; set; }
    public bool EnableDiscussion { get; set; }
    public bool EnableRating { get; set; }
}

public class CourseListResponse
{
    public List<CourseResponse> Courses { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}
