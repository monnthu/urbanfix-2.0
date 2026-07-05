namespace UrbanFix.API.Services;

public sealed record PreparedImageResult(
    Guid Id,
    string StoragePath,
    string PublicUrl,
    string ContentType,
    long FileSizeBytes);

public interface IImageUploadService
{
    Task<PreparedImageResult> PrepareImageAsync(
        Guid reportId,
        IFormFile file,
        short sortOrder,
        CancellationToken cancellationToken = default);
}
