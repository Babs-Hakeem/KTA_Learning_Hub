using KTALearningHub.API.Data;
using KTALearningHub.API.DTOs;
using KTALearningHub.API.Models;
using KTALearningHub.API.Models.Enums;
using KTALearningHub.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KTALearningHub.API.Services;

public class LessonService : ILessonService
{
    private readonly ApplicationDbContext _context;

    public LessonService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<LessonDetailResponse>> CreateLessonAsync(CreateLessonRequest request)
    {
        var moduleExists = await _context.Modules.AnyAsync(m => m.Id == request.ModuleId);
        if (!moduleExists)
            return ApiResponse<LessonDetailResponse>.Fail("Module not found.");

        var maxOrder = await _context.Lessons
            .Where(l => l.ModuleId == request.ModuleId)
            .MaxAsync(l => (int?)l.OrderIndex) ?? 0;

        var lesson = new Lesson
        {
            ModuleId = request.ModuleId,
            Title = request.Title,
            Description = request.Description,
            EstimatedDurationMinutes = request.EstimatedDurationMinutes,
            OrderIndex = request.OrderIndex > 0 ? request.OrderIndex : maxOrder + 1,
            Status = LessonStatus.Draft
        };

        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync();

        return ApiResponse<LessonDetailResponse>.Ok(await MapToLessonDetailResponse(lesson), "Lesson created successfully.");
    }

    public async Task<ApiResponse<LessonDetailResponse>> UpdateLessonAsync(Guid lessonId, UpdateLessonRequest request)
    {
        var lesson = await _context.Lessons.Include(l => l.Module).FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson == null)
            return ApiResponse<LessonDetailResponse>.Fail("Lesson not found.");

        if (request.Title != null) lesson.Title = request.Title;
        if (request.Description != null) lesson.Description = request.Description;
        if (request.EstimatedDurationMinutes.HasValue) lesson.EstimatedDurationMinutes = request.EstimatedDurationMinutes.Value;
        if (request.OrderIndex.HasValue) lesson.OrderIndex = request.OrderIndex.Value;

        await _context.SaveChangesAsync();
        return ApiResponse<LessonDetailResponse>.Ok(await MapToLessonDetailResponse(lesson), "Lesson updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteLessonAsync(Guid lessonId)
    {
        var lesson = await _context.Lessons.FindAsync(lessonId);
        if (lesson == null)
            return ApiResponse<bool>.Fail("Lesson not found.");

        _context.Lessons.Remove(lesson);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Lesson deleted successfully.");
    }

    public async Task<ApiResponse<LessonDetailResponse>> GetLessonByIdAsync(Guid lessonId)
    {
        var lesson = await _context.Lessons.Include(l => l.Module).FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson == null)
            return ApiResponse<LessonDetailResponse>.Fail("Lesson not found.");

        return ApiResponse<LessonDetailResponse>.Ok(await MapToLessonDetailResponse(lesson));
    }

    public async Task<ApiResponse<StudentLessonResponse>> GetStudentLessonAsync(Guid lessonId, Guid userId)
    {
        var lesson = await _context.Lessons.Include(l => l.Module).FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson == null)
            return ApiResponse<StudentLessonResponse>.Fail("Lesson not found.");

        var progress = await _context.LessonProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);

        var assignment = await _context.AssignmentSubmissions
            .Include(a => a.ReviewedBy)
            .FirstOrDefaultAsync(a => a.UserId == userId && a.LessonId == lessonId);

        var reflection = await _context.ReflectionSubmissions
            .Include(r => r.ReviewedBy)
            .FirstOrDefaultAsync(r => r.UserId == userId && r.LessonId == lessonId);

        var rating = await _context.LessonRatings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.LessonId == lessonId);

        var detail = await MapToLessonDetailResponse(lesson);
        var response = new StudentLessonResponse
        {
            Id = detail.Id,
            ModuleId = detail.ModuleId,
            ModuleTitle = detail.ModuleTitle,
            Title = detail.Title,
            Description = detail.Description,
            EstimatedDurationMinutes = detail.EstimatedDurationMinutes,
            OrderIndex = detail.OrderIndex,
            VideoUrl = detail.VideoUrl,
            LessonNotes = detail.LessonNotes,
            DownloadableResourceUrls = detail.DownloadableResourceUrls,
            AudioUrl = detail.AudioUrl,
            HasAssignment = detail.HasAssignment,
            AssignmentTitle = detail.AssignmentTitle,
            AssignmentInstructions = detail.AssignmentInstructions,
            AssignmentSubmissionType = detail.AssignmentSubmissionType,
            EnableReflection = detail.EnableReflection,
            AllowTextReflection = detail.AllowTextReflection,
            AllowVoiceReflection = detail.AllowVoiceReflection,
            AllowDocumentReflection = detail.AllowDocumentReflection,
            EnableDiscussion = detail.EnableDiscussion,
            AllowReplies = detail.AllowReplies,
            AllowLikes = detail.AllowLikes,
            EnableRating = detail.EnableRating,
            Status = detail.Status,
            CreatedAt = detail.CreatedAt,
            UpdatedAt = detail.UpdatedAt,
            AverageRating = detail.AverageRating,
            TotalRatings = detail.TotalRatings,
            TotalComments = detail.TotalComments,
            TotalAssignmentSubmissions = detail.TotalAssignmentSubmissions,
            TotalReflectionSubmissions = detail.TotalReflectionSubmissions,
            Progress = progress != null ? MapToProgressResponse(progress) : null,
            MyAssignment = assignment != null ? MapToAssignmentResponse(assignment) : null,
            MyReflection = reflection != null ? MapToReflectionResponse(reflection) : null,
            MyRating = rating != null ? new LessonRatingResponse
            {
                Id = rating.Id,
                UserId = rating.UserId,
                LessonId = rating.LessonId,
                Rating = rating.Rating,
                Feedback = rating.Feedback,
                CreatedAt = rating.CreatedAt
            } : null
        };

        return ApiResponse<StudentLessonResponse>.Ok(response);
    }

    public async Task<ApiResponse<List<LessonSummaryResponse>>> GetLessonsByModuleAsync(Guid moduleId)
    {
        var lessons = await _context.Lessons
            .Where(l => l.ModuleId == moduleId)
            .OrderBy(l => l.OrderIndex)
            .Select(l => new LessonSummaryResponse
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
            })
            .ToListAsync();

        return ApiResponse<List<LessonSummaryResponse>>.Ok(lessons);
    }

    public async Task<ApiResponse<LessonDetailResponse>> UpdateLessonContentAsync(Guid lessonId, UpdateLessonContentRequest request)
    {
        var lesson = await _context.Lessons.Include(l => l.Module).FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson == null) return ApiResponse<LessonDetailResponse>.Fail("Lesson not found.");

        if (request.VideoUrl != null) lesson.VideoUrl = request.VideoUrl;
        if (request.LessonNotes != null) lesson.LessonNotes = request.LessonNotes;
        if (request.DownloadableResourceUrls != null) lesson.DownloadableResourceUrls = request.DownloadableResourceUrls;

        await _context.SaveChangesAsync();
        return ApiResponse<LessonDetailResponse>.Ok(await MapToLessonDetailResponse(lesson), "Content saved successfully.");
    }

    public async Task<ApiResponse<LessonDetailResponse>> UpdateLessonAudioAsync(Guid lessonId, UpdateLessonAudioRequest request)
    {
        var lesson = await _context.Lessons.Include(l => l.Module).FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson == null) return ApiResponse<LessonDetailResponse>.Fail("Lesson not found.");

        lesson.AudioUrl = request.AudioUrl;
        await _context.SaveChangesAsync();
        return ApiResponse<LessonDetailResponse>.Ok(await MapToLessonDetailResponse(lesson), "Audio saved successfully.");
    }

    public async Task<ApiResponse<LessonDetailResponse>> UpdateLessonAssignmentAsync(Guid lessonId, UpdateLessonAssignmentRequest request)
    {
        var lesson = await _context.Lessons.Include(l => l.Module).FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson == null) return ApiResponse<LessonDetailResponse>.Fail("Lesson not found.");

        lesson.HasAssignment = request.HasAssignment;
        lesson.AssignmentTitle = request.AssignmentTitle;
        lesson.AssignmentInstructions = request.AssignmentInstructions;
        lesson.AssignmentSubmissionType = Enum.Parse<SubmissionType>(request.SubmissionType, true);

        await _context.SaveChangesAsync();
        return ApiResponse<LessonDetailResponse>.Ok(await MapToLessonDetailResponse(lesson), "Assignment settings saved.");
    }

    public async Task<ApiResponse<LessonDetailResponse>> UpdateLessonReflectionAsync(Guid lessonId, UpdateLessonReflectionRequest request)
    {
        var lesson = await _context.Lessons.Include(l => l.Module).FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson == null) return ApiResponse<LessonDetailResponse>.Fail("Lesson not found.");

        lesson.EnableReflection = request.EnableReflection;
        lesson.AllowTextReflection = request.AllowTextReflection;
        lesson.AllowVoiceReflection = request.AllowVoiceReflection;
        lesson.AllowDocumentReflection = request.AllowDocumentReflection;

        await _context.SaveChangesAsync();
        return ApiResponse<LessonDetailResponse>.Ok(await MapToLessonDetailResponse(lesson), "Reflection settings saved.");
    }

    public async Task<ApiResponse<LessonDetailResponse>> UpdateLessonCommunityAsync(Guid lessonId, UpdateLessonCommunityRequest request)
    {
        var lesson = await _context.Lessons.Include(l => l.Module).FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson == null) return ApiResponse<LessonDetailResponse>.Fail("Lesson not found.");

        lesson.EnableDiscussion = request.EnableDiscussion;
        lesson.AllowReplies = request.AllowReplies;
        lesson.AllowLikes = request.AllowLikes;

        await _context.SaveChangesAsync();
        return ApiResponse<LessonDetailResponse>.Ok(await MapToLessonDetailResponse(lesson), "Community settings saved.");
    }

    public async Task<ApiResponse<LessonDetailResponse>> UpdateLessonRatingSettingsAsync(Guid lessonId, UpdateLessonRatingSettingsRequest request)
    {
        var lesson = await _context.Lessons.Include(l => l.Module).FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson == null) return ApiResponse<LessonDetailResponse>.Fail("Lesson not found.");

        lesson.EnableRating = request.EnableRating;
        await _context.SaveChangesAsync();
        return ApiResponse<LessonDetailResponse>.Ok(await MapToLessonDetailResponse(lesson), "Rating settings saved.");
    }

    public async Task<ApiResponse<LessonDetailResponse>> PublishLessonAsync(Guid lessonId, PublishLessonRequest request)
    {
        var lesson = await _context.Lessons.Include(l => l.Module).FirstOrDefaultAsync(l => l.Id == lessonId);
        if (lesson == null) return ApiResponse<LessonDetailResponse>.Fail("Lesson not found.");

        lesson.Status = Enum.Parse<LessonStatus>(request.Status, true);
        await _context.SaveChangesAsync();
        return ApiResponse<LessonDetailResponse>.Ok(await MapToLessonDetailResponse(lesson), $"Lesson status set to {request.Status}.");
    }

    public async Task<ApiResponse<bool>> ReorderLessonsAsync(ReorderLessonsRequest request)
    {
        foreach (var item in request.Lessons)
        {
            var lesson = await _context.Lessons.FindAsync(item.LessonId);
            if (lesson != null) lesson.OrderIndex = item.OrderIndex;
        }

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Lessons reordered successfully.");
    }

    // ===== PRIVATE HELPERS =====
    private async Task<LessonDetailResponse> MapToLessonDetailResponse(Lesson lesson)
    {
        var avgRating = await _context.LessonRatings.Where(r => r.LessonId == lesson.Id).AverageAsync(r => (double?)r.Rating) ?? 0;
        var totalRatings = await _context.LessonRatings.CountAsync(r => r.LessonId == lesson.Id);
        var totalComments = await _context.Comments.CountAsync(c => c.LessonId == lesson.Id && !c.IsHidden);
        var totalAssignments = await _context.AssignmentSubmissions.CountAsync(a => a.LessonId == lesson.Id);
        var totalReflections = await _context.ReflectionSubmissions.CountAsync(r => r.LessonId == lesson.Id);

        return new LessonDetailResponse
        {
            Id = lesson.Id,
            ModuleId = lesson.ModuleId,
            ModuleTitle = lesson.Module?.Title ?? string.Empty,
            Title = lesson.Title,
            Description = lesson.Description,
            EstimatedDurationMinutes = lesson.EstimatedDurationMinutes,
            OrderIndex = lesson.OrderIndex,
            VideoUrl = lesson.VideoUrl,
            LessonNotes = lesson.LessonNotes,
            DownloadableResourceUrls = lesson.DownloadableResourceUrls,
            AudioUrl = lesson.AudioUrl,
            HasAssignment = lesson.HasAssignment,
            AssignmentTitle = lesson.AssignmentTitle,
            AssignmentInstructions = lesson.AssignmentInstructions,
            AssignmentSubmissionType = lesson.AssignmentSubmissionType.ToString(),
            EnableReflection = lesson.EnableReflection,
            AllowTextReflection = lesson.AllowTextReflection,
            AllowVoiceReflection = lesson.AllowVoiceReflection,
            AllowDocumentReflection = lesson.AllowDocumentReflection,
            EnableDiscussion = lesson.EnableDiscussion,
            AllowReplies = lesson.AllowReplies,
            AllowLikes = lesson.AllowLikes,
            EnableRating = lesson.EnableRating,
            Status = lesson.Status.ToString(),
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt,
            AverageRating = Math.Round(avgRating, 1),
            TotalRatings = totalRatings,
            TotalComments = totalComments,
            TotalAssignmentSubmissions = totalAssignments,
            TotalReflectionSubmissions = totalReflections
        };
    }

    private static LessonProgressResponse MapToProgressResponse(LessonProgress p) => new()
    {
        Id = p.Id,
        UserId = p.UserId,
        LessonId = p.LessonId,
        VideoCompleted = p.VideoCompleted,
        VideoCompletedAt = p.VideoCompletedAt,
        NotesCompleted = p.NotesCompleted,
        NotesCompletedAt = p.NotesCompletedAt,
        AudioCompleted = p.AudioCompleted,
        AudioCompletedAt = p.AudioCompletedAt,
        AssignmentCompleted = p.AssignmentCompleted,
        AssignmentCompletedAt = p.AssignmentCompletedAt,
        ReflectionCompleted = p.ReflectionCompleted,
        ReflectionCompletedAt = p.ReflectionCompletedAt,
        DiscussionCompleted = p.DiscussionCompleted,
        DiscussionCompletedAt = p.DiscussionCompletedAt,
        RatingCompleted = p.RatingCompleted,
        RatingCompletedAt = p.RatingCompletedAt,
        IsFullyCompleted = p.IsFullyCompleted,
        FullyCompletedAt = p.FullyCompletedAt
    };

    private static AssignmentSubmissionResponse MapToAssignmentResponse(AssignmentSubmission a) => new()
    {
        Id = a.Id,
        UserId = a.UserId,
        LessonId = a.LessonId,
        TextSubmission = a.TextSubmission,
        DocumentUrl = a.DocumentUrl,
        DocumentFileName = a.DocumentFileName,
        Status = a.Status.ToString(),
        Feedback = a.Feedback,
        ReviewedByName = a.ReviewedBy?.FullName,
        SubmittedAt = a.SubmittedAt,
        ReviewedAt = a.ReviewedAt
    };

    private static ReflectionSubmissionResponse MapToReflectionResponse(ReflectionSubmission r) => new()
    {
        Id = r.Id,
        UserId = r.UserId,
        LessonId = r.LessonId,
        ReflectionType = r.ReflectionType.ToString(),
        TextContent = r.TextContent,
        FileUrl = r.FileUrl,
        FileName = r.FileName,
        AdminComment = r.AdminComment,
        ReviewedByName = r.ReviewedBy?.FullName,
        IsReviewed = r.IsReviewed,
        SubmittedAt = r.SubmittedAt,
        ReviewedAt = r.ReviewedAt
    };
}
