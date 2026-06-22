using Microsoft.AspNetCore.Http;

namespace EcommerceApp.Application.Interfaces.Services;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(IFormFile file, string folderPath, string fileNamePrefix);
    Task<bool> DeleteFileAsync(string fileUrl);
}
