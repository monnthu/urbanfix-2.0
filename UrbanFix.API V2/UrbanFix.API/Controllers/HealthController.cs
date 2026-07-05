using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrbanFix.API.Data;
using UrbanFix.API.Options;
using Microsoft.Extensions.Options;

namespace UrbanFix.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromServices] ReportContext context,
        [FromServices] IOptions<SupabaseOptions> supabaseOptions,
        CancellationToken cancellationToken)
    {
        var checks = new Dictionary<string, object>();

        try
        {
            var canConnect = await context.Database.CanConnectAsync(cancellationToken);
            checks["database"] = canConnect ? "connected" : "unreachable";
        }
        catch (Exception ex)
        {
            checks["database"] = new { status = "error", message = ex.Message };
        }

        var supabase = supabaseOptions.Value;
        checks["supabase"] = new
        {
            urlConfigured = !string.IsNullOrWhiteSpace(supabase.Url),
            bucket = supabase.StorageBucket
        };

        var healthy = checks["database"] as string == "connected";
        return healthy ? Ok(new { status = "healthy", checks }) : StatusCode(503, new { status = "unhealthy", checks });
    }
}
