using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using KTALearningHub.API.DTOs;
using KTALearningHub.API.Services.Interfaces;

namespace KTALearningHub.API.Services;

public class FileService : IFileService
{
    private readonly Cloudinary _cloudinary;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public FileService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;

        var cloudName = configuration["Cloudinary:CloudName"];
        var apiKey = configuration["Cloudinary:ApiKey"];
        var apiSecret = configuration["Cloudinary:ApiSecret"];

        if (!string.IsNullOrEmpty(cloudName) && !string.IsNullOrEmpty(apiKey) && !string.IsNullOrEmpty(apiSecret))
        {
            _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
        }
    }

    public async Task<FileUploadResponse> UploadFileAsync(IFormFile file, string subFolder)
    {
        // If Cloudinary is configured, use it. Otherwise fall back to local storage.
        if (_cloudinary != null)
        {
            return await UploadToCloudinaryAsync(file, subFolder);
        }

        return await UploadToLocalAsync(file, subFolder);
    }

    private async Task<FileUploadResponse> UploadToCloudinaryAsync(IFormFile file, string subFolder)
    {
        var maxSizeMB = int.Parse(_configuration["FileStorage:MaxFileSizeMB"] ?? "50");
        if (file.Length > maxSizeMB * 1024 * 1024)
            throw new InvalidOperationException($"File size exceeds the maximum allowed size of {maxSizeMB}MB.");

        await using var stream = file.OpenReadStream();

        // Determine resource type based on folder
        var resourceType = subFolder switch
        {
            "videos" => ResourceType.Video,
            "audio" => ResourceType.Video, // Cloudinary handles audio as video type
            "images" => ResourceType.Image,
            _ => ResourceType.Raw
        };

        var uploadParams = new RawUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = $"kta-learning-hub/{subFolder}",
            PublicId = $"{Guid.NewGuid()}",
            Overwrite = false
        };

        var uploadResult = await _cloudinary.UploadAsync(uploadParams);

        if (uploadResult.Error != null)
            throw new InvalidOperationException($"Cloudinary upload failed: {uploadResult.Error.Message}");

        return new FileUploadResponse
        {
            FileName = file.FileName,
            FileUrl = uploadResult.SecureUrl?.ToString() ?? uploadResult.Url?.ToString() ?? string.Empty,
            FileSize = file.Length
        };
    }

    private async Task<FileUploadResponse> UploadToLocalAsync(IFormFile file, string subFolder)
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

        // If it's a Cloudinary URL, delete from Cloudinary
        if (fileUrl.Contains("cloudinary.com") && _cloudinary != null)
        {
            try
            {
                // Extract public ID from URL
                var uri = new Uri(fileUrl);
                var segments = uri.Segments;
                if (segments.Length >= 2)
                {
                    var publicId = string.Join("", segments.Skip(2)).TrimEnd('/');
                    publicId = Path.ChangeExtension(publicId, null); // Remove extension
                    _cloudinary.DeleteResources(publicId);
                    return true;
                }
            }
            catch { /* ignore deletion errors */ }
            return false;
        }

        // Local file deletion
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