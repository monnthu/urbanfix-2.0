using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanFix.API.Data;
using UrbanFix.API.Helpers;

namespace UrbanFix.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ReportContext _context;

    public AuthController(ReportContext context)
    {
        _context = context;
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
}
