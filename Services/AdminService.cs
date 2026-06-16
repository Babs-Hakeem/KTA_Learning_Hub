using KTALearningHub.API.Data;
using KTALearningHub.API.DTOs;
using KTALearningHub.API.Models;
using KTALearningHub.API.Models.Enums;
using KTALearningHub.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KTALearningHub.API.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;

    public AdminService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<AdminDashboardResponse>> GetDashboardAsync()
    {
        var totalStudents = await _context.Users.CountAsync(u => u.Role == UserRole.Student);
        var totalCourses = await _context.Courses.CountAsync();
        var totalEnrollments = await _context.Enrollments.CountAsync();
        var totalAssignments = await _context.AssignmentSubmissions.CountAsync();
        var totalReflections = await _context.ReflectionSubmissions.CountAsync();
        var avgRating = await _context.LessonRatings.AverageAsync(r => (double?)r.Rating) ?? 0;

        var recentEnrollments = await _context.Enrollments
            .Include(e => e.User).Include(e => e.Course)
            .OrderByDescending(e => e.EnrolledAt)
            .Take(10)
            .Select(e => new RecentEnrollmentResponse
            {
                Id = e.Id,
                StudentName = e.User.FullName,
                CourseTitle = e.Course.Title,
                EnrolledAt = e.EnrolledAt,
                Status = e.Status.ToString()
            })
            .ToListAsync();

        var recentAssignments = await _context.AssignmentSubmissions
            .Include(a => a.User).Include(a => a.Lesson)
            .OrderByDescending(a => a.SubmittedAt)
            .Take(5)
            .Select(a => new RecentSubmissionResponse
            {
                Id = a.Id,
                StudentName = a.User.FullName,
                LessonTitle = a.Lesson.Title,
                Type = "Assignment",
                SubmittedAt = a.SubmittedAt,
                Status = a.Status.ToString()
            })
            .ToListAsync();

        var recentReflections = await _context.ReflectionSubmissions
            .Include(r => r.User).Include(r => r.Lesson)
            .OrderByDescending(r => r.SubmittedAt)
            .Take(5)
            .Select(r => new RecentSubmissionResponse
            {
                Id = r.Id,
                StudentName = r.User.FullName,
                LessonTitle = r.Lesson.Title,
                Type = "Reflection",
                SubmittedAt = r.SubmittedAt,
                Status = r.IsReviewed ? "Reviewed" : "Pending"
            })
            .ToListAsync();

        var recentSubmissions = recentAssignments.Concat(recentReflections)
            .OrderByDescending(s => s.SubmittedAt).Take(10).ToList();

        return ApiResponse<AdminDashboardResponse>.Ok(new AdminDashboardResponse
        {
            TotalStudents = totalStudents,
            TotalCourses = totalCourses,
            TotalEnrollments = totalEnrollments,
            TotalAssignmentsSubmitted = totalAssignments,
            TotalReflectionsSubmitted = totalReflections,
            AverageCourseRating = Math.Round(avgRating, 1),
            RecentEnrollments = recentEnrollments,
            RecentSubmissions = recentSubmissions
        });
    }

    public async Task<ApiResponse<StudentListResponse>> GetStudentsAsync(PaginationParams paginationParams)
    {
        var query = _context.Users.Where(u => u.Role == UserRole.Student).AsQueryable();

        if (!string.IsNullOrEmpty(paginationParams.Search))
        {
            var search = paginationParams.Search.ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(search) || u.Email.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        query = paginationParams.SortBy?.ToLower() switch
        {
            "name" => paginationParams.SortDescending ? query.OrderByDescending(u => u.FullName) : query.OrderBy(u => u.FullName),
            "email" => paginationParams.SortDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            "lastlogin" => paginationParams.SortDescending ? query.OrderByDescending(u => u.LastLogin) : query.OrderBy(u => u.LastLogin),
            _ => paginationParams.SortDescending ? query.OrderByDescending(u => u.CreatedAt) : query.OrderBy(u => u.CreatedAt)
        };

        var students = await query
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        var responses = new List<StudentDetailResponse>();
        foreach (var student in students)
        {
            responses.Add(await MapToStudentDetail(student));
        }

        return ApiResponse<StudentListResponse>.Ok(new StudentListResponse
        {
            Students = responses,
            TotalCount = totalCount,
            Page = paginationParams.Page,
            PageSize = paginationParams.PageSize
        });
    }

    public async Task<ApiResponse<StudentDetailResponse>> GetStudentDetailAsync(Guid studentId)
    {
        var student = await _context.Users.FirstOrDefaultAsync(u => u.Id == studentId);
        if (student == null)
            return ApiResponse<StudentDetailResponse>.Fail("Student not found.");

        return ApiResponse<StudentDetailResponse>.Ok(await MapToStudentDetail(student));
    }

    public async Task<ApiResponse<bool>> UpdateStudentStatusAsync(Guid studentId, UpdateStudentStatusRequest request)
    {
        var student = await _context.Users.FindAsync(studentId);
        if (student == null)
            return ApiResponse<bool>.Fail("Student not found.");

        student.Status = Enum.Parse<UserStatus>(request.Status, true);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Student status updated.");
    }

    public async Task<ApiResponse<UserResponse>> CreateStudentAsync(CreateStudentRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower()))
            return ApiResponse<UserResponse>.Fail("A user with this email already exists.");

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email.ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Student,
            Status = UserStatus.Active
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return ApiResponse<UserResponse>.Ok(new UserResponse
        {
            Id = user.Id, FullName = user.FullName, Email = user.Email,
            Role = user.Role.ToString(), Status = user.Status.ToString(), CreatedAt = user.CreatedAt
        }, "Student created successfully.");
    }

    public async Task<ApiResponse<CourseAnalyticsResponse>> GetCourseAnalyticsAsync(Guid courseId)
    {
        var course = await _context.Courses
            .Include(c => c.Modules).ThenInclude(m => m.Lessons)
            .Include(c => c.Enrollments)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return ApiResponse<CourseAnalyticsResponse>.Fail("Course not found.");

        var lessonIds = course.Modules.SelectMany(m => m.Lessons).Select(l => l.Id).ToList();
        var totalEnrollments = course.Enrollments.Count;
        var activeEnrollments = course.Enrollments.Count(e => e.Status == EnrollmentStatus.Active);
        var completedEnrollments = course.Enrollments.Count(e => e.Status == EnrollmentStatus.Completed);
        var avgRating = lessonIds.Count > 0
            ? await _context.LessonRatings.Where(r => lessonIds.Contains(r.LessonId)).AverageAsync(r => (double?)r.Rating) ?? 0 : 0;
        var totalRatings = await _context.LessonRatings.CountAsync(r => lessonIds.Contains(r.LessonId));

        var mostViewed = await _context.LessonProgresses
            .Where(p => lessonIds.Contains(p.LessonId))
            .GroupBy(p => p.LessonId)
            .Select(g => new { LessonId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync();

        var mostCommented = await _context.Comments
            .Where(c => lessonIds.Contains(c.LessonId))
            .GroupBy(c => c.LessonId)
            .Select(g => new { LessonId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync();

        var mostSubmitted = await _context.AssignmentSubmissions
            .Where(a => lessonIds.Contains(a.LessonId))
            .GroupBy(a => a.LessonId)
            .Select(g => new { LessonId = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync();

        var activeStudents = await _context.Enrollments
            .Where(e => e.CourseId == courseId && e.Status != EnrollmentStatus.Locked)
            .Include(e => e.User)
            .OrderByDescending(e => e.ProgressPercentage)
            .Take(10)
            .Select(e => new ActiveStudentResponse
            {
                UserId = e.UserId,
                FullName = e.User.FullName,
                ProgressPercentage = e.ProgressPercentage,
                AssignmentsSubmitted = _context.AssignmentSubmissions.Count(a => a.UserId == e.UserId && lessonIds.Contains(a.LessonId)),
                CommentsPosted = _context.Comments.Count(c => c.UserId == e.UserId && lessonIds.Contains(c.LessonId))
            })
            .ToListAsync();

        async Task<LessonAnalyticsSummary?> GetLessonSummary(Guid? id, int count)
        {
            if (id == null) return null;
            var lesson = await _context.Lessons.FindAsync(id);
            return lesson == null ? null : new LessonAnalyticsSummary { LessonId = lesson.Id, LessonTitle = lesson.Title, Count = count };
        }

        return ApiResponse<CourseAnalyticsResponse>.Ok(new CourseAnalyticsResponse
        {
            CourseId = courseId,
            CourseTitle = course.Title,
            TotalEnrollments = totalEnrollments,
            ActiveEnrollments = activeEnrollments,
            CompletedEnrollments = completedEnrollments,
            CompletionRate = totalEnrollments > 0 ? Math.Round((decimal)completedEnrollments / totalEnrollments * 100, 2) : 0,
            AverageRating = Math.Round(avgRating, 1),
            TotalRatings = totalRatings,
            MostViewedLesson = await GetLessonSummary(mostViewed?.LessonId, mostViewed?.Count ?? 0),
            MostCommentedLesson = await GetLessonSummary(mostCommented?.LessonId, mostCommented?.Count ?? 0),
            MostSubmittedAssignment = await GetLessonSummary(mostSubmitted?.LessonId, mostSubmitted?.Count ?? 0),
            MostActiveStudents = activeStudents
        });
    }

    public async Task<ApiResponse<PlatformAnalyticsResponse>> GetPlatformAnalyticsAsync()
    {
        var totalStudents = await _context.Users.CountAsync(u => u.Role == UserRole.Student);
        var totalCourses = await _context.Courses.CountAsync();
        var totalEnrollments = await _context.Enrollments.CountAsync();
        var totalRevenue = await _context.Payments.Where(p => p.Status == PaymentStatus.Completed).SumAsync(p => p.Amount);
        var avgRating = await _context.LessonRatings.AverageAsync(r => (double?)r.Rating) ?? 0;
        var completedEnrollments = await _context.Enrollments.CountAsync(e => e.Status == EnrollmentStatus.Completed);
        var completionRate = totalEnrollments > 0 ? Math.Round((decimal)completedEnrollments / totalEnrollments * 100, 2) : 0;

        var courseBreakdown = await _context.Courses
            .Include(c => c.Enrollments)
            .Select(c => new CourseAnalyticsSummary
            {
                CourseId = c.Id,
                Title = c.Title,
                Enrollments = c.Enrollments.Count,
                Revenue = _context.Payments.Where(p => p.CourseId == c.Id && p.Status == PaymentStatus.Completed).Sum(p => p.Amount),
                AverageRating = _context.LessonRatings
                    .Where(r => _context.Lessons
                        .Where(l => _context.Modules.Where(m => m.CourseId == c.Id).Select(m => m.Id).Contains(l.ModuleId))
                        .Select(l => l.Id).Contains(r.LessonId))
                    .Average(r => (double?)r.Rating) ?? 0
            })
            .ToListAsync();

        return ApiResponse<PlatformAnalyticsResponse>.Ok(new PlatformAnalyticsResponse
        {
            TotalStudents = totalStudents,
            TotalCourses = totalCourses,
            TotalEnrollments = totalEnrollments,
            TotalRevenue = totalRevenue,
            OverallAverageRating = Math.Round(avgRating, 1),
            OverallCompletionRate = completionRate,
            CourseBreakdown = courseBreakdown
        });
    }

    public async Task<ApiResponse<PlatformSettingsResponse>> GetPlatformSettingsAsync()
    {
        var settings = await _context.PlatformSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new PlatformSettings();
            _context.PlatformSettings.Add(settings);
            await _context.SaveChangesAsync();
        }

        return ApiResponse<PlatformSettingsResponse>.Ok(new PlatformSettingsResponse
        {
            Id = settings.Id,
            LogoUrl = settings.LogoUrl,
            PlatformName = settings.PlatformName,
            PrimaryColor = settings.PrimaryColor,
            EmailSettingsJson = settings.EmailSettingsJson,
            PaymentGatewayJson = settings.PaymentGatewayJson,
            NotificationSettingsJson = settings.NotificationSettingsJson,
            UpdatedAt = settings.UpdatedAt
        });
    }

    public async Task<ApiResponse<PlatformSettingsResponse>> UpdatePlatformSettingsAsync(UpdatePlatformSettingsRequest request)
    {
        var settings = await _context.PlatformSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            settings = new PlatformSettings();
            _context.PlatformSettings.Add(settings);
        }

        if (request.LogoUrl != null) settings.LogoUrl = request.LogoUrl;
        if (request.PlatformName != null) settings.PlatformName = request.PlatformName;
        if (request.PrimaryColor != null) settings.PrimaryColor = request.PrimaryColor;
        if (request.EmailSettingsJson != null) settings.EmailSettingsJson = request.EmailSettingsJson;
        if (request.PaymentGatewayJson != null) settings.PaymentGatewayJson = request.PaymentGatewayJson;
        if (request.NotificationSettingsJson != null) settings.NotificationSettingsJson = request.NotificationSettingsJson;

        await _context.SaveChangesAsync();

        return ApiResponse<PlatformSettingsResponse>.Ok(new PlatformSettingsResponse
        {
            Id = settings.Id,
            LogoUrl = settings.LogoUrl,
            PlatformName = settings.PlatformName,
            PrimaryColor = settings.PrimaryColor,
            EmailSettingsJson = settings.EmailSettingsJson,
            PaymentGatewayJson = settings.PaymentGatewayJson,
            NotificationSettingsJson = settings.NotificationSettingsJson,
            UpdatedAt = settings.UpdatedAt
        }, "Settings updated successfully.");
    }

    private async Task<StudentDetailResponse> MapToStudentDetail(User student)
    {
        var enrollments = await _context.Enrollments
            .Include(e => e.Course).Include(e => e.Payment)
            .Where(e => e.UserId == student.Id)
            .ToListAsync();

        var assignmentsSubmitted = await _context.AssignmentSubmissions.CountAsync(a => a.UserId == student.Id);
        var reflectionsSubmitted = await _context.ReflectionSubmissions.CountAsync(r => r.UserId == student.Id);
        var avgProgress = enrollments.Count > 0 ? enrollments.Average(e => (double)e.ProgressPercentage) : 0;

        return new StudentDetailResponse
        {
            Id = student.Id,
            FullName = student.FullName,
            Email = student.Email,
            ProfileImageUrl = student.ProfileImageUrl,
            Status = student.Status.ToString(),
            LastLogin = student.LastLogin,
            CreatedAt = student.CreatedAt,
            CoursesEnrolled = enrollments.Count,
            AverageProgress = Math.Round((decimal)avgProgress, 2),
            AssignmentsSubmitted = assignmentsSubmitted,
            ReflectionsSubmitted = reflectionsSubmitted,
            Enrollments = enrollments.Select(e => new EnrollmentResponse
            {
                Id = e.Id,
                UserId = e.UserId,
                StudentName = student.FullName,
                CourseId = e.CourseId,
                CourseTitle = e.Course?.Title ?? "",
                Status = e.Status.ToString(),
                ProgressPercentage = e.ProgressPercentage,
                CoursePrice = e.Course?.Price ?? 0,
                EnrolledAt = e.EnrolledAt,
                CompletedAt = e.CompletedAt,
                IsPaid = e.Payment?.Status == PaymentStatus.Completed
            }).ToList()
        };
    }
}
