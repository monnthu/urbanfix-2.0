using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Urbanfix.Models;
using Urbanfix.Services;

namespace Urbanfix.Controllers;

[Authorize(Roles = ProfileRoles.Admin)]
public class AdminController : Controller
{
    private readonly ReportApiService _reportApiService;

    public AdminController(ReportApiService reportApiService)
    {
        _reportApiService = reportApiService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var (success, data, error) = await _reportApiService.ObtenerReportesAsync(cancellationToken);

        if (!success || data is null)
        {
            TempData["Error"] = error ?? "No se pudieron cargar los reportes.";
            return View(new List<AdminReportStatusViewModel>());
        }

        var model = data
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new AdminReportStatusViewModel
            {
                ReportId = r.Id,
                Title = r.Title,
                Status = r.Status,
                StatusLabel = r.StatusLabel ?? r.Status.GetDisplayName(),
                CreatedAt = r.CreatedAt
            })
            .ToList();

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        Guid reportId,
        ReportStatus status,
        CancellationToken cancellationToken)
    {
        var (success, error) = await _reportApiService.ActualizarEstadoAsync(
            reportId,
            status,
            cancellationToken);

        if (!success)
        {
            TempData["Error"] = error ?? "No se pudo actualizar el estado.";
        }
        else
        {
            TempData["Success"] = "Estado del reporte actualizado.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid reportId, CancellationToken cancellationToken)
    {
        var (success, error) = await _reportApiService.EliminarReporteAsync(reportId, cancellationToken);

        if (!success)
        {
            TempData["Error"] = error ?? "No se pudo eliminar el reporte.";
        }
        else
        {
            TempData["Success"] = "Reporte eliminado correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }
}
