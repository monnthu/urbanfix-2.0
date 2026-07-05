using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Urbanfix.Models;

namespace Urbanfix.Services;

public class ReportApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ApiSettings _apiSettings;
    private readonly ILogger<ReportApiService> _logger;

    public ReportApiService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        IOptions<ApiSettings> apiSettings,
        ILogger<ReportApiService> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _apiSettings = apiSettings.Value;
        _logger = logger;

        if (!string.IsNullOrWhiteSpace(_apiSettings.BaseUrl))
        {
            _httpClient.BaseAddress = new Uri(_apiSettings.BaseUrl.TrimEnd('/') + "/");
        }
    }

    public async Task<(bool Success, IReadOnlyList<ReportDto>? Data, string? Error)> ObtenerReportesAsync(
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidarConfiguracionApi();
        if (validationError is not null)
        {
            return (false, null, validationError);
        }

        try
        {
            var endpoint = NormalizarEndpoint(_apiSettings.ReportsEndpoint);
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return (false, null, await LeerErrorAsync(response, cancellationToken));
            }

            var reports = await response.Content.ReadFromJsonAsync<List<ReportDto>>(JsonOptions, cancellationToken)
                ?? [];

            EnriquecerMetadatos(reports);
            NormalizarUrlsDeImagenes(reports);
            return (true, reports, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reportes desde la API.");
            return (false, null, "No se pudo conectar con la API remota.");
        }
    }

    public async Task<(bool Success, ReportDto? Data, string? Error)> ObtenerReportePorIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidarConfiguracionApi();
        if (validationError is not null)
        {
            return (false, null, validationError);
        }

        try
        {
            var endpoint = $"{NormalizarEndpoint(_apiSettings.ReportsEndpoint)}/{id}";
            var response = await _httpClient.GetAsync(endpoint, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return (false, null, "Reporte no encontrado.");
            }

            if (!response.IsSuccessStatusCode)
            {
                return (false, null, await LeerErrorAsync(response, cancellationToken));
            }

            var report = await response.Content.ReadFromJsonAsync<ReportDto>(JsonOptions, cancellationToken);
            if (report is not null)
            {
                EnriquecerMetadatos([report]);
                NormalizarUrlsDeImagenes([report]);
            }

            return (true, report, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener el reporte {ReportId} desde la API.", id);
            return (false, null, "No se pudo conectar con la API remota.");
        }
    }

    public async Task<(bool Success, string? Error)> EnviarReporteAsync(
        ReportFormViewModel form,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidarConfiguracionApi(requireAuth: true);
        if (validationError is not null)
        {
            return (false, validationError);
        }

        try
        {
            var token = await ObtenerBearerTokenAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(token))
            {
                return (false, "No se pudo obtener un token JWT valido para enviar el reporte.");
            }

            using var content = BuildCreateReportContent(form);
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                NormalizarEndpoint(_apiSettings.ReportsEndpoint))
            {
                Content = content
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return (false, "Debes iniciar sesion para enviar reportes.");
            }

            return (false, await LeerErrorAsync(response, cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar reporte a la API.");
            return (false, "No se pudo conectar con la API remota.");
        }
    }

    public async Task<(bool Success, string? Error)> ActualizarEstadoAsync(
        Guid reportId,
        ReportStatus status,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidarConfiguracionApi(requireAuth: true);
        if (validationError is not null)
        {
            return (false, validationError);
        }

        try
        {
            var token = await ObtenerBearerTokenAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(token))
            {
                return (false, "Debes iniciar sesion como administrador.");
            }

            var endpoint = $"{NormalizarEndpoint(_apiSettings.ReportsEndpoint)}/{reportId}/status";
            using var request = new HttpRequestMessage(HttpMethod.Patch, endpoint)
            {
                Content = JsonContent.Create(new { status = (int)status })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return (false, "No tienes permisos para actualizar el estado del reporte.");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return (false, "Solo los administradores pueden cambiar el estado de un reporte.");
            }

            return (false, await LeerErrorAsync(response, cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar estado del reporte {ReportId}.", reportId);
            return (false, "No se pudo conectar con la API remota.");
        }
    }

    public async Task<(bool Success, string? Error)> EliminarReporteAsync(
        Guid reportId,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidarConfiguracionApi(requireAuth: true);
        if (validationError is not null)
        {
            return (false, validationError);
        }

        try
        {
            var token = await ObtenerBearerTokenAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(token))
            {
                return (false, "Debes iniciar sesion como administrador.");
            }

            var endpoint = $"{NormalizarEndpoint(_apiSettings.ReportsEndpoint)}/{reportId}";
            using var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return (false, "No tienes permisos para eliminar el reporte.");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                return (false, "Solo los administradores pueden eliminar reportes.");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return (false, "Reporte no encontrado.");
            }

            return (false, await LeerErrorAsync(response, cancellationToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar el reporte {ReportId}.", reportId);
            return (false, "No se pudo conectar con la API remota.");
        }
    }

    private MultipartFormDataContent BuildCreateReportContent(ReportFormViewModel form)
    {
        var content = new MultipartFormDataContent
        {
            { new StringContent(form.Titulo.Trim()), "Title" },
            { new StringContent(form.Descripcion.Trim()), "Description" },
            { new StringContent(((int)form.Categoria).ToString(CultureInfo.InvariantCulture)), "Category" },
            {
                new StringContent(form.Latitud.ToString(CultureInfo.InvariantCulture)),
                "Latitude"
            },
            {
                new StringContent(form.Longitud.ToString(CultureInfo.InvariantCulture)),
                "Longitude"
            }
        };

        if (form.Prioridad.HasValue)
        {
            content.Add(
                new StringContent(((int)form.Prioridad.Value).ToString(CultureInfo.InvariantCulture)),
                "Priority");
        }

        if (form.Imagen is { Length: > 0 })
        {
            var streamContent = new StreamContent(form.Imagen.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(form.Imagen.ContentType);
            content.Add(streamContent, "Images", form.Imagen.FileName);
        }

        return content;
    }

    private Task<string?> ObtenerBearerTokenAsync(CancellationToken cancellationToken)
    {
        var userToken = _httpContextAccessor.HttpContext?.User.FindFirstValue("access_token");
        if (!string.IsNullOrWhiteSpace(userToken))
        {
            return Task.FromResult<string?>(userToken);
        }

        if (!string.IsNullOrWhiteSpace(_apiSettings.BearerToken))
        {
            return Task.FromResult<string?>(_apiSettings.BearerToken.Trim());
        }

        return Task.FromResult<string?>(null);
    }

    private string? ValidarConfiguracionApi(bool requireAuth = false)
    {
        if (string.IsNullOrWhiteSpace(_apiSettings.BaseUrl))
        {
            return "Configura ApiSettings:BaseUrl en appsettings.json.";
        }

        if (!requireAuth)
        {
            return null;
        }

        if (_httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(_apiSettings.BearerToken))
        {
            return null;
        }

        return "Debes iniciar sesion para realizar esta accion.";
    }

    private static string NormalizarEndpoint(string endpoint) =>
        endpoint.TrimStart('/');

    private static void EnriquecerMetadatos(IEnumerable<ReportDto> reports)
    {
        foreach (var report in reports)
        {
            report.StatusLabel = report.Status.GetDisplayName();
            report.CategoryLabel = report.Category.GetDisplayName();
            report.PriorityLabel = report.Priority?.GetDisplayName() ?? "Sin especificar";
        }
    }

    private void NormalizarUrlsDeImagenes(IEnumerable<ReportDto> reports)
    {
        var baseUrl = _apiSettings.BaseUrl.TrimEnd('/');

        foreach (var report in reports)
        {
            foreach (var image in report.Images)
            {
                if (string.IsNullOrWhiteSpace(image.Url))
                {
                    continue;
                }

                if (Uri.TryCreate(image.Url, UriKind.Absolute, out _))
                {
                    continue;
                }

                image.Url = $"{baseUrl}/{image.Url.TrimStart('/')}";
            }
        }
    }

    private static async Task<string> LeerErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return string.IsNullOrWhiteSpace(body)
            ? $"La API respondio con error {(int)response.StatusCode}."
            : body;
    }
}
