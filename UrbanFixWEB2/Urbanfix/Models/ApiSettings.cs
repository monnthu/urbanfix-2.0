namespace Urbanfix.Models;

public class ApiSettings
{
    public string BaseUrl { get; set; } = string.Empty;

    public string ReportsEndpoint { get; set; } = "api/Reports";

    /// <summary>
    /// JWT de Supabase (usuario autenticado). Alternativa: configurar SupabaseUrl + SupabaseAnonKey + credenciales.
    /// </summary>
    public string BearerToken { get; set; } = string.Empty;

    public string? SupabaseUrl { get; set; }

    public string? SupabaseAnonKey { get; set; }

    public string? SupabaseEmail { get; set; }

    public string? SupabasePassword { get; set; }
}
