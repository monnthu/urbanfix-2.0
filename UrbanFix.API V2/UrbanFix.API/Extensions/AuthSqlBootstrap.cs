using Npgsql;

namespace UrbanFix.API.Extensions;

public static class AuthSqlBootstrap
{
    public static async Task<int> ApplyAuthTriggerAsync(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection no configurada.");

        var sqlPath = Path.Combine(AppContext.BaseDirectory, "Scripts", "supabase-auth-trigger.sql");
        if (!File.Exists(sqlPath))
        {
            sqlPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Scripts", "supabase-auth-trigger.sql"));
        }

        if (!File.Exists(sqlPath))
        {
            Console.Error.WriteLine($"No se encontro supabase-auth-trigger.sql (buscado en {sqlPath}).");
            return 1;
        }

        var sql = await File.ReadAllTextAsync(sqlPath);

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();

        Console.WriteLine("Trigger de Auth aplicado correctamente.");
        return 0;
    }
}
