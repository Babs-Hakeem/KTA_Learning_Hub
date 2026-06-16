using KTALearningHub.API.DTOs;
using KTALearningHub.API.Services.Interfaces;

namespace KTALearningHub.API.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public FileService(IWebHostEnvironment environment, IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    public async Task<FileUploadResponse> UploadFileAsync(IFormFile file, string subFolder)
    {
        var uploadPath = _configuration["FileStorage:UploadPath"] ?? "wwwroot/uploads";
        var maxSizeMB = int.Parse(_configuration["FileStorage:MaxFileSizeMB"] ?? "50");

        if (file.Length > maxSizeMB * 1024 * 1024)
            throw new InvalidOperationException($"File size exceeds the maximum allowed size of {maxSizeMB}MB.");

        var folderPath = Path.Combine(_environment.ContentRootPath, uploadPath, subFolder);
        Directory.CreateDirectory(folderPath);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine(folderPath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var fileUrl = $"/uploads/{subFolder}/{fileName}";

        return new FileUploadResponse
        {
            FileName = file.FileName,
            FileUrl = fileUrl,
            FileSize = file.Length
        };
    }

    public bool DeleteFile(string fileUrl)
    {
        if (string.IsNullOrEmpty(fileUrl))
            return false;

        var uploadPath = _configuration["FileStorage:UploadPath"] ?? "wwwroot/uploads";
        var relativePath = fileUrl.Replace("/uploads/", "");
        var filePath = Path.Combine(_environment.ContentRootPath, uploadPath, relativePath);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return true;
        }

        return false;
    }
}
