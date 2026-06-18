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
        var folder = $"kta-learning-hub/{subFolder}";
        var publicId = $"{Guid.NewGuid()}";

        // FIX: Use correct upload params based on file type
        // Videos and Audio MUST use VideoUploadParams for streaming to work
        if (subFolder == "videos" || subFolder == "audio")
        {
            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                PublicId = publicId,
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

        // Images MUST use ImageUploadParams
        if (subFolder == "images")
        {
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                PublicId = publicId,
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

        // Documents and everything else use RawUploadParams
        {
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                PublicId = publicId,
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
                // Cloudinary URL format: https://res.cloudinary.com/cloudname/video/upload/v1234567890/folder/publicId.ext
                var uri = new Uri(fileUrl);
                var pathSegments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

                // Find the upload segment index (usually after "upload" or "video/upload")
                int uploadIndex = -1;
                for (int i = 0; i < pathSegments.Length; i++)
                {
                    if (pathSegments[i] == "upload" || pathSegments[i] == "v" + pathSegments[i].TrimStart('v'))
                    {
                        uploadIndex = i;
                        break;
                    }
                }

                // If we found upload segment, everything after is the public ID
                if (uploadIndex >= 0 && uploadIndex + 1 < pathSegments.Length)
                {
                    var publicIdWithFolder = string.Join("/", pathSegments.Skip(uploadIndex + 1));
                    var publicId = Path.ChangeExtension(publicIdWithFolder, null);

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