namespace UrbanFix.API.Services;

public interface ISupabaseStorageService
{
    Task<string> UploadAsync(
        string objectPath,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        IEnumerable<string> objectPaths,
        CancellationToken cancellationToken = default);

    string GetPublicUrl(string objectPath);
}
