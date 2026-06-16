using System.Security.Claims;
using KTALearningHub.API.DTOs;
using KTALearningHub.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KTALearningHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LearningController : ControllerBase
{
    private readonly ILearningService _learningService;

    public LearningController(ILearningService learningService)
    {
        _learningService = learningService;
    }

    // ===== PROGRESS =====

    /// <summary>Get lesson progress for current user</summary>
    [HttpGet("progress/{lessonId:guid}")]
    public async Task<IActionResult> GetProgress(Guid lessonId)
    {
        var userId = GetUserId();
        var result = await _learningService.GetLessonProgressAsync(userId, lessonId);
        return Ok(result);
    }

    /// <summary>Mark a lesson step as complete (Video, Notes, Audio, Assignment, Reflection, Discussion, Rating)</summary>
    [HttpPost("progress/{lessonId:guid}/complete")]
    public async Task<IActionResult> MarkStepComplete(Guid lessonId, [FromBody] MarkStepCompleteRequest request)
    {
        var userId = GetUserId();
        var result = await _learningService.MarkStepCompleteAsync(userId, lessonId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // ===== ASSIGNMENTS =====

    /// <summary>Submit an assignment</summary>
    [HttpPost("assignments/submit")]
    public async Task<IActionResult> SubmitAssignment([FromBody] SubmitAssignmentRequest request)
    {
        var userId = GetUserId();
        var result = await _learningService.SubmitAssignmentAsync(userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Review an assignment submission (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("assignments/{submissionId:guid}/review")]
    public async Task<IActionResult> ReviewAssignment(Guid submissionId, [FromBody] ReviewAssignmentRequest request)
    {
        var reviewerId = GetUserId();
        var result = await _learningService.ReviewAssignmentAsync(submissionId, reviewerId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get all assignment submissions (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("assignments")]
    public async Task<IActionResult> GetAssignments([FromQuery] PaginationParams pagination, [FromQuery] Guid? courseId, [FromQuery] Guid? lessonId, [FromQuery] string? status)
    {
        var result = await _learningService.GetAssignmentsAsync(pagination, courseId, lessonId, status);
        return Ok(result);
    }

    /// <summary>Get assignment submission by ID (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("assignments/{submissionId:guid}")]
    public async Task<IActionResult> GetAssignmentById(Guid submissionId)
    {
        var result = await _learningService.GetAssignmentByIdAsync(submissionId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // ===== REFLECTIONS =====

    /// <summary>Submit a reflection</summary>
    [HttpPost("reflections/submit")]
    public async Task<IActionResult> SubmitReflection([FromBody] SubmitReflectionRequest request)
    {
        var userId = GetUserId();
        var result = await _learningService.SubmitReflectionAsync(userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Review a reflection (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("reflections/{submissionId:guid}/review")]
    public async Task<IActionResult> ReviewReflection(Guid submissionId, [FromBody] ReviewReflectionRequest request)
    {
        var reviewerId = GetUserId();
        var result = await _learningService.ReviewReflectionAsync(submissionId, reviewerId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get all reflection submissions (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("reflections")]
    public async Task<IActionResult> GetReflections([FromQuery] PaginationParams pagination, [FromQuery] Guid? courseId, [FromQuery] Guid? lessonId, [FromQuery] string? reflectionType)
    {
        var result = await _learningService.GetReflectionsAsync(pagination, courseId, lessonId, reflectionType);
        return Ok(result);
    }

    /// <summary>Get reflection submission by ID (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("reflections/{submissionId:guid}")]
    public async Task<IActionResult> GetReflectionById(Guid submissionId)
    {
        var result = await _learningService.GetReflectionByIdAsync(submissionId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    // ===== RATINGS =====

    /// <summary>Submit or update a lesson rating</summary>
    [HttpPost("ratings/submit")]
    public async Task<IActionResult> SubmitRating([FromBody] SubmitRatingRequest request)
    {
        var userId = GetUserId();
        var result = await _learningService.SubmitRatingAsync(userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get all ratings for a lesson</summary>
    [HttpGet("ratings/lesson/{lessonId:guid}")]
    public async Task<IActionResult> GetLessonRatings(Guid lessonId)
    {
        var result = await _learningService.GetLessonRatingsAsync(lessonId);
        return Ok(result);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
