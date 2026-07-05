namespace UrbanFix.API.Services;

public interface ISupabaseAdminAuthService
{
    Task<(bool Success, string? Error)> CreateConfirmedUserAsync(
        string email,
        string password,
        string fullName,
        string nationalId,
        CancellationToken cancellationToken = default);
}
