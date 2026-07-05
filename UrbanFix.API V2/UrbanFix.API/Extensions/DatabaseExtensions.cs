using Microsoft.EntityFrameworkCore;
using UrbanFix.API.Data;

namespace UrbanFix.API.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ReportContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ReportContext>>();

        try
        {
            await context.Database.MigrateAsync();
            logger.LogInformation("Migraciones de base de datos aplicadas correctamente.");
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "No se pudieron aplicar las migraciones. Ejecuta Scripts/supabase-init.sql en Supabase si es la primera vez.");
            throw;
        }
    }
}
