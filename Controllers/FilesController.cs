using KTALearningHub.API.DTOs;
using KTALearningHub.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KTALearningHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IFileService _fileService;

    public FilesController(IFileService fileService)
    {
        _fileService = fileService;
    }

    /// <summary>Upload a video file</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("upload/video")]
    [RequestSizeLimit(500_000_000)] // 500MB
    public async Task<IActionResult> UploadVideo(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<string>.Fail("No file provided."));

        var result = await _fileService.UploadFileAsync(file, "videos");
        return Ok(ApiResponse<FileUploadResponse>.Ok(result, "Video uploaded successfully."));
    }

    /// <summary>Upload an audio file</summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("upload/audio")]
    [RequestSizeLimit(100_000_000)] // 100MB
    public async Task<IActionResult> UploadAudio(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<string>.Fail("No file provided."));

        var result = await _fileService.UploadFileAsync(file, "audio");
        return Ok(ApiResponse<FileUploadResponse>.Ok(result, "Audio uploaded successfully."));
    }

    /// <summary>Upload a document (assignment, resource, reflection)</summary>
    [HttpPost("upload/document")]
    [RequestSizeLimit(50_000_000)] // 50MB
    public async Task<IActionResult> UploadDocument(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<string>.Fail("No file provided."));

        var result = await _fileService.UploadFileAsync(file, "documents");
        return Ok(ApiResponse<FileUploadResponse>.Ok(result, "Document uploaded successfully."));
    }

    /// <summary>Upload an image (thumbnails, profile pictures)</summary>
    [HttpPost("upload/image")]
    [RequestSizeLimit(10_000_000)] // 10MB
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<string>.Fail("No file provided."));

        var result = await _fileService.UploadFileAsync(file, "images");
        return Ok(ApiResponse<FileUploadResponse>.Ok(result, "Image uploaded successfully."));
    }
}
