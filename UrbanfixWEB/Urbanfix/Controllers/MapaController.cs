using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Urbanfix.Models;

namespace Urbanfix.Controllers;

public class MapaController : Controller
{
    private readonly MapSettings _mapSettings;

    public MapaController(IOptions<MapSettings> mapSettings)
    {
        _mapSettings = mapSettings.Value;
    }

    public IActionResult Index()
    {
        ViewBag.DefaultLat = _mapSettings.DefaultLat;
        ViewBag.DefaultLng = _mapSettings.DefaultLng;
        ViewBag.DefaultZoom = _mapSettings.DefaultZoom;
        return View();
    }
}
