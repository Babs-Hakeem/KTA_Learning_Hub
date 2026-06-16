using KTALearningHub.API.DTOs;
using KTALearningHub.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KTALearningHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class ModulesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public ModulesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    /// <summary>Get modules by course ID</summary>
    [HttpGet("course/{courseId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetModulesByCourse(Guid courseId)
    {
        var result = await _courseService.GetModulesByCourseAsync(courseId);
        return Ok(result);
    }

    /// <summary>Create a new module</summary>
    [HttpPost]
    public async Task<IActionResult> CreateModule([FromBody] CreateModuleRequest request)
    {
        var result = await _courseService.CreateModuleAsync(request);
        return result.Success ? CreatedAtAction(nameof(GetModulesByCourse), new { courseId = request.CourseId }, result) : BadRequest(result);
    }

    /// <summary>Update a module</summary>
    [HttpPut("{moduleId:guid}")]
    public async Task<IActionResult> UpdateModule(Guid moduleId, [FromBody] UpdateModuleRequest request)
    {
        var result = await _courseService.UpdateModuleAsync(moduleId, request);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    /// <summary>Delete a module</summary>
    [HttpDelete("{moduleId:guid}")]
    public async Task<IActionResult> DeleteModule(Guid moduleId)
    {
        var result = await _courseService.DeleteModuleAsync(moduleId);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Reorder modules within a course</summary>
    [HttpPut("reorder")]
    public async Task<IActionResult> ReorderModules([FromBody] ReorderModulesRequest request)
    {
        var result = await _courseService.ReorderModulesAsync(request);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
