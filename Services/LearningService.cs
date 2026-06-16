using KTALearningHub.API.Data;
using KTALearningHub.API.DTOs;
using KTALearningHub.API.Models;
using KTALearningHub.API.Models.Enums;
using KTALearningHub.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KTALearningHub.API.Services;

public class LearningService : ILearningService
{
    private readonly ApplicationDbContext _context;

    public LearningService(ApplicationDbContext context)
    {
        _context = context;
    }

    // ===== PROGRESS =====
    public async Task<ApiResponse<LessonProgressResponse>> GetLessonProgressAsync(Guid userId, Guid lessonId)
    {
        var progress = await _context.LessonProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

        if (progress == null)
            return ApiResponse<LessonProgressResponse>.Ok(new LessonProgressResponse
            {
                UserId = userId,
                LessonId = lessonId,
                CompletionPercentage = 0
            });

        return ApiResponse<LessonProgressResponse>.Ok(MapToProgressResponse(progress));
    }

    public async Task<ApiResponse<LessonProgressResponse>> MarkStepCompleteAsync(Guid userId, Guid lessonId, MarkStepCompleteRequest request)
    {
        var lesson = await _context.Lessons.FindAsync(lessonId);
        if (lesson == null)
            return ApiResponse<LessonProgressResponse>.Fail("Lesson not found.");

        var progress = await _context.LessonProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

        if (progress == null)
        {
            progress = new LessonProgress
            {
                UserId = userId,
                LessonId = lessonId
            };
            _context.LessonProgresses.Add(progress);
        }

        var now = DateTime.UtcNow;
        switch (request.Step.ToLower())
        {
            case "video": progress.VideoCompleted = true; progress.VideoCompletedAt = now; break;
            case "notes": progress.NotesCompleted = true; progress.NotesCompletedAt = now; break;
            case "audio": progress.AudioCompleted = true; progress.AudioCompletedAt = now; break;
            case "assignment": progress.AssignmentCompleted = true; progress.AssignmentCompletedAt = now; break;
            case "reflection": progress.ReflectionCompleted = true; progress.ReflectionCompletedAt = now; break;
            case "discussion": progress.DiscussionCompleted = true; progress.DiscussionCompletedAt = now; break;
            case "rating": progress.RatingCompleted = true; progress.RatingCompletedAt = now; break;
            default: return ApiResponse<LessonProgressResponse>.Fail($"Invalid step: {request.Step}");
        }

        // Check if all applicable steps are completed
        var allComplete = progress.VideoCompleted && progress.NotesCompleted && progress.AudioCompleted;
        if (lesson.HasAssignment) allComplete = allComplete && progress.AssignmentCompleted;
        if (lesson.EnableReflection) allComplete = allComplete && progress.ReflectionCompleted;
        if (lesson.EnableDiscussion) allComplete = allComplete && progress.DiscussionCompleted;
        if (lesson.EnableRating) allComplete = allComplete && progress.RatingCompleted;

        if (allComplete && !progress.IsFullyCompleted)
        {
            progress.IsFullyCompleted = true;
            progress.FullyCompletedAt = now;
        }

        await _context.SaveChangesAsync();
        return ApiResponse<LessonProgressResponse>.Ok(MapToProgressResponse(progress), "Step completed.");
    }

    // ===== ASSIGNMENTS =====
    public async Task<ApiResponse<AssignmentSubmissionResponse>> SubmitAssignmentAsync(Guid userId, SubmitAssignmentRequest request)
    {
        var lesson = await _context.Lessons.FindAsync(request.LessonId);
        if (lesson == null || !lesson.HasAssignment)
            return ApiResponse<AssignmentSubmissionResponse>.Fail("Lesson or assignment not found.");

        var existing = await _context.AssignmentSubmissions
            .FirstOrDefaultAsync(a => a.UserId == userId && a.LessonId == request.LessonId);

        if (existing != null)
        {
            existing.TextSubmission = request.TextSubmission;
            existing.DocumentUrl = request.DocumentUrl;
            existing.DocumentFileName = request.DocumentFileName;
            existing.Status = AssignmentReviewStatus.Pending;
            existing.SubmittedAt = DateTime.UtcNow;
            existing.Feedback = null;
            existing.ReviewedById = null;
            existing.ReviewedAt = null;
        }
        else
        {
            existing = new AssignmentSubmission
            {
                UserId = userId,
                LessonId = request.LessonId,
                TextSubmission = request.TextSubmission,
                DocumentUrl = request.DocumentUrl,
                DocumentFileName = request.DocumentFileName,
                Status = AssignmentReviewStatus.Pending
            };
            _context.AssignmentSubmissions.Add(existing);
        }

        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);
        var response = new AssignmentSubmissionResponse
        {
            Id = existing.Id,
            UserId = userId,
            StudentName = user?.FullName ?? string.Empty,
            StudentEmail = user?.Email ?? string.Empty,
            LessonId = request.LessonId,
            LessonTitle = lesson.Title,
            TextSubmission = existing.TextSubmission,
            DocumentUrl = existing.DocumentUrl,
            DocumentFileName = existing.DocumentFileName,
            Status = existing.Status.ToString(),
            SubmittedAt = existing.SubmittedAt
        };

        return ApiResponse<AssignmentSubmissionResponse>.Ok(response, "Assignment submitted successfully.");
    }

    public async Task<ApiResponse<AssignmentSubmissionResponse>> ReviewAssignmentAsync(Guid submissionId, Guid reviewerId, ReviewAssignmentRequest request)
    {
        var submission = await _context.AssignmentSubmissions
            .Include(a => a.User)
            .Include(a => a.Lesson)
            .FirstOrDefaultAsync(a => a.Id == submissionId);

        if (submission == null)
            return ApiResponse<AssignmentSubmissionResponse>.Fail("Submission not found.");

        submission.Status = Enum.Parse<AssignmentReviewStatus>(request.Status, true);
        submission.Feedback = request.Feedback;
        submission.ReviewedById = reviewerId;
        submission.ReviewedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var reviewer = await _context.Users.FindAsync(reviewerId);
        var course = await _context.Modules
            .Where(m => m.Id == submission.Lesson.ModuleId)
            .Select(m => m.Course.Title)
            .FirstOrDefaultAsync();

        return ApiResponse<AssignmentSubmissionResponse>.Ok(new AssignmentSubmissionResponse
        {
            Id = submission.Id,
            UserId = submission.UserId,
            StudentName = submission.User.FullName,
            StudentEmail = submission.User.Email,
            LessonId = submission.LessonId,
            LessonTitle = submission.Lesson.Title,
            CourseName = course,
            TextSubmission = submission.TextSubmission,
            DocumentUrl = submission.DocumentUrl,
            DocumentFileName = submission.DocumentFileName,
            Status = submission.Status.ToString(),
            Feedback = submission.Feedback,
            ReviewedByName = reviewer?.FullName,
            SubmittedAt = submission.SubmittedAt,
            ReviewedAt = submission.ReviewedAt
        }, "Assignment reviewed successfully.");
    }

    public async Task<ApiResponse<AssignmentListResponse>> GetAssignmentsAsync(PaginationParams paginationParams, Guid? courseId = null, Guid? lessonId = null, string? status = null)
    {
        var query = _context.AssignmentSubmissions
            .Include(a => a.User)
            .Include(a => a.Lesson)
                .ThenInclude(l => l.Module)
                    .ThenInclude(m => m.Course)
            .Include(a => a.ReviewedBy)
            .AsQueryable();

        if (lessonId.HasValue)
            query = query.Where(a => a.LessonId == lessonId.Value);

        if (courseId.HasValue)
            query = query.Where(a => a.Lesson.Module.CourseId == courseId.Value);

        if (!string.IsNullOrEmpty(status))
        {
            var statusEnum = Enum.Parse<AssignmentReviewStatus>(status, true);
            query = query.Where(a => a.Status == statusEnum);
        }

        if (!string.IsNullOrEmpty(paginationParams.Search))
        {
            var search = paginationParams.Search.ToLower();
            query = query.Where(a => a.User.FullName.ToLower().Contains(search) || a.User.Email.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();
        var submissions = await query
            .OrderByDescending(a => a.SubmittedAt)
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return ApiResponse<AssignmentListResponse>.Ok(new AssignmentListResponse
        {
            Submissions = submissions.Select(a => new AssignmentSubmissionResponse
            {
                Id = a.Id,
                UserId = a.UserId,
                StudentName = a.User.FullName,
                StudentEmail = a.User.Email,
                LessonId = a.LessonId,
                LessonTitle = a.Lesson.Title,
                CourseName = a.Lesson.Module?.Course?.Title,
                TextSubmission = a.TextSubmission,
                DocumentUrl = a.DocumentUrl,
                DocumentFileName = a.DocumentFileName,
                Status = a.Status.ToString(),
                Feedback = a.Feedback,
                ReviewedByName = a.ReviewedBy?.FullName,
                SubmittedAt = a.SubmittedAt,
                ReviewedAt = a.ReviewedAt
            }).ToList(),
            TotalCount = totalCount,
            Page = paginationParams.Page,
            PageSize = paginationParams.PageSize
        });
    }

    public async Task<ApiResponse<AssignmentSubmissionResponse>> GetAssignmentByIdAsync(Guid submissionId)
    {
        var a = await _context.AssignmentSubmissions
            .Include(s => s.User)
            .Include(s => s.Lesson).ThenInclude(l => l.Module).ThenInclude(m => m.Course)
            .Include(s => s.ReviewedBy)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        if (a == null)
            return ApiResponse<AssignmentSubmissionResponse>.Fail("Submission not found.");

        return ApiResponse<AssignmentSubmissionResponse>.Ok(new AssignmentSubmissionResponse
        {
            Id = a.Id, UserId = a.UserId, StudentName = a.User.FullName, StudentEmail = a.User.Email,
            LessonId = a.LessonId, LessonTitle = a.Lesson.Title, CourseName = a.Lesson.Module?.Course?.Title,
            TextSubmission = a.TextSubmission, DocumentUrl = a.DocumentUrl, DocumentFileName = a.DocumentFileName,
            Status = a.Status.ToString(), Feedback = a.Feedback, ReviewedByName = a.ReviewedBy?.FullName,
            SubmittedAt = a.SubmittedAt, ReviewedAt = a.ReviewedAt
        });
    }

    // ===== REFLECTIONS =====
    public async Task<ApiResponse<ReflectionSubmissionResponse>> SubmitReflectionAsync(Guid userId, SubmitReflectionRequest request)
    {
        var lesson = await _context.Lessons.FindAsync(request.LessonId);
        if (lesson == null || !lesson.EnableReflection)
            return ApiResponse<ReflectionSubmissionResponse>.Fail("Lesson not found or reflection not enabled.");

        var existing = await _context.ReflectionSubmissions
            .FirstOrDefaultAsync(r => r.UserId == userId && r.LessonId == request.LessonId);

        if (existing != null)
        {
            existing.ReflectionType = Enum.Parse<ReflectionType>(request.ReflectionType, true);
            existing.TextContent = request.TextContent;
            existing.FileUrl = request.FileUrl;
            existing.FileName = request.FileName;
            existing.IsReviewed = false;
            existing.AdminComment = null;
            existing.SubmittedAt = DateTime.UtcNow;
        }
        else
        {
            existing = new ReflectionSubmission
            {
                UserId = userId,
                LessonId = request.LessonId,
                ReflectionType = Enum.Parse<ReflectionType>(request.ReflectionType, true),
                TextContent = request.TextContent,
                FileUrl = request.FileUrl,
                FileName = request.FileName
            };
            _context.ReflectionSubmissions.Add(existing);
        }

        await _context.SaveChangesAsync();
        var user = await _context.Users.FindAsync(userId);

        return ApiResponse<ReflectionSubmissionResponse>.Ok(new ReflectionSubmissionResponse
        {
            Id = existing.Id, UserId = userId, StudentName = user?.FullName ?? "", StudentEmail = user?.Email ?? "",
            LessonId = request.LessonId, LessonTitle = lesson.Title,
            ReflectionType = existing.ReflectionType.ToString(),
            TextContent = existing.TextContent, FileUrl = existing.FileUrl, FileName = existing.FileName,
            IsReviewed = false, SubmittedAt = existing.SubmittedAt
        }, "Reflection submitted successfully.");
    }

    public async Task<ApiResponse<ReflectionSubmissionResponse>> ReviewReflectionAsync(Guid submissionId, Guid reviewerId, ReviewReflectionRequest request)
    {
        var submission = await _context.ReflectionSubmissions
            .Include(r => r.User).Include(r => r.Lesson)
            .FirstOrDefaultAsync(r => r.Id == submissionId);

        if (submission == null)
            return ApiResponse<ReflectionSubmissionResponse>.Fail("Reflection not found.");

        submission.AdminComment = request.AdminComment;
        submission.IsReviewed = true;
        submission.ReviewedById = reviewerId;
        submission.ReviewedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        var reviewer = await _context.Users.FindAsync(reviewerId);

        return ApiResponse<ReflectionSubmissionResponse>.Ok(new ReflectionSubmissionResponse
        {
            Id = submission.Id, UserId = submission.UserId,
            StudentName = submission.User.FullName, StudentEmail = submission.User.Email,
            LessonId = submission.LessonId, LessonTitle = submission.Lesson.Title,
            ReflectionType = submission.ReflectionType.ToString(),
            TextContent = submission.TextContent, FileUrl = submission.FileUrl, FileName = submission.FileName,
            AdminComment = submission.AdminComment, ReviewedByName = reviewer?.FullName,
            IsReviewed = true, SubmittedAt = submission.SubmittedAt, ReviewedAt = submission.ReviewedAt
        }, "Reflection reviewed successfully.");
    }

    public async Task<ApiResponse<ReflectionListResponse>> GetReflectionsAsync(PaginationParams paginationParams, Guid? courseId = null, Guid? lessonId = null, string? reflectionType = null)
    {
        var query = _context.ReflectionSubmissions
            .Include(r => r.User).Include(r => r.Lesson).ThenInclude(l => l.Module).ThenInclude(m => m.Course)
            .Include(r => r.ReviewedBy)
            .AsQueryable();

        if (lessonId.HasValue) query = query.Where(r => r.LessonId == lessonId.Value);
        if (courseId.HasValue) query = query.Where(r => r.Lesson.Module.CourseId == courseId.Value);
        if (!string.IsNullOrEmpty(reflectionType))
        {
            var typeEnum = Enum.Parse<ReflectionType>(reflectionType, true);
            query = query.Where(r => r.ReflectionType == typeEnum);
        }

        var totalCount = await query.CountAsync();
        var submissions = await query.OrderByDescending(r => r.SubmittedAt)
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return ApiResponse<ReflectionListResponse>.Ok(new ReflectionListResponse
        {
            Submissions = submissions.Select(r => new ReflectionSubmissionResponse
            {
                Id = r.Id, UserId = r.UserId, StudentName = r.User.FullName, StudentEmail = r.User.Email,
                LessonId = r.LessonId, LessonTitle = r.Lesson.Title, CourseName = r.Lesson.Module?.Course?.Title,
                ReflectionType = r.ReflectionType.ToString(), TextContent = r.TextContent,
                FileUrl = r.FileUrl, FileName = r.FileName, AdminComment = r.AdminComment,
                ReviewedByName = r.ReviewedBy?.FullName, IsReviewed = r.IsReviewed,
                SubmittedAt = r.SubmittedAt, ReviewedAt = r.ReviewedAt
            }).ToList(),
            TotalCount = totalCount, Page = paginationParams.Page, PageSize = paginationParams.PageSize
        });
    }

    public async Task<ApiResponse<ReflectionSubmissionResponse>> GetReflectionByIdAsync(Guid submissionId)
    {
        var r = await _context.ReflectionSubmissions
            .Include(s => s.User).Include(s => s.Lesson).ThenInclude(l => l.Module).ThenInclude(m => m.Course)
            .Include(s => s.ReviewedBy)
            .FirstOrDefaultAsync(s => s.Id == submissionId);

        if (r == null) return ApiResponse<ReflectionSubmissionResponse>.Fail("Reflection not found.");

        return ApiResponse<ReflectionSubmissionResponse>.Ok(new ReflectionSubmissionResponse
        {
            Id = r.Id, UserId = r.UserId, StudentName = r.User.FullName, StudentEmail = r.User.Email,
            LessonId = r.LessonId, LessonTitle = r.Lesson.Title, CourseName = r.Lesson.Module?.Course?.Title,
            ReflectionType = r.ReflectionType.ToString(), TextContent = r.TextContent,
            FileUrl = r.FileUrl, FileName = r.FileName, AdminComment = r.AdminComment,
            ReviewedByName = r.ReviewedBy?.FullName, IsReviewed = r.IsReviewed,
            SubmittedAt = r.SubmittedAt, ReviewedAt = r.ReviewedAt
        });
    }

    // ===== RATINGS =====
    public async Task<ApiResponse<LessonRatingResponse>> SubmitRatingAsync(Guid userId, SubmitRatingRequest request)
    {
        var lesson = await _context.Lessons.FindAsync(request.LessonId);
        if (lesson == null || !lesson.EnableRating)
            return ApiResponse<LessonRatingResponse>.Fail("Lesson not found or rating not enabled.");

        var existing = await _context.LessonRatings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.LessonId == request.LessonId);

        if (existing != null)
        {
            existing.Rating = request.Rating;
            existing.Feedback = request.Feedback;
        }
        else
        {
            existing = new LessonRating
            {
                UserId = userId,
                LessonId = request.LessonId,
                Rating = request.Rating,
                Feedback = request.Feedback
            };
            _context.LessonRatings.Add(existing);
        }

        await _context.SaveChangesAsync();
        var user = await _context.Users.FindAsync(userId);

        return ApiResponse<LessonRatingResponse>.Ok(new LessonRatingResponse
        {
            Id = existing.Id, UserId = userId, StudentName = user?.FullName ?? "",
            LessonId = request.LessonId, LessonTitle = lesson.Title,
            Rating = existing.Rating, Feedback = existing.Feedback, CreatedAt = existing.CreatedAt
        }, "Rating submitted successfully.");
    }

    public async Task<ApiResponse<List<LessonRatingResponse>>> GetLessonRatingsAsync(Guid lessonId)
    {
        var ratings = await _context.LessonRatings
            .Include(r => r.User).Include(r => r.Lesson)
            .Where(r => r.LessonId == lessonId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new LessonRatingResponse
            {
                Id = r.Id, UserId = r.UserId, StudentName = r.User.FullName,
                LessonId = r.LessonId, LessonTitle = r.Lesson.Title,
                Rating = r.Rating, Feedback = r.Feedback, CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        return ApiResponse<List<LessonRatingResponse>>.Ok(ratings);
    }

    // ===== HELPERS =====
    private static LessonProgressResponse MapToProgressResponse(LessonProgress p)
    {
        int completed = 0, total = 7;
        if (p.VideoCompleted) completed++;
        if (p.NotesCompleted) completed++;
        if (p.AudioCompleted) completed++;
        if (p.AssignmentCompleted) completed++;
        if (p.ReflectionCompleted) completed++;
        if (p.DiscussionCompleted) completed++;
        if (p.RatingCompleted) completed++;

        return new LessonProgressResponse
        {
            Id = p.Id, UserId = p.UserId, LessonId = p.LessonId,
            VideoCompleted = p.VideoCompleted, VideoCompletedAt = p.VideoCompletedAt,
            NotesCompleted = p.NotesCompleted, NotesCompletedAt = p.NotesCompletedAt,
            AudioCompleted = p.AudioCompleted, AudioCompletedAt = p.AudioCompletedAt,
            AssignmentCompleted = p.AssignmentCompleted, AssignmentCompletedAt = p.AssignmentCompletedAt,
            ReflectionCompleted = p.ReflectionCompleted, ReflectionCompletedAt = p.ReflectionCompletedAt,
            DiscussionCompleted = p.DiscussionCompleted, DiscussionCompletedAt = p.DiscussionCompletedAt,
            RatingCompleted = p.RatingCompleted, RatingCompletedAt = p.RatingCompletedAt,
            IsFullyCompleted = p.IsFullyCompleted, FullyCompletedAt = p.FullyCompletedAt,
            CompletionPercentage = Math.Round((decimal)completed / total * 100, 2)
        };
    }
}
