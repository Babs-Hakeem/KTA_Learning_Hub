using System.Security.Claims;
using KTALearningHub.API.DTOs;
using KTALearningHub.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KTALearningHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    /// <summary>Enroll in a course</summary>
    [HttpPost]
    public async Task<IActionResult> Enroll([FromBody] EnrollCourseRequest request)
    {
        var userId = GetUserId();
        var result = await _enrollmentService.EnrollInCourseAsync(userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Process payment to unlock a course</summary>
    [HttpPost("payment")]
    public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentRequest request)
    {
        var userId = GetUserId();
        var result = await _enrollmentService.ProcessPaymentAsync(userId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Get all enrollments for the current user</summary>
    [HttpGet("my-courses")]
    public async Task<IActionResult> GetMyEnrollments()
    {
        var userId = GetUserId();
        var result = await _enrollmentService.GetUserEnrollmentsAsync(userId);
        return Ok(result);
    }

    /// <summary>Get enrollment status for a specific course</summary>
    [HttpGet("course/{courseId:guid}")]
    public async Task<IActionResult> GetEnrollment(Guid courseId)
    {
        var userId = GetUserId();
        var result = await _enrollmentService.GetEnrollmentAsync(userId, courseId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    private Guid GetUserId() =>
        Guid.Parse(User.FindFirstValue("userId") ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
