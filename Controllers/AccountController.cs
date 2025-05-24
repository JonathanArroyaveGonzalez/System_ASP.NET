using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View("~/Views/Login/Index.cshtml", model);

        try
        {
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
                ModelState.AddModelError("", "Contrase�a incorrecta.");
                return View("~/Views/Login/Index.cshtml", model);
            }

            // Verificar restricciones activas
            var ahora = DateTime.Now;
            var tieneRestriccion = await _context.Restriccions
                .AnyAsync(r => r.UsuarioId == usuario.Id &&
                       (r.FechaFin == null || r.FechaFin > ahora) &&
                       r.FechaInicio <= ahora);

            if (tieneRestriccion)
            {
                // Actualizar estado del usuario
                usuario.Estado = "inactivo";
                await _context.SaveChangesAsync();

                ModelState.AddModelError("", "Su cuenta tiene restricciones activas y ha sido desactivada. Contacte al administrador.");
                return View("~/Views/Login/Index.cshtml", model);
            }

            // Actualizar �ltimo acceso
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

            _logger.LogInformation($"Usuario {usuario.Email} ha iniciado sesi�n.");

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el proceso de login");
            ModelState.AddModelError("", "Ocurri� un error al intentar iniciar sesi�n.");
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
                ModelState.AddModelError("Email", "El correo ya est� registrado.");
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

            // Autenticar directamente despu�s del registro
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
            ModelState.AddModelError("", "Ocurri� un error al registrar el usuario.");
            return View("~/Views/Login/Register.cshtml", model);
        }
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        _logger.LogInformation($"Usuario {User.Identity.Name} ha cerrado sesi�n.");
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
                ModelState.AddModelError("Email", "El correo ya est� registrado.");
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
            ModelState.AddModelError("", "Ocurri� un error al crear el administrador.");
            return View(model);
        }
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var usuario = await _context.Usuarios.FindAsync(userId);

        if (usuario == null)
        {
            return NotFound();
        }

        var profileViewModel = new ProfileViewModel
        {
            Id = usuario.Id,
            Nombre = usuario.Nombre,
            Apellido = usuario.Apellido,
            Email = usuario.Email,
            FechaRegistro = usuario.FechaRegistro ?? DateTime.MinValue,
            UltimoAcceso = usuario.UltimoAcceso
        };

        // Obtener historial de votaciones del usuario
        var votacionesParticipadas = await _context.Votos
            .Where(v => v.UsuarioId == userId)
            .Include(v => v.Votacion)
            .ThenInclude(v => v.Asamblea)
            .Include(v => v.Opcion)
            .OrderByDescending(v => v.Fecha)
            .Take(10)
            .ToListAsync();

        ViewBag.Votaciones = votacionesParticipadas;

        // Verificar si el usuario tiene restricciones activas
        var tieneRestriccion = await _context.Restriccions
            .AnyAsync(r => r.UsuarioId == userId &&
                    (r.FechaFin == null || r.FechaFin >= DateTime.Now) &&
                    r.FechaInicio <= DateTime.Now);

        ViewBag.TieneRestriccion = tieneRestriccion;
        
        // Obtener restricciones activas para mostrarlas
        var restriccionesActivas = await _context.Restriccions
            .Where(r => r.UsuarioId == userId &&
                    (r.FechaFin == null || r.FechaFin >= DateTime.Now) &&
                    r.FechaInicio <= DateTime.Now)
            .Include(r => r.CreadoPorNavigation)
            .ToListAsync();
            
        ViewBag.RestriccionesActivas = restriccionesActivas;

        return View(profileViewModel);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Profile", model);
        }

        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var usuario = await _context.Usuarios.FindAsync(userId);

        if (usuario == null)
        {
            return NotFound();
        }

        // Si el correo cambia, verificar que no exista
        if (usuario.Email != model.Email &&
            await _context.Usuarios.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError("Email", "Este correo electr�nico ya est� en uso.");
            return View("Profile", model);
        }

        // Actualizar datos b�sicos
        usuario.Nombre = model.Nombre;
        usuario.Apellido = model.Apellido;
        usuario.Email = model.Email;

        // Cambiar contrase�a si se proporcion� una nueva
        if (!string.IsNullOrEmpty(model.NewPassword))
        {
            if (string.IsNullOrEmpty(model.CurrentPassword))
            {
                ModelState.AddModelError("CurrentPassword", "Debe proporcionar la contrase�a actual.");
                return View("Profile", model);
            }

            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, usuario.Password))
            {
                ModelState.AddModelError("CurrentPassword", "La contrase�a actual es incorrecta.");
                return View("Profile", model);
            }

            usuario.Password = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
        }

        await _context.SaveChangesAsync();

        // Actualizar las reclamaciones de identidad
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

        TempData["SuccessMessage"] = "Perfil actualizado exitosamente.";
        return RedirectToAction(nameof(Profile));
    }

    //Metodo para promover a admin
    // GET: Account/PromoteToAdmin
    [Authorize(Roles = "admin")] // Solo los administradores pueden acceder
    [HttpGet]
    public async Task<IActionResult> PromoteToAdmin()
    {
        // Obtener usuarios que no son administradores y est�n activos
        // para evitar promover usuarios inactivos o que ya son admins.
        var nonAdminUsers = await _context.Usuarios
            .Where(u => u.Rol != "admin" && u.Estado == "activo")
            .OrderBy(u => u.Apellido)
            .ThenBy(u => u.Nombre)
            .Select(u => new {
                u.Id,
                // Mostramos NombreCompleto y Email para mejor identificaci�n en el dropdown
                DisplayName = u.NombreCompleto + " (" + u.Email + ")"
            })
            .ToListAsync();

        if (!nonAdminUsers.Any())
        {
            TempData["InfoMessage"] = "No hay usuarios activos disponibles para promover a administrador en este momento.";
        }

        ViewBag.UserList = new SelectList(nonAdminUsers, "Id", "DisplayName");
        // La vista se ubicar� en Views/Account/NewAdmin.cshtml como solicitaste
        return View("~/Views/Account/NewAdmin.cshtml");
    }

    // POST: Account/PromoteToAdmin
    [Authorize(Roles = "admin")] // Solo los administradores pueden ejecutar esta acci�n
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PromoteToAdmin(int selectedUserId) // El nombre del par�metro debe coincidir con el 'name' del select en el form
    {
        if (selectedUserId <= 0) // Validaci�n b�sica del ID seleccionado
        {
            ModelState.AddModelError("selectedUserId", "Por favor, seleccione un usuario v�lido de la lista.");
        }

        Usuario? userToPromote = null; // Declarar fuera del if para usarlo despu�s

        if (ModelState.IsValid) // Procede solo si la validaci�n b�sica del ID es correcta
        {
            userToPromote = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == selectedUserId);

            if (userToPromote == null)
            {
                ModelState.AddModelError("", "El usuario seleccionado no fue encontrado en la base de datos.");
            }
            else if (userToPromote.Rol == "admin")
            {
                ModelState.AddModelError("", $"El usuario '{userToPromote.NombreCompleto}' ya es un administrador.");
            }
            else if (userToPromote.Estado != "activo") // Doble chequeo, aunque el GET ya filtra
            {
                ModelState.AddModelError("", $"El usuario '{userToPromote.NombreCompleto}' no est� activo y no puede ser promovido.");
            }
        }

        if (ModelState.IsValid && userToPromote != null) // Asegurarse que userToPromote no sea null
        {
            try
            {
                userToPromote.Rol = "admin"; // Cambiar el rol del usuario
                _context.Usuarios.Update(userToPromote);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Usuario {userToPromote.Email} (ID: {userToPromote.Id}) fue promovido a administrador por {User.Identity?.Name}.");
                TempData["SuccessMessage"] = $"El usuario '{userToPromote.NombreCompleto}' ha sido promovido a administrador exitosamente.";

                // Redirigir a una p�gina relevante, por ejemplo, la lista de usuarios si existe, o al home.
                // Si tienes un UsuariosController con una acci�n Index:
                return RedirectToAction("Index", "Usuarios");
                // Si no, puedes redirigir a Home o a la misma p�gina de promover:
                // return RedirectToAction(nameof(PromoteToAdmin));
            }
            catch (DbUpdateException ex) // Ser m�s espec�fico con la excepci�n si es posible
            {
                _logger.LogError(ex, $"Error de base de datos al promover usuario ID {selectedUserId} a administrador.");
                ModelState.AddModelError("", "Ocurri� un error al actualizar la base de datos. Int�ntelo de nuevo.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error general al promover usuario ID {selectedUserId} a administrador.");
                ModelState.AddModelError("", "Ocurri� un error inesperado al intentar promover el usuario.");
            }
        }

        // Si ModelState no es v�lido o hubo un error, recargar la lista y mostrar la vista de nuevo
        var nonAdminUsers = await _context.Usuarios
            .Where(u => u.Rol != "admin" && u.Estado == "activo")
            .OrderBy(u => u.Apellido)
            .ThenBy(u => u.Nombre)
            .Select(u => new { u.Id, DisplayName = u.NombreCompleto + " (" + u.Email + ")" })
            .ToListAsync();

        // Mantener el valor seleccionado si hubo un error para no perder la selecci�n del usuario
        ViewBag.UserList = new SelectList(nonAdminUsers, "Id", "DisplayName", selectedUserId);

        return View("~/Views/Account/NewAdmin.cshtml");
    }
}