using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Urbanfix.Helpers;
using Urbanfix.Models;

namespace Urbanfix.Services;

public class SupabaseAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly ApiSettings _apiSettings;
    private readonly ILogger<SupabaseAuthService> _logger;

    public SupabaseAuthService(
        HttpClient httpClient,
        IOptions<ApiSettings> apiSettings,
        ILogger<SupabaseAuthService> logger)
    {
        _httpClient = httpClient;
        _apiSettings = apiSettings.Value;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error)> RegisterAsync(
        RegisterViewModel model,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetSupabaseConfig(out var supabaseUrl, out var anonKey, out var error))
        {
            return (false, error);
        }

        if (!SalvadoranIdValidator.TryNormalize(model.NationalId, out var normalizedId))
        {
            return (false, "El DUI o ID personal no tiene un formato valido.");
        }

        var nationalIdCheck = await IsNationalIdAvailableAsync(normalizedId, cancellationToken);
        if (nationalIdCheck is null)
        {
            return (false, "No se pudo validar el DUI. Intenta de nuevo.");
        }

        if (!nationalIdCheck.Value)
        {
            return (false, "Ese DUI o ID personal ya esta registrado.");
        }

        try
        {
            if (string.IsNullOrWhiteSpace(_apiSettings.BaseUrl))
            {
                return (false, "Configura ApiSettings:BaseUrl para registrar usuarios.");
            }

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_apiSettings.BaseUrl.TrimEnd('/')}/api/Auth/register");
            request.Content = JsonContent.Create(new
            {
                email = model.Email.Trim(),
                password = model.Password,
                fullName = model.FullName.Trim(),
                nationalId = normalizedId
            });

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Registro via API fallo ({StatusCode}): {Body}", (int)response.StatusCode, body);
            return (false, ParseApiError(body) ?? "No se pudo completar el registro.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar usuario en Supabase.");
            return (false, "No se pudo conectar con el servicio de autenticacion.");
        }
    }

    public async Task<(bool Success, AuthSession? Session, string? Error)> LoginAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (!TryGetSupabaseConfig(out var supabaseUrl, out var anonKey, out var error))
        {
            return (false, null, error);
        }

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{supabaseUrl}/auth/v1/token?grant_type=password");
            request.Headers.Add("apikey", anonKey);
            request.Content = JsonContent.Create(new
            {
                email = email.Trim(),
                password
            });

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Supabase login fallo ({StatusCode}): {Body}", (int)response.StatusCode, body);
                var parsed = ParseSupabaseError(body);
                if (parsed?.Contains("confirm", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return (false, null, "Tu cuenta aun no esta activa. Registrate de nuevo o contacta al administrador.");
                }

                return (false, null, parsed ?? "Correo o contrasena incorrectos.");
            }

            var payload = JsonSerializer.Deserialize<SupabaseAuthResponse>(body, JsonOptions);
            if (string.IsNullOrWhiteSpace(payload?.AccessToken))
            {
                return (false, null, "No se pudo obtener la sesion del usuario.");
            }

            var session = new AuthSession
            {
                AccessToken = payload.AccessToken,
                ExpiresIn = payload.ExpiresIn > 0 ? payload.ExpiresIn : 3600,
                UserId = payload.User?.Id ?? string.Empty,
                Email = payload.User?.Email ?? email.Trim()
            };

            return (true, session, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al iniciar sesion en Supabase.");
            return (false, null, "No se pudo conectar con el servicio de autenticacion.");
        }
    }

    public async Task<ProfileDto?> GetProfileAsync(
        string accessToken,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_apiSettings.BaseUrl))
        {
            return null;
        }

        try
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_apiSettings.BaseUrl.TrimEnd('/')}/api/Profiles/me");
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ProfileDto>(JsonOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener perfil del usuario.");
            return null;
        }
    }

    private async Task<bool?> IsNationalIdAvailableAsync(
        string normalizedId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_apiSettings.BaseUrl))
        {
            return null;
        }

        try
        {
            var url =
                $"{_apiSettings.BaseUrl.TrimEnd('/')}/api/Auth/national-id-available?nationalId={Uri.EscapeDataString(normalizedId)}";
            var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var payload = await response.Content.ReadFromJsonAsync<NationalIdAvailabilityResponse>(
                JsonOptions,
                cancellationToken);

            return payload?.Available;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar disponibilidad del DUI.");
            return null;
        }
    }

    private bool TryGetSupabaseConfig(out string supabaseUrl, out string anonKey, out string? error)
    {
        supabaseUrl = _apiSettings.SupabaseUrl?.TrimEnd('/') ?? string.Empty;
        anonKey = _apiSettings.SupabaseAnonKey ?? string.Empty;

        if (string.IsNullOrWhiteSpace(supabaseUrl) || string.IsNullOrWhiteSpace(anonKey))
        {
            error = "Configura ApiSettings:SupabaseUrl y SupabaseAnonKey.";
            return false;
        }

        error = null;
        return true;
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
        }
        catch
        {
            // Ignorar errores de parseo.
        }

        return null;
    }

    private static string? ParseApiError(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            var payload = JsonSerializer.Deserialize<ApiErrorResponse>(body, JsonOptions);
            if (!string.IsNullOrWhiteSpace(payload?.Error))
            {
                return payload.Error;
            }
        }
        catch
        {
            // Ignorar errores de parseo.
        }

        return null;
    }

    private sealed class ApiErrorResponse
    {
        [JsonPropertyName("error")]
        public string? Error { get; set; }
    }

    private sealed class SupabaseAuthResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("user")]
        public SupabaseUserResponse? User { get; set; }
    }

    private sealed class SupabaseUserResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }

    private sealed class SupabaseErrorResponse
    {
        [JsonPropertyName("error_code")]
        public string? ErrorCode { get; set; }

        [JsonPropertyName("msg")]
        public string? Msg { get; set; }

        [JsonPropertyName("error_description")]
        public string? ErrorDescription { get; set; }
    }

    private sealed class NationalIdAvailabilityResponse
    {
        [JsonPropertyName("available")]
        public bool Available { get; set; }
    }
}

public class AuthSession
{
    public string AccessToken { get; set; } = string.Empty;

    public int ExpiresIn { get; set; }

    public string UserId { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
