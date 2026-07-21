using Microsoft.AspNetCore.Http;
using ShiftOne.Infrastructure.Providers.Files;
using ShiftOne.Shared.Constants;

namespace ShiftOne.Tests.Services.User;

public class FileServiceUploadTests
{
    [Fact]
    public async Task UploadImageAsync_WhenImageIsValid_ReturnsUploadsRelativePath()
    {
        var service = new FileService(new TestCurrentUserService());
        var ownerId = Guid.NewGuid();
        var file = CreateFormFile("avatar.png", new byte[] { 1, 2, 3 });

        var path = await service.UploadImageAsync(FilePathType.UserProfiles, ownerId, file);

        Assert.StartsWith($"/uploads/UserProfiles/{ownerId}/", path);
        Assert.EndsWith(".png", path);
        await service.DeleteFileAsync(path);
    }

    [Fact]
    public async Task UploadImageAsync_WhenExtensionIsNotAllowed_ThrowsValidationError()
    {
        var service = new FileService(new TestCurrentUserService());
        var file = CreateFormFile("avatar.exe", new byte[] { 1, 2, 3 });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadImageAsync(FilePathType.UserProfiles, Guid.NewGuid(), file));

        Assert.Equal("Only jpg, jpeg, png, and webp images are allowed.", exception.Message);
    }

    [Fact]
    public async Task UploadImageAsync_WhenImageIsTooLarge_ThrowsValidationError()
    {
        var service = new FileService(new TestCurrentUserService());
        var file = CreateFormFile("avatar.jpg", new byte[(5 * 1024 * 1024) + 1]);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UploadImageAsync(FilePathType.UserProfiles, Guid.NewGuid(), file));

        Assert.Equal("Image file size cannot exceed 5 MB.", exception.Message);
    }

    private static IFormFile CreateFormFile(string fileName, byte[] bytes)
    {
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName);
    }
}
