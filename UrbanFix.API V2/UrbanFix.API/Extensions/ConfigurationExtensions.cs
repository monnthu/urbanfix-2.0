using UrbanFix.API.Options;

namespace UrbanFix.API.Extensions;

public static class ConfigurationExtensions
{
    public static void ValidateSupabaseConfiguration(this IConfiguration configuration)
    {
        var supabase = configuration.GetSection(SupabaseOptions.SectionName).Get<SupabaseOptions>()
            ?? throw new InvalidOperationException("Falta la seccion Supabase en appsettings.");

        if (string.IsNullOrWhiteSpace(supabase.Url))
        {
            throw new InvalidOperationException("Supabase:Url es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(supabase.ServiceRoleKey))
        {
            throw new InvalidOperationException("Supabase:ServiceRoleKey es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(supabase.StorageBucket))
        {
            throw new InvalidOperationException("Supabase:StorageBucket es obligatorio.");
        }

        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection es obligatorio.");
        }

        if (connectionString.Contains("YOUR_", StringComparison.OrdinalIgnoreCase)
            || supabase.Url.Contains("YOUR_", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Configura los valores reales de Supabase en appsettings.Development.json, User Secrets, " +
                "o define ASPNETCORE_ENVIRONMENT=Development al ejecutar la API.");
        }
    }
}
