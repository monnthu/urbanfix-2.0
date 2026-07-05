using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using UrbanFix.API.Options;

namespace UrbanFix.API.Services;

public class SupabaseStorageService : ISupabaseStorageService
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseOptions _options;

    public SupabaseStorageService(HttpClient httpClient, IOptions<SupabaseOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<string> UploadAsync(
        string objectPath,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var requestUri = BuildObjectUri(objectPath);
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ServiceRoleKey);
        request.Headers.Add("x-upsert", "true");

        var streamContent = new StreamContent(content);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        request.Content = streamContent;

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Error al subir imagen a Supabase Storage: {body}");
        }

        return objectPath;
    }

    public async Task DeleteAsync(
        IEnumerable<string> objectPaths,
        CancellationToken cancellationToken = default)
    {
        var paths = objectPaths.ToList();
        if (paths.Count == 0)
        {
            return;
        }

        var requestUri = $"{_options.Url.TrimEnd('/')}/storage/v1/object/{_options.StorageBucket}";
        using var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ServiceRoleKey);
        request.Content = JsonContent.Create(new { prefixes = paths });

        await _httpClient.SendAsync(request, cancellationToken);
    }

    public string GetPublicUrl(string objectPath) =>
        $"{_options.Url.TrimEnd('/')}/storage/v1/object/public/{_options.StorageBucket}/{objectPath}";

    private string BuildObjectUri(string objectPath) =>
        $"{_options.Url.TrimEnd('/')}/storage/v1/object/{_options.StorageBucket}/{objectPath}";
}
