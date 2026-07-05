using Microsoft.AspNetCore.Mvc;
using Urbanfix.Services;

namespace Urbanfix.Controllers.Api;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly ReportApiService _reportApiService;

    public ReportsController(ReportApiService reportApiService)
    {
        _reportApiService = reportApiService;
    }

    [HttpGet]
    public async Task<IActionResult> GetReports(CancellationToken cancellationToken)
    {
        var (success, data, error) = await _reportApiService.ObtenerReportesAsync(cancellationToken);

        if (!success)
        {
            return BadRequest(new { error });
        }

        return Ok(data);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetReport(Guid id, CancellationToken cancellationToken)
    {
        var (success, data, error) = await _reportApiService.ObtenerReportePorIdAsync(id, cancellationToken);

        if (!success)
        {
            if (error == "Reporte no encontrado.")
            {
                return NotFound(new { error });
            }

            return BadRequest(new { error });
        }

        return Ok(data);
    }
}
