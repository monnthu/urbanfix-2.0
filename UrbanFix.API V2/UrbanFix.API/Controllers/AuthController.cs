using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanFix.API.Contracts.Requests;
using UrbanFix.API.Data;
using UrbanFix.API.Helpers;
using UrbanFix.API.Services;

namespace UrbanFix.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ReportContext _context;
    private readonly ISupabaseAdminAuthService _adminAuthService;

    public AuthController(
        ReportContext context,
        ISupabaseAdminAuthService adminAuthService)
    {
        _context = context;
        _adminAuthService = adminAuthService;
    }

    [HttpGet("national-id-available")]
    public async Task<ActionResult<object>> IsNationalIdAvailable(
        [FromQuery] string nationalId,
        CancellationToken cancellationToken)
    {
        if (!SalvadoranIdValidator.TryNormalize(nationalId, out var normalized))
        {
            return BadRequest(new { error = "El DUI o ID personal no tiene un formato valido." });
        }

        var exists = await _context.Profiles
            .AsNoTracking()
            .AnyAsync(p => p.NationalId == normalized, cancellationToken);

        return Ok(new { available = !exists, normalizedId = normalized });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        if (!SalvadoranIdValidator.TryNormalize(request.NationalId, out var normalizedId))
        {
            return BadRequest(new { error = "El DUI o ID personal no tiene un formato valido." });
        }

        var exists = await _context.Profiles
            .AsNoTracking()
            .AnyAsync(p => p.NationalId == normalizedId, cancellationToken);

        if (exists)
        {
            return BadRequest(new { error = "Ese DUI o ID personal ya esta registrado." });
        }

        var (success, error) = await _adminAuthService.CreateConfirmedUserAsync(
            request.Email,
            request.Password,
            request.FullName,
            normalizedId,
            cancellationToken);

        if (!success)
        {
            return BadRequest(new { error = error ?? "No se pudo completar el registro." });
        }

        return Ok(new { message = "Cuenta creada correctamente." });
    }
}
