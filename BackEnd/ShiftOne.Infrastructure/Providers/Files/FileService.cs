using ShiftOne.Core.Interfaces.Infrastructure.Providers;
using ShiftOne.Shared.Constants;
using Microsoft.AspNetCore.Http;

namespace ShiftOne.Infrastructure.Providers.Files
{
    public class FileService : IFileService
    {
        private const long MaxImageBytes = 5 * 1024 * 1024;
        private static readonly HashSet<string> AllowedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

        private readonly string _baseDirectory;
        private readonly ICurrentUserService _currentUserService;

        public FileService(ICurrentUserService currentUserService)
        {
            _baseDirectory = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

            if (!Directory.Exists(_baseDirectory))
                Directory.CreateDirectory(_baseDirectory);
            _currentUserService = currentUserService;
        }

        public async Task<string> UploadImageAsync(FilePathType fileType, Guid ownerId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new InvalidOperationException("Image file is required.");
            }

            if (file.Length > MaxImageBytes)
            {
                throw new InvalidOperationException("Image file size cannot exceed 5 MB.");
            }

            var extension = Path.GetExtension(file.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedImageExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Only jpg, jpeg, png, and webp images are allowed.");
            }

            var safeFileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            return await UploadFileAsync(fileType, ownerId, safeFileName, memoryStream.ToArray());
        }

        public async Task<string> UploadFileAsync(FilePathType fileType, Guid ownerId, string fileName, byte[] fileData)
        {
            var relativeFolder = FilePathConstants.PathMappings[fileType];
            var safeFileName = Path.GetFileName(fileName);
            if (string.IsNullOrWhiteSpace(safeFileName))
            {
                throw new InvalidOperationException("File name is required.");
            }

            var targetFolder = Path.Combine(relativeFolder, ownerId.ToString());
            var fullFolderPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), targetFolder));
            if (!fullFolderPath.StartsWith(_baseDirectory, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid upload path.");
            }
            
            if (!Directory.Exists(fullFolderPath))
                Directory.CreateDirectory(fullFolderPath);

            var filePath = Path.Combine(fullFolderPath, safeFileName);

            await File.WriteAllBytesAsync(filePath, fileData);
            
            var relativePath = $"/{targetFolder.Replace("\\", "/").Trim('/')}/{safeFileName.Replace("\\", "/")}";
            return relativePath;
        }

        public Task<bool> DeleteFileAsync(string relativePath)
        {
            var fileFullPath = Path.Combine(Directory.GetCurrentDirectory(), relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(fileFullPath))
            {
                File.Delete(fileFullPath);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<string?> GetFileUrlAsync(string? relativePath)
        {
            if (!string.IsNullOrEmpty(relativePath))
            {                
                var imagePath = _currentUserService.GetBaseUrl(relativePath);
                return Task.FromResult<string?>(imagePath);
            }
            return Task.FromResult<string?>(null);
        }
    }
}

