using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Urbanfix.Models;
using Urbanfix.Services;

namespace Urbanfix.Controllers;

[Authorize(Roles = ProfileRoles.Admin)]
public class AdminController : Controller
{
    private readonly ReportApiService _reportApiService;
    private readonly InstitutionDirectoryService _institutionDirectoryService;

    public AdminController(
        ReportApiService reportApiService,
        InstitutionDirectoryService institutionDirectoryService)
    {
        _reportApiService = reportApiService;
        _institutionDirectoryService = institutionDirectoryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var (success, data, error) = await _reportApiService.ObtenerReportesAsync(cancellationToken);
        var model = new AdminDashboardViewModel
        {
            Institutions = BuildInstitutionApprovalModels()
        };

        if (!success || data is null)
        {
            TempData["Error"] = error ?? "No se pudieron cargar los reportes.";
            return View(model);
        }

        model.Reports = data
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
    public IActionResult ApproveInstitution(Guid institutionId)
    {
        if (!_institutionDirectoryService.Approve(institutionId))
        {
            TempData["Error"] = "No se encontro la solicitud institucional.";
        }
        else
        {
            TempData["Success"] = "Institucion verificada correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RejectInstitution(Guid institutionId)
    {
        if (!_institutionDirectoryService.Reject(institutionId, "Solicitud rechazada por administracion."))
        {
            TempData["Error"] = "No se encontro la solicitud institucional.";
        }
        else
        {
            TempData["Success"] = "Solicitud institucional rechazada.";
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

    private IReadOnlyList<AdminInstitutionApprovalViewModel> BuildInstitutionApprovalModels()
    {
        return _institutionDirectoryService.GetApplications()
            .Select(application =>
            {
                var zone = _institutionDirectoryService.GetZone(application.ServiceZoneCode);

                return new AdminInstitutionApprovalViewModel
                {
                    Id = application.Id,
                    InstitutionName = application.InstitutionName,
                    OfficialEmail = application.OfficialEmail,
                    OfficialDomain = application.OfficialDomain,
                    ServiceZoneName = zone?.Name ?? application.ServiceZoneCode,
                    CoverageSummary = string.Join(
                        ", ",
                        application.CoverageCategories.Select(category => category.GetDisplayName())),
                    Status = application.Status,
                    StatusLabel = application.Status.GetDisplayName(),
                    CreatedAt = application.CreatedAt
                };
            })
            .ToList();
    }
}
