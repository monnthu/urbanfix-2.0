using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanFix.API.Contracts.Responses;
using UrbanFix.API.Data;

namespace UrbanFix.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProfilesController : ControllerBase
{
    private readonly ReportContext _context;

    public ProfilesController(ReportContext context)
    {
        _context = context;
    }

    [HttpGet("me")]
    public async Task<ActionResult<ProfileDto>> GetCurrentProfile(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var profile = await _context.Profiles
            .AsNoTracking()
            .Where(p => p.Id == userId.Value)
            .Select(p => new ProfileDto
            {
                Id = p.Id,
                Role = p.Role,
                FullName = p.FullName,
                NationalId = p.NationalId,
                CreatedAt = p.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (profile is null)
        {
            return NotFound(new
            {
                error = "No existe un perfil para este usuario. Verifica el trigger handle_new_user en Supabase."
            });
        }

        return Ok(profile);
    }

    private Guid? GetCurrentUserId()
    {
        var sub = User.FindFirstValue("sub") ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var userId) ? userId : null;
    }
}
