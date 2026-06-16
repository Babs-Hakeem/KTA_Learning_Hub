using System.Security.Claims;
using KTALearningHub.API.DTOs;
using KTALearningHub.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KTALearningHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DiscussionsController : ControllerBase
{
    private readonly IDiscussionService _discussionService;

    public DiscussionsController(IDiscussionService discussionService)
    {
        _discussionService = discussionService;
    }

    /// <summary>Get comments for a lesson</summary>
    [HttpGet("lesson/{lessonId:guid}")]
    public async Task<IActionResult> GetLessonComments(Guid lessonId)
    {
        var userId = GetUserIdOrNull();
        var result = await _discussionService.GetLessonCommentsAsync(lessonId, userId);
        return Ok(result);
    }

    /// <summary>Get all comments (admin) with optional filters</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAllComments([FromQuery] PaginationParams pagination, [FromQuery] Guid? courseId, [FromQuery] Guid? lessonId)
    {
        var result = await _discussionService.GetAllCommentsAsync(pagination, courseId, lessonId);
        return Ok(result);
    }

    /// <summary>Post a new comment or reply</summary>
    [HttpPost]
    public async Task<IActionResult> CreateComment([FromBody] CreateCommentRequest request)
    {
        var userId = GetUserId();
        var result = await _discussionService.CreateCommentAsync(userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Update own comment</summary>
    [HttpPut("{commentId:guid}")]
    public async Task<IActionResult> UpdateComment(Guid commentId, [FromBody] UpdateCommentRequest request)
    {
        var userId = GetUserId();
        var result = await _discussionService.UpdateCommentAsync(commentId, userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Delete a comment (own or admin)</summary>
    [HttpDelete("{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(Guid commentId)
    {
        var userId = GetUserId();
        var isAdmin = User.IsInRole("Admin");
        var result = await _discussionService.DeleteCommentAsync(commentId, userId, isAdmin);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Like or unlike a comment</summary>
    [HttpPost("{commentId:guid}/like")]
    public async Task<IActionResult> LikeComment(Guid commentId)
    {
        var userId = GetUserId();
        var result = await _discussionService.LikeCommentAsync(commentId, userId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Toggle hide/show a comment (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{commentId:guid}/toggle-hide")]
    public async Task<IActionResult> ToggleHide(Guid commentId)
    {
        var result = await _discussionService.ToggleHideCommentAsync(commentId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Toggle pin/unpin a comment (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{commentId:guid}/toggle-pin")]
    public async Task<IActionResult> TogglePin(Guid commentId)
    {
        var result = await _discussionService.TogglePinCommentAsync(commentId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private Guid? GetUserIdOrNull()
    {
        var claim = User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return claim != null ? Guid.Parse(claim) : null;
    }
}
