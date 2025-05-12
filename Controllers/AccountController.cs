using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Generators;
using System.Security.Claims;
using VotingSystem.Models;

public class AccountController : Controller
{
    private readonly DbAb85acVotacionesdbContext _context;
    private readonly ILogger<AccountController> _logger;

    public AccountController(DbAb85acVotacionesdbContext context, ILogger<AccountController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }
        return View("~/Views/Login/Index.cshtml");
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Login/Index.cshtml", model);

        try
        {
            //var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == model.Email && u.Estado == "activo");

            //Prueba login
            if (!ModelState.IsValid)
                return View("~/Views/Login/Index.cshtml", model);

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (usuario == null)
            {
                ModelState.AddModelError("", "El usuario no existe.");
                return View("~/Views/Login/Index.cshtml", model);
            }

            if (usuario.Estado != "activo")
            {
                ModelState.AddModelError("", "Cuenta inactiva. Contacte al administrador.");
                return View("~/Views/Login/Index.cshtml", model);
            }

            if (!BCrypt.Net.BCrypt.Verify(model.Password, usuario.Password))
            {
                ModelState.AddModelError("", "Contraseña incorrecta.");
                return View("~/Views/Login/Index.cshtml", model);
            }

            //Prueba Login

            if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.Password, usuario.Password))
            {
                ModelState.AddModelError("", "Credenciales inválidas o cuenta inactiva.");
                return View("~/Views/Login/Index.cshtml", model);
            }


            // Actualizar último acceso
            usuario.UltimoAcceso = DateTime.Now;
            await _context.SaveChangesAsync();

            // Crear claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
                });

            _logger.LogInformation($"Usuario {usuario.Email} ha iniciado sesión.");

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el proceso de login");
            ModelState.AddModelError("", "Ocurrió un error al intentar iniciar sesión.");
            return View("~/Views/Login/Index.cshtml", model);
        }
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }
        return View("~/Views/Login/Register.cshtml");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Login/Register.cshtml", model);

        try
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "El correo ya está registrado.");
                return View("~/Views/Login/Register.cshtml", model);
            }

            var usuario = new Usuario
            {
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                Email = model.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Rol = "propietario", // Todos los registrados son propietarios
                Estado = "activo",
                FechaRegistro = DateTime.Now
            };

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Autenticar directamente después del registro
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity));

            _logger.LogInformation($"Nuevo usuario registrado: {usuario.Email}");

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el registro de usuario");
            ModelState.AddModelError("", "Ocurrió un error al registrar el usuario.");
            return View("~/Views/Login/Register.cshtml", model);
        }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        //await HttpContext.SignOutAsync();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        _logger.LogInformation($"Usuario {User.Identity.Name} ha cerrado sesión.");
        return RedirectToAction("Index", "Home");
    }

    [Authorize(Roles = "admin")]
    [HttpGet]
    public IActionResult CreateAdmin()
    {
        return View();
    }

    [Authorize(Roles = "admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAdmin(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            if (await _context.Usuarios.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "El correo ya está registrado.");
                return View(model);
            }

            var admin = new Usuario
            {
                Nombre = model.Nombre,
                Apellido = model.Apellido,
                Email = model.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Rol = "admin",
                Estado = "activo",
                FechaRegistro = DateTime.Now
            };

            _context.Usuarios.Add(admin);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Administrador creado exitosamente.";
            return RedirectToAction("Index", "Usuarios");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear administrador");
            ModelState.AddModelError("", "Ocurrió un error al crear el administrador.");
            return View(model);
        }
    }
}