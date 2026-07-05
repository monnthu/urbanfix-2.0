using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Urbanfix.Models;
using Urbanfix.Services;

namespace Urbanfix.Controllers;

[Authorize]
public class InstitutionController : Controller
{
    private static readonly string[] AllowedProofContentTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "image/webp"
    ];

    private readonly InstitutionDirectoryService _institutionDirectoryService;
    private readonly InstitutionRoutingService _institutionRoutingService;
    private readonly ReportApiService _reportApiService;

    public InstitutionController(
        InstitutionDirectoryService institutionDirectoryService,
        InstitutionRoutingService institutionRoutingService,
        ReportApiService reportApiService)
    {
        _institutionDirectoryService = institutionDirectoryService;
        _institutionRoutingService = institutionRoutingService;
        _reportApiService = reportApiService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        var profileId = GetProfileId();
        var application = _institutionDirectoryService.GetApplicationForUser(profileId);

        if (application?.Status == InstitutionStatus.Verified)
        {
            return RedirectToAction(nameof(Dashboard));
        }

        ViewBag.CurrentApplication = application;
        return View(BuildRegistrationModel(application));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Register(InstitutionRegistrationViewModel model)
    {
        model.ZoneOptions = _institutionDirectoryService.GetZoneOptions(model.ServiceZoneCode);

        ValidateProofDocument(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var profileId = GetProfileId();
        var contactEmail = User.FindFirstValue(ClaimTypes.Email) ?? model.OfficialEmail.Trim();
        var (success, error) = _institutionDirectoryService.SubmitApplication(
            profileId,
            contactEmail,
            model,
            model.ProofDocument?.FileName);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "No se pudo enviar la solicitud.");
            return View(model);
        }

        TempData["Success"] = "Solicitud enviada. Un administrador debe aprobar la institucion.";
        return RedirectToAction(nameof(Register));
    }

    [HttpGet]
    public async Task<IActionResult> Dashboard(CancellationToken cancellationToken)
    {
        var institution = _institutionDirectoryService.GetVerifiedApplicationForUser(GetProfileId());
        if (institution is null)
        {
            TempData["Error"] = "Tu institucion debe estar verificada para acceder al panel.";
            return RedirectToAction(nameof(Register));
        }

        var serviceZone = _institutionDirectoryService.GetZone(institution.ServiceZoneCode);
        var model = new InstitutionDashboardViewModel
        {
            Institution = institution,
            ServiceZoneName = serviceZone?.Name ?? institution.ServiceZoneCode
        };

        var (success, reports, error) = await _reportApiService.ObtenerReportesAsync(cancellationToken);
        if (!success || reports is null)
        {
            model.Error = error ?? "No se pudieron cargar los reportes asignados.";
            return View(model);
        }

        model.AssignedReports = _institutionRoutingService.GetAssignedReports(institution, reports);
        return View(model);
    }

    private InstitutionRegistrationViewModel BuildRegistrationModel(InstitutionApplication? application = null)
    {
        return new InstitutionRegistrationViewModel
        {
            InstitutionName = application?.InstitutionName ?? string.Empty,
            OfficialEmail = application?.OfficialEmail ?? string.Empty,
            OfficialDomain = application?.OfficialDomain ?? string.Empty,
            Department = application?.Department,
            ServiceZoneCode = application?.ServiceZoneCode ?? string.Empty,
            CoverageCategories = application?.CoverageCategories ?? [],
            ZoneOptions = _institutionDirectoryService.GetZoneOptions(application?.ServiceZoneCode)
        };
    }

    private void ValidateProofDocument(InstitutionRegistrationViewModel model)
    {
        if (model.ProofDocument is null || model.ProofDocument.Length == 0)
        {
            ModelState.AddModelError(nameof(model.ProofDocument), "Adjunta un documento de respaldo.");
            return;
        }

        if (!AllowedProofContentTypes.Contains(model.ProofDocument.ContentType))
        {
            ModelState.AddModelError(
                nameof(model.ProofDocument),
                "Solo se permiten documentos PDF o imagenes JPG, PNG o WEBP.");
        }

        if (model.ProofDocument.Length > 10 * 1024 * 1024)
        {
            ModelState.AddModelError(
                nameof(model.ProofDocument),
                "El documento no puede superar 10 MB.");
        }
    }

    private string GetProfileId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("El usuario autenticado no tiene identificador.");
    }
}
