using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VotingSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace VotingSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DbAb85acVotacionesdbContext _context;
        private readonly EmailNotificationController _emailNotificationController; // Inyectado

        public HomeController(ILogger<HomeController> logger,
                              DbAb85acVotacionesdbContext context,
                              EmailNotificationController emailNotificationController) // Modificado: IHttpClientFactory eliminado (si no se usa más), EmailNotificationController añadido
        {
            _logger = logger;
            _context = context;
            // _httpClientFactory = httpClientFactory;
            _emailNotificationController = emailNotificationController;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return View("~/Views/Home/Unauthorized.cshtml"); // Vista para usuarios no autenticados
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            {
                _logger.LogWarning("Claim NameIdentifier no encontrado o no es un entero válido. Cerrando sesión.");
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["ErrorMessage"] = "Error al identificar su sesión. Por favor, inicie sesión nuevamente.";
                return RedirectToAction("Index", "Account"); // Asume que Account/Index es tu página de login
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
            var ahora = DateTime.Now;

            var usuarioActual = await _context.Usuarios.FindAsync(userId);
            if (usuarioActual == null)
            {
                _logger.LogWarning($"Usuario con ID {userId} no encontrado en la base de datos. Cerrando sesión.");
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                TempData["ErrorMessage"] = "Su usuario no fue encontrado. Por favor, contacte al administrador.";
                return RedirectToAction("Index", "Account");
            }

            // Verificar restricciones
            var restricciones = await _context.Restriccions
                .Where(r => r.UsuarioId == userId &&
                            (r.FechaFin == null || r.FechaFin > ahora) &&
                            r.FechaInicio <= ahora)
                .ToListAsync();

            var tieneRestriccion = restricciones.Any();
            ViewBag.TieneRestriccion = tieneRestriccion; // Aunque redirija, es bueno tenerlo

            if (tieneRestriccion)
            {
                var restriccionInfo = restricciones.First(); // Para detalles en la notificación
                _logger.LogInformation($"Usuario ID {userId} ({usuarioActual.Email}) tiene restricción activa ID {restriccionInfo.Id}.");

                if (usuarioActual.Estado == "activo")
                {
                    usuarioActual.Estado = "inactivo";
                    _context.Usuarios.Update(usuarioActual);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Usuario ID {userId} ({usuarioActual.Email}) estado cambiado a 'inactivo' debido a restricción.");
                }

                // Notificar al administrador sobre la restricción
                string subject = $"Alerta: Usuario con Restricción Activa - {usuarioActual.NombreCompleto}";
                string messageBody = $@"El usuario <strong>{usuarioActual.NombreCompleto}</strong> (Email: {usuarioActual.Email}, ID: {userId}) tiene una restricción activa y su cuenta ha sido marcada como inactiva.
                                     <br><br><strong>Detalles de la restricción:</strong>
                                     <br>- Motivo: {restriccionInfo.Motivo}
                                     <br>- Tipo: {restriccionInfo.TipoRestriccion}
                                     <br>- Fecha de Inicio: {restriccionInfo.FechaInicio:dd/MM/yyyy HH:mm}
                                     <br>- Fecha de Fin: {(restriccionInfo.FechaFin.HasValue ? restriccionInfo.FechaFin.Value.ToString("dd/MM/yyyy HH:mm") : "Indefinida")}";
                try
                {
                    await _emailNotificationController.NotificarAAdministradoresActivosAsync(subject, messageBody); //
                    _logger.LogInformation($"Notificación de restricción para usuario ID {userId} enviada a administradores.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error al enviar notificación de restricción para usuario ID {userId} a administradores.");
                    // No detener el flujo por error de email, pero sí registrarlo.
                }

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme); //
                TempData["ErrorMessage"] = "Su cuenta presenta restricciones activas y ha sido desactivada. Por favor, contacte al administrador para más información."; //
                return RedirectToAction("Index", "Account"); // Redirigir a la página de login
            }

            // Si no hay restricción, cargar datos para el dashboard
            var votacionesHabilitadas = await _context.Votacions
                .Include(v => v.Asamblea)
                .Where(v => v.Estado == "en_curso" &&
                            v.FechaInicio <= ahora && v.FechaFin >= ahora &&
                            !_context.Votos.Any(vt => vt.VotacionId == v.Id && vt.UsuarioId == userId))
                .OrderByDescending(v => v.FechaInicio) // Mostrar las más recientes primero
                .ToListAsync(); //

            List<Votacion> votacionesProgramadas;
            if (userRole == "admin") //
            {
                votacionesProgramadas = await _context.Votacions
                    .Include(v => v.Asamblea)
                    .Where(v => v.Estado == "pendiente" || v.Estado == "programada") // Admins ven pendientes y programadas
                    .OrderBy(v => v.FechaInicio)
                    .ToListAsync();
            }
            else
            {
                votacionesProgramadas = await _context.Votacions
                    .Include(v => v.Asamblea)
                    .Where(v => v.Estado == "programada" && v.FechaInicio > ahora) // Usuarios ven solo programadas futuras
                    .OrderBy(v => v.FechaInicio)
                    .ToListAsync();
            }

            ViewBag.VotacionesHabilitadas = votacionesHabilitadas;
            ViewBag.VotacionesProgramadas = votacionesProgramadas;
            ViewBag.EsAdmin = userRole == "admin";
            ViewBag.Asambleas = await _context.Asambleas
                .Where(a => a.Estado != "finalizada" && a.Estado != "cancelada") // Asambleas activas o próximas
                .OrderByDescending(a => a.Fecha)
                .ToListAsync();
            ViewBag.NombreUsuario = usuarioActual.Nombre; // Para el saludo

            return View("~/Views/Home/Index.cshtml");
        }

        public IActionResult Privacy()
        {
            return View("~/Views/Home/Privacy.cshtml");
        }

        public IActionResult About()
        {
            return View("~/Views/Home/About.cshtml");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}