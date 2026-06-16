using KTALearningHub.API.DTOs;
using KTALearningHub.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KTALearningHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    /// <summary>Get all published courses (public)</summary>
    [HttpGet("published")]
    public async Task<IActionResult> GetPublishedCourses()
    {
        var result = await _courseService.GetPublishedCoursesAsync();
        return Ok(result);
    }

    /// <summary>Get all courses with pagination (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetCourses([FromQuery] PaginationParams paginationParams)
    {
        var result = await _courseService.GetCoursesAsync(paginationParams);
        return Ok(result);
    }

    /// <summary>Get course by ID with modules and lessons</summary>
    [HttpGet("{courseId:guid}")]
    public async Task<IActionResult> GetCourseById(Guid courseId)
    {
        var result = await _courseService.GetCourseByIdAsync(courseId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a new course (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
    {
        var result = await _courseService.CreateCourseAsync(request);
        return result.Success ? CreatedAtAction(nameof(GetCourseById), new { courseId = result.Data!.Id }, result) : BadRequest(result);
    }

    /// <summary>Update a course (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{courseId:guid}")]
    public async Task<IActionResult> UpdateCourse(Guid courseId, [FromBody] UpdateCourseRequest request)
    {
        var result = await _courseService.UpdateCourseAsync(courseId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Delete a course (admin)</summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{courseId:guid}")]
    public async Task<IActionResult> DeleteCourse(Guid courseId)
    {
        var result = await _courseService.DeleteCourseAsync(courseId);
        return result.Success ? Ok(result) : NotFound(result);
    }
}
