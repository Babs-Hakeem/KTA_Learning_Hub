using System.Security.Claims;
using KTALearningHub.API.DTOs;
using KTALearningHub.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KTALearningHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LessonsController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonsController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    /// <summary>Get lessons by module ID</summary>
    [HttpGet("module/{moduleId:guid}")]
    public async Task<IActionResult> GetLessonsByModule(Guid moduleId)
    {
        var result = await _lessonService.GetLessonsByModuleAsync(moduleId);
        return Ok(result);
    }

    /// <summary>Get lesson detail (admin view)</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("{lessonId:guid}")]
    public async Task<IActionResult> GetLessonById(Guid lessonId)
    {
        var result = await _lessonService.GetLessonByIdAsync(lessonId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Get lesson for student (includes progress, submissions)</summary>
    [Authorize]
    [HttpGet("{lessonId:guid}/student")]
    public async Task<IActionResult> GetStudentLesson(Guid lessonId)
    {
        var userId = GetUserId();
        var result = await _lessonService.GetStudentLessonAsync(lessonId, userId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a new lesson (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateLesson([FromBody] CreateLessonRequest request)
    {
        var result = await _lessonService.CreateLessonAsync(request);
        return result.Success ? CreatedAtAction(nameof(GetLessonById), new { lessonId = result.Data!.Id }, result) : BadRequest(result);
    }

    /// <summary>Update lesson basic info (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{lessonId:guid}")]
    public async Task<IActionResult> UpdateLesson(Guid lessonId, [FromBody] UpdateLessonRequest request)
    {
        var result = await _lessonService.UpdateLessonAsync(lessonId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Delete a lesson (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{lessonId:guid}")]
    public async Task<IActionResult> DeleteLesson(Guid lessonId)
    {
        var result = await _lessonService.DeleteLessonAsync(lessonId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Update lesson content: video, notes, resources (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{lessonId:guid}/content")]
    public async Task<IActionResult> UpdateContent(Guid lessonId, [FromBody] UpdateLessonContentRequest request)
    {
        var result = await _lessonService.UpdateLessonContentAsync(lessonId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Update lesson audio (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{lessonId:guid}/audio")]
    public async Task<IActionResult> UpdateAudio(Guid lessonId, [FromBody] UpdateLessonAudioRequest request)
    {
        var result = await _lessonService.UpdateLessonAudioAsync(lessonId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Update lesson assignment settings (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{lessonId:guid}/assignment")]
    public async Task<IActionResult> UpdateAssignment(Guid lessonId, [FromBody] UpdateLessonAssignmentRequest request)
    {
        var result = await _lessonService.UpdateLessonAssignmentAsync(lessonId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Update lesson reflection settings (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{lessonId:guid}/reflection")]
    public async Task<IActionResult> UpdateReflection(Guid lessonId, [FromBody] UpdateLessonReflectionRequest request)
    {
        var result = await _lessonService.UpdateLessonReflectionAsync(lessonId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Update lesson community/discussion settings (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{lessonId:guid}/community")]
    public async Task<IActionResult> UpdateCommunity(Guid lessonId, [FromBody] UpdateLessonCommunityRequest request)
    {
        var result = await _lessonService.UpdateLessonCommunityAsync(lessonId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Update lesson rating settings (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{lessonId:guid}/rating-settings")]
    public async Task<IActionResult> UpdateRatingSettings(Guid lessonId, [FromBody] UpdateLessonRatingSettingsRequest request)
    {
        var result = await _lessonService.UpdateLessonRatingSettingsAsync(lessonId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Publish/unpublish lesson (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{lessonId:guid}/publish")]
    public async Task<IActionResult> PublishLesson(Guid lessonId, [FromBody] PublishLessonRequest request)
    {
        var result = await _lessonService.PublishLessonAsync(lessonId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Reorder lessons within a module (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("reorder")]
    public async Task<IActionResult> ReorderLessons([FromBody] ReorderLessonsRequest request)
    {
        var result = await _lessonService.ReorderLessonsAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
