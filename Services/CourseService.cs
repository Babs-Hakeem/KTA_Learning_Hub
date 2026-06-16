using KTALearningHub.API.Data;
using KTALearningHub.API.DTOs;
using KTALearningHub.API.Models;
using KTALearningHub.API.Models.Enums;
using KTALearningHub.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KTALearningHub.API.Services;

public class CourseService : ICourseService
{
    private readonly ApplicationDbContext _context;

    public CourseService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<CourseResponse>> CreateCourseAsync(CreateCourseRequest request)
    {
        var course = new Course
        {
            Title = request.Title,
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            Price = request.Price,
            Category = request.Category,
            InstructorId = request.InstructorId,
            Duration = request.Duration,
            Level = Enum.Parse<CourseLevel>(request.Level, true),
            Status = Enum.Parse<CourseStatus>(request.Status, true)
        };

        _context.Courses.Add(course);
        await _context.SaveChangesAsync();

        return ApiResponse<CourseResponse>.Ok(await MapToCourseResponse(course), "Course created successfully.");
    }

    public async Task<ApiResponse<CourseResponse>> UpdateCourseAsync(Guid courseId, UpdateCourseRequest request)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null)
            return ApiResponse<CourseResponse>.Fail("Course not found.");

        if (request.Title != null) course.Title = request.Title;
        if (request.Description != null) course.Description = request.Description;
        if (request.ThumbnailUrl != null) course.ThumbnailUrl = request.ThumbnailUrl;
        if (request.Price.HasValue) course.Price = request.Price.Value;
        if (request.Category != null) course.Category = request.Category;
        if (request.InstructorId.HasValue) course.InstructorId = request.InstructorId;
        if (request.Duration != null) course.Duration = request.Duration;
        if (request.Level != null) course.Level = Enum.Parse<CourseLevel>(request.Level, true);
        if (request.Status != null) course.Status = Enum.Parse<CourseStatus>(request.Status, true);

        await _context.SaveChangesAsync();
        return ApiResponse<CourseResponse>.Ok(await MapToCourseResponse(course), "Course updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteCourseAsync(Guid courseId)
    {
        var course = await _context.Courses.FindAsync(courseId);
        if (course == null)
            return ApiResponse<bool>.Fail("Course not found.");

        _context.Courses.Remove(course);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Course deleted successfully.");
    }

    public async Task<ApiResponse<CourseDetailResponse>> GetCourseByIdAsync(Guid courseId)
    {
        var course = await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Modules.OrderBy(m => m.OrderIndex))
                .ThenInclude(m => m.Lessons.OrderBy(l => l.OrderIndex))
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return ApiResponse<CourseDetailResponse>.Fail("Course not found.");

        var allLessonIds = course.Modules.SelectMany(m => m.Lessons).Select(l => l.Id).ToList();
        var avgRating = await _context.LessonRatings
            .Where(r => allLessonIds.Contains(r.LessonId))
            .AverageAsync(r => (double?)r.Rating) ?? 0;

        var response = new CourseDetailResponse
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            ThumbnailUrl = course.ThumbnailUrl,
            Price = course.Price,
            Category = course.Category,
            InstructorId = course.InstructorId,
            InstructorName = course.Instructor?.FullName,
            Duration = course.Duration,
            Level = course.Level.ToString(),
            Status = course.Status.ToString(),
            TotalModules = course.Modules.Count,
            TotalLessons = course.Modules.Sum(m => m.Lessons.Count),
            TotalEnrollments = course.Enrollments.Count,
            AverageRating = Math.Round(avgRating, 1),
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt,
            Modules = course.Modules.Select(m => new ModuleResponse
            {
                Id = m.Id,
                CourseId = m.CourseId,
                Title = m.Title,
                Description = m.Description,
                OrderIndex = m.OrderIndex,
                TotalLessons = m.Lessons.Count,
                CreatedAt = m.CreatedAt,
                Lessons = m.Lessons.Select(l => new LessonSummaryResponse
                {
                    Id = l.Id,
                    Title = l.Title,
                    Description = l.Description,
                    EstimatedDurationMinutes = l.EstimatedDurationMinutes,
                    OrderIndex = l.OrderIndex,
                    Status = l.Status.ToString(),
                    HasAssignment = l.HasAssignment,
                    EnableReflection = l.EnableReflection,
                    EnableDiscussion = l.EnableDiscussion,
                    EnableRating = l.EnableRating
                }).ToList()
            }).ToList()
        };

        return ApiResponse<CourseDetailResponse>.Ok(response);
    }

    public async Task<ApiResponse<CourseListResponse>> GetCoursesAsync(PaginationParams paginationParams)
    {
        var query = _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Modules)
                .ThenInclude(m => m.Lessons)
            .Include(c => c.Enrollments)
            .AsQueryable();

        if (!string.IsNullOrEmpty(paginationParams.Search))
        {
            var search = paginationParams.Search.ToLower();
            query = query.Where(c => c.Title.ToLower().Contains(search) || c.Description.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        query = paginationParams.SortBy?.ToLower() switch
        {
            "title" => paginationParams.SortDescending ? query.OrderByDescending(c => c.Title) : query.OrderBy(c => c.Title),
            "price" => paginationParams.SortDescending ? query.OrderByDescending(c => c.Price) : query.OrderBy(c => c.Price),
            _ => paginationParams.SortDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt)
        };

        var courses = await query
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        var courseResponses = new List<CourseResponse>();
        foreach (var course in courses)
        {
            courseResponses.Add(await MapToCourseResponse(course));
        }

        return ApiResponse<CourseListResponse>.Ok(new CourseListResponse
        {
            Courses = courseResponses,
            TotalCount = totalCount,
            Page = paginationParams.Page,
            PageSize = paginationParams.PageSize
        });
    }

    public async Task<ApiResponse<List<CourseResponse>>> GetPublishedCoursesAsync()
    {
        var courses = await _context.Courses
            .Include(c => c.Instructor)
            .Include(c => c.Modules)
                .ThenInclude(m => m.Lessons)
            .Include(c => c.Enrollments)
            .Where(c => c.Status == CourseStatus.Published)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        var responses = new List<CourseResponse>();
        foreach (var course in courses)
        {
            responses.Add(await MapToCourseResponse(course));
        }

        return ApiResponse<List<CourseResponse>>.Ok(responses);
    }

    // ===== MODULE METHODS =====
    public async Task<ApiResponse<ModuleResponse>> CreateModuleAsync(CreateModuleRequest request)
    {
        var courseExists = await _context.Courses.AnyAsync(c => c.Id == request.CourseId);
        if (!courseExists)
            return ApiResponse<ModuleResponse>.Fail("Course not found.");

        var maxOrder = await _context.Modules
            .Where(m => m.CourseId == request.CourseId)
            .MaxAsync(m => (int?)m.OrderIndex) ?? 0;

        var module = new Module
        {
            CourseId = request.CourseId,
            Title = request.Title,
            Description = request.Description,
            OrderIndex = request.OrderIndex > 0 ? request.OrderIndex : maxOrder + 1
        };

        _context.Modules.Add(module);
        await _context.SaveChangesAsync();

        return ApiResponse<ModuleResponse>.Ok(MapToModuleResponse(module), "Module created successfully.");
    }

    public async Task<ApiResponse<ModuleResponse>> UpdateModuleAsync(Guid moduleId, UpdateModuleRequest request)
    {
        var module = await _context.Modules.Include(m => m.Lessons).FirstOrDefaultAsync(m => m.Id == moduleId);
        if (module == null)
            return ApiResponse<ModuleResponse>.Fail("Module not found.");

        if (request.Title != null) module.Title = request.Title;
        if (request.Description != null) module.Description = request.Description;
        if (request.OrderIndex.HasValue) module.OrderIndex = request.OrderIndex.Value;

        await _context.SaveChangesAsync();
        return ApiResponse<ModuleResponse>.Ok(MapToModuleResponse(module), "Module updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteModuleAsync(Guid moduleId)
    {
        var module = await _context.Modules.FindAsync(moduleId);
        if (module == null)
            return ApiResponse<bool>.Fail("Module not found.");

        _context.Modules.Remove(module);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Module deleted successfully.");
    }

    public async Task<ApiResponse<List<ModuleResponse>>> GetModulesByCourseAsync(Guid courseId)
    {
        var modules = await _context.Modules
            .Include(m => m.Lessons.OrderBy(l => l.OrderIndex))
            .Where(m => m.CourseId == courseId)
            .OrderBy(m => m.OrderIndex)
            .ToListAsync();

        return ApiResponse<List<ModuleResponse>>.Ok(modules.Select(MapToModuleResponse).ToList());
    }

    public async Task<ApiResponse<bool>> ReorderModulesAsync(ReorderModulesRequest request)
    {
        foreach (var item in request.Modules)
        {
            var module = await _context.Modules.FindAsync(item.ModuleId);
            if (module != null)
                module.OrderIndex = item.OrderIndex;
        }

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Modules reordered successfully.");
    }

    // ===== MAPPING HELPERS =====
    private async Task<CourseResponse> MapToCourseResponse(Course course)
    {
        var allLessonIds = course.Modules?.SelectMany(m => m.Lessons).Select(l => l.Id).ToList() ?? new List<Guid>();
        var avgRating = allLessonIds.Count > 0
            ? await _context.LessonRatings.Where(r => allLessonIds.Contains(r.LessonId)).AverageAsync(r => (double?)r.Rating) ?? 0
            : 0;

        return new CourseResponse
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            ThumbnailUrl = course.ThumbnailUrl,
            Price = course.Price,
            Category = course.Category,
            InstructorId = course.InstructorId,
            InstructorName = course.Instructor?.FullName,
            Duration = course.Duration,
            Level = course.Level.ToString(),
            Status = course.Status.ToString(),
            TotalModules = course.Modules?.Count ?? 0,
            TotalLessons = course.Modules?.Sum(m => m.Lessons.Count) ?? 0,
            TotalEnrollments = course.Enrollments?.Count ?? 0,
            AverageRating = Math.Round(avgRating, 1),
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt
        };
    }

    private static ModuleResponse MapToModuleResponse(Module module) => new()
    {
        Id = module.Id,
        CourseId = module.CourseId,
        Title = module.Title,
        Description = module.Description,
        OrderIndex = module.OrderIndex,
        TotalLessons = module.Lessons?.Count ?? 0,
        CreatedAt = module.CreatedAt,
        Lessons = module.Lessons?.Select(l => new LessonSummaryResponse
        {
            Id = l.Id,
            Title = l.Title,
            Description = l.Description,
            EstimatedDurationMinutes = l.EstimatedDurationMinutes,
            OrderIndex = l.OrderIndex,
            Status = l.Status.ToString(),
            HasAssignment = l.HasAssignment,
            EnableReflection = l.EnableReflection,
            EnableDiscussion = l.EnableDiscussion,
            EnableRating = l.EnableRating
        }).ToList() ?? new()
    };
}
