using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace UrbanFix.API.Extensions;

public static class SupabaseJwtKeyFactory
{
    /// <summary>
    /// Supabase firma los JWT con HMAC-SHA256 usando el Legacy JWT Secret
    /// como cadena UTF-8 literal (no decodificar Base64).
    /// </summary>
    public static SymmetricSecurityKey CreateSigningKey(string jwtSecret)
    {
        if (string.IsNullOrWhiteSpace(jwtSecret))
        {
            throw new InvalidOperationException("Supabase:JwtSecret es obligatorio.");
        }

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
    }
}
