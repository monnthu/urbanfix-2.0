using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Urbanfix.Models;
using Urbanfix.Services;

namespace Urbanfix.Controllers;

public class AccountController : Controller
{
    private readonly SupabaseAuthService _authService;

    public AccountController(SupabaseAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, error) = await _authService.RegisterAsync(model, cancellationToken);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "No se pudo completar el registro.");
            return View(model);
        }

        TempData["Success"] = "Cuenta creada correctamente. Ya puedes iniciar sesion.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToLocal(returnUrl);
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var (success, session, error) = await _authService.LoginAsync(
            model.Email,
            model.Password,
            cancellationToken);

        if (!success || session is null)
        {
            ModelState.AddModelError(string.Empty, error ?? "No se pudo iniciar sesion.");
            return View(model);
        }

        var profile = await _authService.GetProfileAsync(session.AccessToken, cancellationToken);
        await SignInUserAsync(session, profile);

        return RedirectToLocal(model.ReturnUrl);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task SignInUserAsync(AuthSession session, ProfileDto? profile)
    {
        var role = profile?.Role ?? ProfileRoles.Civilian;
        var displayName = profile?.FullName ?? session.Email;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, session.UserId),
            new(ClaimTypes.Email, session.Email),
            new(ClaimTypes.Name, displayName),
            new(ClaimTypes.Role, role),
            new("access_token", session.AccessToken)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);
        var expiresAt = DateTimeOffset.UtcNow.AddSeconds(session.ExpiresIn);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = expiresAt
            });
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }
}
