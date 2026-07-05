namespace UrbanFix.API.Services;

public class ImageUploadService : IImageUploadService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    private static readonly string[] AllowedContentTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private readonly ISupabaseStorageService _storage;

    public ImageUploadService(ISupabaseStorageService storage)
    {
        _storage = storage;
    }

    public async Task<PreparedImageResult> PrepareImageAsync(
        Guid reportId,
        IFormFile file,
        short sortOrder,
        CancellationToken cancellationToken = default)
    {
        ValidateFile(file);

        var imageId = Guid.NewGuid();
        var extension = GetExtension(file.ContentType);
        var storagePath = $"reports/{reportId}/{imageId}{extension}";

        await using var stream = file.OpenReadStream();
        await _storage.UploadAsync(storagePath, stream, file.ContentType, cancellationToken);

        return new PreparedImageResult(
            imageId,
            storagePath,
            _storage.GetPublicUrl(storagePath),
            file.ContentType,
            file.Length);
    }

    private static void ValidateFile(IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new InvalidOperationException("La imagen está vacía.");
        }

        if (file.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException(
                $"La imagen supera el tamaño máximo de {MaxFileSizeBytes} bytes.");
        }

        if (!AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Tipo de contenido no permitido: {file.ContentType}.");
        }
    }

    private static string GetExtension(string contentType) => contentType.ToLowerInvariant() switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        "image/webp" => ".webp",
        _ => ".bin"
    };
}
