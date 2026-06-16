using KTALearningHub.API.Data;
using KTALearningHub.API.DTOs;
using KTALearningHub.API.Models;
using KTALearningHub.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KTALearningHub.API.Services;

public class DiscussionService : IDiscussionService
{
    private readonly ApplicationDbContext _context;

    public DiscussionService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<CommentResponse>> CreateCommentAsync(Guid userId, CreateCommentRequest request)
    {
        var lesson = await _context.Lessons.FindAsync(request.LessonId);
        if (lesson == null || !lesson.EnableDiscussion)
            return ApiResponse<CommentResponse>.Fail("Lesson not found or discussion not enabled.");

        if (request.ParentCommentId.HasValue && !lesson.AllowReplies)
            return ApiResponse<CommentResponse>.Fail("Replies are not allowed for this lesson.");

        var comment = new Comment
        {
            UserId = userId,
            LessonId = request.LessonId,
            ParentCommentId = request.ParentCommentId,
            Content = request.Content
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var user = await _context.Users.FindAsync(userId);
        return ApiResponse<CommentResponse>.Ok(new CommentResponse
        {
            Id = comment.Id,
            UserId = userId,
            UserName = user?.FullName ?? "",
            UserProfileImage = user?.ProfileImageUrl,
            UserRole = user?.Role.ToString() ?? "",
            LessonId = comment.LessonId,
            ParentCommentId = comment.ParentCommentId,
            Content = comment.Content,
            IsPinned = false,
            IsHidden = false,
            LikesCount = 0,
            IsLikedByCurrentUser = false,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        }, "Comment posted successfully.");
    }

    public async Task<ApiResponse<CommentResponse>> UpdateCommentAsync(Guid commentId, Guid userId, UpdateCommentRequest request)
    {
        var comment = await _context.Comments.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == commentId);
        if (comment == null)
            return ApiResponse<CommentResponse>.Fail("Comment not found.");

        if (comment.UserId != userId)
            return ApiResponse<CommentResponse>.Fail("You can only edit your own comments.");

        comment.Content = request.Content;
        await _context.SaveChangesAsync();

        return ApiResponse<CommentResponse>.Ok(MapToCommentResponse(comment, userId), "Comment updated successfully.");
    }

    public async Task<ApiResponse<bool>> DeleteCommentAsync(Guid commentId, Guid userId, bool isAdmin)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
            return ApiResponse<bool>.Fail("Comment not found.");

        if (!isAdmin && comment.UserId != userId)
            return ApiResponse<bool>.Fail("You can only delete your own comments.");

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Comment deleted successfully.");
    }

    public async Task<ApiResponse<bool>> ToggleHideCommentAsync(Guid commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
            return ApiResponse<bool>.Fail("Comment not found.");

        comment.IsHidden = !comment.IsHidden;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, comment.IsHidden ? "Comment hidden." : "Comment visible.");
    }

    public async Task<ApiResponse<bool>> TogglePinCommentAsync(Guid commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
            return ApiResponse<bool>.Fail("Comment not found.");

        comment.IsPinned = !comment.IsPinned;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, comment.IsPinned ? "Comment pinned." : "Comment unpinned.");
    }

    public async Task<ApiResponse<bool>> LikeCommentAsync(Guid commentId, Guid userId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null)
            return ApiResponse<bool>.Fail("Comment not found.");

        var existingLike = await _context.CommentLikes
            .FirstOrDefaultAsync(l => l.CommentId == commentId && l.UserId == userId);

        if (existingLike != null)
        {
            _context.CommentLikes.Remove(existingLike);
            comment.LikesCount = Math.Max(0, comment.LikesCount - 1);
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Ok(false, "Like removed.");
        }

        _context.CommentLikes.Add(new CommentLike
        {
            UserId = userId,
            CommentId = commentId
        });
        comment.LikesCount++;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Ok(true, "Comment liked.");
    }

    public async Task<ApiResponse<CommentListResponse>> GetLessonCommentsAsync(Guid lessonId, Guid? currentUserId = null)
    {
        var comments = await _context.Comments
            .Include(c => c.User)
            .Include(c => c.Replies).ThenInclude(r => r.User)
            .Include(c => c.Likes)
            .Where(c => c.LessonId == lessonId && c.ParentCommentId == null && !c.IsHidden)
            .OrderByDescending(c => c.IsPinned)
            .ThenByDescending(c => c.CreatedAt)
            .ToListAsync();

        var response = comments.Select(c => MapToCommentResponseWithReplies(c, currentUserId)).ToList();

        return ApiResponse<CommentListResponse>.Ok(new CommentListResponse
        {
            Comments = response,
            TotalCount = response.Count
        });
    }

    public async Task<ApiResponse<CommentListResponse>> GetAllCommentsAsync(PaginationParams paginationParams, Guid? courseId = null, Guid? lessonId = null)
    {
        var query = _context.Comments
            .Include(c => c.User)
            .Include(c => c.Lesson).ThenInclude(l => l.Module).ThenInclude(m => m.Course)
            .AsQueryable();

        if (lessonId.HasValue) query = query.Where(c => c.LessonId == lessonId.Value);
        if (courseId.HasValue) query = query.Where(c => c.Lesson.Module.CourseId == courseId.Value);

        if (!string.IsNullOrEmpty(paginationParams.Search))
        {
            var search = paginationParams.Search.ToLower();
            query = query.Where(c => c.Content.ToLower().Contains(search) || c.User.FullName.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();
        var comments = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((paginationParams.Page - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToListAsync();

        return ApiResponse<CommentListResponse>.Ok(new CommentListResponse
        {
            Comments = comments.Select(c => MapToCommentResponse(c, null)).ToList(),
            TotalCount = totalCount
        });
    }

    private CommentResponse MapToCommentResponse(Comment c, Guid? currentUserId)
    {
        return new CommentResponse
        {
            Id = c.Id,
            UserId = c.UserId,
            UserName = c.User?.FullName ?? "",
            UserProfileImage = c.User?.ProfileImageUrl,
            UserRole = c.User?.Role.ToString() ?? "",
            LessonId = c.LessonId,
            ParentCommentId = c.ParentCommentId,
            Content = c.Content,
            IsPinned = c.IsPinned,
            IsHidden = c.IsHidden,
            LikesCount = c.LikesCount,
            IsLikedByCurrentUser = currentUserId.HasValue && c.Likes?.Any(l => l.UserId == currentUserId.Value) == true,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        };
    }

    private CommentResponse MapToCommentResponseWithReplies(Comment c, Guid? currentUserId)
    {
        var response = MapToCommentResponse(c, currentUserId);
        response.Replies = c.Replies?
            .Where(r => !r.IsHidden)
            .OrderBy(r => r.CreatedAt)
            .Select(r => MapToCommentResponse(r, currentUserId))
            .ToList() ?? new();
        return response;
    }
}
