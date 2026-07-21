using Microsoft.AspNetCore.Http;
using ShiftOne.Shared.Constants;

namespace ShiftOne.Core.Interfaces.Infrastructure.Providers
{
    public interface IFileService
    {
        Task<string> UploadImageAsync(FilePathType fileType, Guid ownerId, IFormFile file);
        Task<string> UploadFileAsync(FilePathType fileType, Guid ownerId, string fileName, byte[] fileData);
        Task<bool> DeleteFileAsync(string filePath);
        Task<string?> GetFileUrlAsync(string? filePath);
    }
}


