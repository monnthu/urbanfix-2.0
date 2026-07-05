using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using UrbanFix.API.Data;
using UrbanFix.API.Entities;

namespace UrbanFix.API.Authorization;

public class AdminRequirementHandler : AuthorizationHandler<AdminRequirement>
{
    private readonly ReportContext _context;

    public AdminRequirementHandler(ReportContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AdminRequirement requirement)
    {
        var sub = context.User.FindFirstValue("sub")
            ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(sub, out var userId))
        {
            return;
        }

        var isAdmin = await _context.Profiles
            .AsNoTracking()
            .AnyAsync(p => p.Id == userId && p.Role == ProfileRoles.Admin);

        if (isAdmin)
        {
            context.Succeed(requirement);
        }
    }
}
