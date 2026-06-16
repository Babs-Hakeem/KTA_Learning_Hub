using KTALearningHub.API.DTOs;
using KTALearningHub.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KTALearningHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // ===== DASHBOARD =====

    /// <summary>Get admin dashboard overview</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _adminService.GetDashboardAsync();
        return Ok(result);
    }

    // ===== STUDENTS =====

    /// <summary>Get all students with pagination and search</summary>
    [HttpGet("students")]
    public async Task<IActionResult> GetStudents([FromQuery] PaginationParams pagination)
    {
        var result = await _adminService.GetStudentsAsync(pagination);
        return Ok(result);
    }

    /// <summary>Get student detail by ID</summary>
    [HttpGet("students/{studentId:guid}")]
    public async Task<IActionResult> GetStudentDetail(Guid studentId)
    {
        var result = await _adminService.GetStudentDetailAsync(studentId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Update student status (Active/Inactive)</summary>
    [HttpPut("students/{studentId:guid}/status")]
    public async Task<IActionResult> UpdateStudentStatus(Guid studentId, [FromBody] UpdateStudentStatusRequest request)
    {
        var result = await _adminService.UpdateStudentStatusAsync(studentId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Create a new student account</summary>
    [HttpPost("students")]
    public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
    {
        var result = await _adminService.CreateStudentAsync(request);
        return result.Success ? CreatedAtAction(nameof(GetStudentDetail), new { studentId = result.Data!.Id }, result) : BadRequest(result);
    }

    // ===== ANALYTICS =====

    /// <summary>Get analytics for a specific course</summary>
    [HttpGet("analytics/course/{courseId:guid}")]
    public async Task<IActionResult> GetCourseAnalytics(Guid courseId)
    {
        var result = await _adminService.GetCourseAnalyticsAsync(courseId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Get platform-wide analytics</summary>
    [HttpGet("analytics/platform")]
    public async Task<IActionResult> GetPlatformAnalytics()
    {
        var result = await _adminService.GetPlatformAnalyticsAsync();
        return Ok(result);
    }

    // ===== SETTINGS =====

    /// <summary>Get platform settings</summary>
    [HttpGet("settings")]
    public async Task<IActionResult> GetSettings()
    {
        var result = await _adminService.GetPlatformSettingsAsync();
        return Ok(result);
    }

    /// <summary>Update platform settings</summary>
    [HttpPut("settings")]
    public async Task<IActionResult> UpdateSettings([FromBody] UpdatePlatformSettingsRequest request)
    {
        var result = await _adminService.UpdatePlatformSettingsAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
