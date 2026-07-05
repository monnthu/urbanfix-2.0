using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using UrbanFix.API.Options;

namespace UrbanFix.API.Services;

public class SupabaseAdminAuthService : ISupabaseAdminAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly SupabaseOptions _options;
    private readonly ILogger<SupabaseAdminAuthService> _logger;

    public SupabaseAdminAuthService(
        HttpClient httpClient,
        IOptions<SupabaseOptions> options,
        ILogger<SupabaseAdminAuthService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error)> CreateConfirmedUserAsync(
        string email,
        string password,
        string fullName,
        string nationalId,
        CancellationToken cancellationToken = default)
    {
        var supabaseUrl = _options.Url.TrimEnd('/');

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{supabaseUrl}/auth/v1/admin/users");
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.ServiceRoleKey);
        request.Headers.Add("apikey", _options.ServiceRoleKey);
        request.Content = JsonContent.Create(new
        {
            email = email.Trim(),
            password,
            email_confirm = true,
            user_metadata = new
            {
                full_name = fullName.Trim(),
                national_id = nationalId
            }
        });

        var response = await _httpClient.SendAsync(request, cancellationToken);
        if (response.IsSuccessStatusCode)
        {
            return (true, null);
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogWarning(
            "Supabase admin create user fallo ({StatusCode}): {Body}",
            (int)response.StatusCode,
            body);

        return (false, ParseSupabaseError(body) ?? "No se pudo crear la cuenta.");
    }

    private static string? ParseSupabaseError(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            var payload = JsonSerializer.Deserialize<SupabaseErrorResponse>(body, JsonOptions);
            if (!string.IsNullOrWhiteSpace(payload?.Msg))
            {
                return payload.Msg;
            }

            if (!string.IsNullOrWhiteSpace(payload?.ErrorDescription))
            {
                return payload.ErrorDescription;
            }

            if (!string.IsNullOrWhiteSpace(payload?.Message))
            {
                return payload.Message;
            }
        }
        catch
        {
            // Ignorar errores de parseo.
        }

        return null;
    }

    private sealed class SupabaseErrorResponse
    {
        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("message")]
        public string? Message { get; set; }

        [JsonPropertyName("error_description")]
        public string? ErrorDescription { get; set; }
    }
}
