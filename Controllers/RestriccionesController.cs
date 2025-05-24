using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VotingSystem.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Collections.Generic; // Necesario para List<SelectListItem>

namespace VotingSystem.Controllers
{
    [Authorize(Roles = "admin")]
    public class RestriccionesController : Controller
    {
        private readonly DbAb85acVotacionesdbContext _context;
        private readonly ILogger<RestriccionesController> _logger; // Inyectar ILogger

        public RestriccionesController(DbAb85acVotacionesdbContext context, ILogger<RestriccionesController> logger) // Añadir ILogger
        {
            _context = context;
            _logger = logger;
        }

        // GET: Restricciones
        public async Task<IActionResult> Index()
        {
            var restricciones = await _context.Restriccions
                .Include(r => r.Usuario)
                .Include(r => r.CreadoPorNavigation)
                .OrderByDescending(r => r.FechaInicio)
                .ToListAsync();

            return View(restricciones);
        }

        // GET: Restricciones/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restriccion = await _context.Restriccions
                .Include(r => r.Usuario)
                .Include(r => r.CreadoPorNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (restriccion == null)
            {
                return NotFound();
            }

            return View(restriccion);
        }

        private void PopulateDropdowns(Restriccion? restriccion = null)
        {
            ViewData["UsuarioId"] = new SelectList(
                _context.Usuarios.Where(u => u.Rol != "admin").OrderBy(u => u.Nombre).ThenBy(u => u.Apellido),
                "Id",
                "NombreCompleto",
                restriccion?.UsuarioId);

            var tiposRestriccionData = new List<string> { "Morosidad", "Incumplimiento", "Suspensión temporal", "Otra" };
            ViewData["TiposRestriccionList"] = new SelectList(tiposRestriccionData, restriccion?.TipoRestriccion);
        }

        // GET: Restricciones/Create
        public IActionResult Create()
        {
            PopulateDropdowns();
            return View("~/Views/Restricciones/Create.cshtml");
        }

        // POST: Restricciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UsuarioId,TipoRestriccion,FechaInicio,FechaFin,Motivo")] Restriccion restriccion)
        {
            string? adminUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(adminUserIdClaim) || !int.TryParse(adminUserIdClaim, out int adminUserId))
            {
                ModelState.AddModelError("", "No se pudo identificar al administrador. Por favor, inicie sesión nuevamente.");
                // No continuar si no se puede identificar al creador.
            }
            else
            {
                restriccion.CreadoPor = adminUserId;
            }

            // Remover validación de propiedades de navegación que no se enlazan directamente
            ModelState.Remove("Usuario");
            ModelState.Remove("CreadoPorNavigation");

            if (ModelState.IsValid)
            {
                _context.Add(restriccion);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Restricción creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("ModelState inválido al crear restricción.");
            foreach (var error in ModelState)
            {
                _logger.LogWarning($"Campo: {error.Key}, Errores: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }

            PopulateDropdowns(restriccion);
            return View("~/Views/Restricciones/Create.cshtml",restriccion);
        }

        // GET: Restricciones/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restriccion = await _context.Restriccions.FindAsync(id);
            if (restriccion == null)
            {
                return NotFound();
            }

            PopulateDropdowns(restriccion);
            return View("~/Views/Restricciones/Edit.cshtml", restriccion);
        }

        // POST: Restricciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UsuarioId,TipoRestriccion,FechaInicio,FechaFin,Motivo")] Restriccion restriccionViewModel) // CreadoPor eliminado de Bind
        {
            if (id != restriccionViewModel.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Usuario");
            ModelState.Remove("CreadoPorNavigation");

            if (ModelState.IsValid)
            {
                var restriccionToUpdate = await _context.Restriccions.FindAsync(id);
                if (restriccionToUpdate == null)
                {
                    return NotFound();
                }

                // Actualizar propiedades de la entidad rastreada
                restriccionToUpdate.UsuarioId = restriccionViewModel.UsuarioId;
                restriccionToUpdate.TipoRestriccion = restriccionViewModel.TipoRestriccion;
                restriccionToUpdate.FechaInicio = restriccionViewModel.FechaInicio;
                restriccionToUpdate.FechaFin = restriccionViewModel.FechaFin;
                restriccionToUpdate.Motivo = restriccionViewModel.Motivo;
                // CreadoPor NO se actualiza desde el ViewModel, mantiene su valor original.

                try
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Restricción actualizada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    _logger.LogError(ex, "Error de concurrencia al editar restricción ID {RestriccionId}", restriccionToUpdate.Id);
                    if (!RestriccionExists(restriccionViewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "El registro que intentó editar fue modificado por otro usuario después de que obtuvo el valor original. La operación de edición fue cancelada. Inténtelo de nuevo.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al actualizar restricción ID {RestriccionId}", restriccionToUpdate.Id);
                    ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar los cambios. Inténtelo de nuevo.");
                }
            }

            _logger.LogWarning("ModelState inválido al editar restricción ID {RestriccionId}", id);
            foreach (var error in ModelState)
            {
                _logger.LogWarning($"Campo: {error.Key}, Errores: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
            }

            PopulateDropdowns(restriccionViewModel);
            return View(restriccionViewModel);
        }

        // GET: Restricciones/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restriccion = await _context.Restriccions
                .Include(r => r.Usuario)
                .Include(r => r.CreadoPorNavigation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (restriccion == null)
            {
                return NotFound();
            }

            return View("~/Views/Restricciones/Delete.cshtml", restriccion);
        }

        // POST: Restricciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var restriccion = await _context.Restriccions.FindAsync(id);
            if (restriccion == null) // Añadir verificación por si se elimina entre GET y POST
            {
                TempData["ErrorMessage"] = "La restricción que intentó eliminar ya no existe.";
                return RedirectToAction(nameof(Index));
            }
            _context.Restriccions.Remove(restriccion);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Restricción eliminada exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        private bool RestriccionExists(int id)
        {
            return _context.Restriccions.Any(e => e.Id == id);
        }
    }
}