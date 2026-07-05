using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Urbanfix.Models;
using Urbanfix.Services;

namespace Urbanfix.Controllers;

[Authorize]
public class ReportarController : Controller
{
    private readonly ReportApiService _reportApiService;
    private readonly MapSettings _mapSettings;

    public ReportarController(ReportApiService reportApiService, IOptions<MapSettings> mapSettings)
    {
        _reportApiService = reportApiService;
        _mapSettings = mapSettings.Value;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var model = new ReportFormViewModel
        {
            Latitud = (decimal)_mapSettings.DefaultLat,
            Longitud = (decimal)_mapSettings.DefaultLng
        };

        ViewBag.DefaultLat = _mapSettings.DefaultLat;
        ViewBag.DefaultLng = _mapSettings.DefaultLng;
        ViewBag.DefaultZoom = _mapSettings.DefaultZoom;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ReportFormViewModel model, CancellationToken cancellationToken)
    {
        ViewBag.DefaultLat = _mapSettings.DefaultLat;
        ViewBag.DefaultLng = _mapSettings.DefaultLng;
        ViewBag.DefaultZoom = _mapSettings.DefaultZoom;

        if (model.Imagen is { Length: > 0 })
        {
            var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!allowedTypes.Contains(model.Imagen.ContentType))
            {
                ModelState.AddModelError(nameof(model.Imagen), "Solo se permiten imagenes JPG, PNG o WEBP.");
            }

            if (model.Imagen.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError(nameof(model.Imagen), "La imagen no puede superar 5 MB.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, error) = await _reportApiService.EnviarReporteAsync(model, cancellationToken);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "No se pudo enviar el reporte.");
            return View(model);
        }

        TempData["Success"] = "Reporte enviado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
