using EcommerceApp.Application.Interfaces.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace EcommerceApp.Infrastructure.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".pdf", ".webp"
    };

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folderPath, string fileNamePrefix)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException("Uploaded file is empty");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException("Unsupported file type");
        }

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var safeFolderPath = folderPath.Replace('\\', '/').Trim('/');
        var absoluteFolder = Path.Combine(webRootPath, safeFolderPath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(absoluteFolder);

        var safePrefix = string.Join("-", fileNamePrefix.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        var fileName = $"{safePrefix}-{Guid.NewGuid():N}{extension}";
        var absolutePath = Path.Combine(absoluteFolder, fileName);

        await using var stream = File.Create(absolutePath);
        await file.CopyToAsync(stream);

        return $"/{safeFolderPath}/{fileName}";
    }

    public Task<bool> DeleteFileAsync(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return Task.FromResult(false);
        }

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrWhiteSpace(webRootPath))
        {
            webRootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var relativePath = fileUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(webRootPath, relativePath));
        var rootPath = Path.GetFullPath(webRootPath);

        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase) || !File.Exists(fullPath))
        {
            return Task.FromResult(false);
        }

        File.Delete(fullPath);
        return Task.FromResult(true);
    }
}
