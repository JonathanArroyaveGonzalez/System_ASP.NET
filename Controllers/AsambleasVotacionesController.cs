using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Icao;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VotingSystem.Models;
using System.Security.Claims; 
using Microsoft.AspNetCore.Authorization;

namespace VotingSystem.Controllers
{
    public class AsambleasVotacionesController : Controller
    {
        private readonly DbAb85acVotacionesdbContext _context;
        private readonly EmailNotificationController _emailNotificationController; // INJECTED

        public AsambleasVotacionesController(DbAb85acVotacionesdbContext context, EmailNotificationController emailNotificationController)
        {
            _context = context;
            _emailNotificationController = emailNotificationController; // INITIALIZED
        }

        // ASAMBLEAS

        // GET: Asambleas
        public async Task<IActionResult> IndexAsambleas()
        {
            return View("~/Views/Asambleas/IndexAsambleas.cshtml", await _context.Asambleas.Include(a => a.Creador).ToListAsync());
        }


        // GET: Asambleas/Create
        public IActionResult CreateAsamblea()
        {
            ViewBag.CreadorId = new SelectList(
                _context.Usuarios.Where(u => u.Rol == "admin").Select(u => new {
                    Id = u.Id,
                    NombreCompleto = $"{u.Nombre} {u.Apellido}"
                }).ToList(),
                "Id",
                "NombreCompleto"
            );
            return View("~/Views/Asambleas/CreateAsamblea.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsamblea([Bind("Titulo,Descripcion,Fecha,Estado,CreadorId,Acta")] Asamblea asamblea)
        {
            asamblea.FechaCreacion = DateTime.Now;
            ModelState.Remove("Creador");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(asamblea);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(IndexAsambleas));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al guardar asamblea: {ex.Message}"); // Log error
                    ModelState.AddModelError("", "Error al guardar la asamblea. Por favor, inténtelo de nuevo.");
                }
            }
            
            ViewBag.CreadorId = new SelectList( 
                _context.Usuarios.Where(u => u.Rol == "admin").Select(u => new { 
                    Id = u.Id,
                    NombreCompleto = $"{u.Nombre} {u.Apellido}"
                }).ToList(),
                "Id",
                "NombreCompleto",
                asamblea.CreadorId
            );
            return View("~/Views/Asambleas/CreateAsamblea.cshtml", asamblea);
        }



        // GET: Asambleas/Edit/5
        public async Task<IActionResult> EditAsamblea(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asamblea = await _context.Asambleas.FindAsync(id);
            if (asamblea == null)
            {
                return NotFound();
            }
            ViewData["CreadorId"] = _context.Usuarios.Select(u => new { u.Id, NombreCompleto = $"{u.Nombre} {u.Apellido}" }).ToList();
            return View("~/Views/Asambleas/EditAsamblea.cshtml", asamblea);
        }

        // POST: Asambleas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAsamblea(int id, [Bind("Id,Titulo,Descripcion,Fecha,Estado,CreadorId,Acta")] Asamblea asamblea)
        {
            if (id != asamblea.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(asamblea);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AsambleaExists(asamblea.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(IndexAsambleas));
            }
            ViewData["CreadorId"] = _context.Usuarios.Select(u => new { u.Id, NombreCompleto = $"{u.Nombre} {u.Apellido}" }).ToList();
            return View("~/Views/Asambleas/EditAsamblea.cshtml",asamblea);
        }

        // GET: Asambleas/Delete/5
        public async Task<IActionResult> DeleteAsamblea(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var asamblea = await _context.Asambleas
                .Include(a => a.Creador)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (asamblea == null)
            {
                return NotFound();
            }

            return View("~/Views/Asambleas/DeleteAsamblea.cshtml",asamblea);
        }

        // POST: Asambleas/Delete/5
        [HttpPost, ActionName("DeleteAsamblea")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAsambleaConfirmed(int id)
        {
            var asamblea = await _context.Asambleas.FindAsync(id);
            _context.Asambleas.Remove(asamblea);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(IndexAsambleas));
        }

        // VOTACIONES

        // GET: Votaciones
        public async Task<IActionResult> IndexVotaciones()
        {
            var votaciones = await _context.Votacions
                .Include(v => v.Asamblea)
                .ToListAsync();
            return View("~/Views/Votaciones/IndexVotaciones.cshtml", votaciones);
        }
       
        // GET: Votaciones/Create
        public IActionResult CreateVotacion()
        {
            // Crear ViewBag correctamente para el SelectList
            ViewBag.AsambleaId = new SelectList(
                _context.Asambleas.Select(a => new {
                    Id = a.Id,
                    Titulo = a.Titulo
                }).ToList(),
                "Id",
                "Titulo"
            );

            return View("~/Views/Votaciones/CreateVotacion.cshtml");
        }

        // POST: Votaciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVotacion([Bind("Titulo,Descripcion,TipoVotacion,FechaInicio,FechaFin,QuorumRequerido,AsambleaId,Estado")] Votacion votacion, List<string> OpcionTexto, List<string> OpcionDescripcion)
        {
            ModelState.Remove("Asamblea");
            ModelState.Remove("OpcionVotacions");
            ModelState.Remove("Votos");

            if (OpcionTexto == null || !OpcionTexto.Any(x => !string.IsNullOrWhiteSpace(x)) || OpcionTexto.Count(x => !string.IsNullOrWhiteSpace(x)) < 2)
            {
                ModelState.AddModelError("", "Debe agregar al menos dos opciones de votación válidas.");
            }

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _context.Add(votacion);
                    await _context.SaveChangesAsync(); // Save Votacion to get its ID

                    var opciones = new List<OpcionVotacion>();
                    for (int i = 0; i < OpcionTexto.Count; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(OpcionTexto[i]))
                        {
                            opciones.Add(new OpcionVotacion
                            {
                                VotacionId = votacion.Id, // Use the generated VotacionId
                                Texto = OpcionTexto[i].Trim(),
                                Descripcion = (OpcionDescripcion != null && i < OpcionDescripcion.Count) ? OpcionDescripcion[i]?.Trim() : null,
                                Orden = opciones.Count + 1
                            });
                        }
                    }
                    _context.OpcionVotacions.AddRange(opciones);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Send email notification using the new controller
                    try
                    {
                        var asamblea = await _context.Asambleas.FindAsync(votacion.AsambleaId);
                        var usuariosActivos = await _context.Usuarios.Where(u => u.Estado == "activo").ToListAsync();

                        if (usuariosActivos.Any())
                        {
                            // Pass the loaded asamblea object
                            await _emailNotificationController.NotificarNuevaVotacionAsync(votacion, usuariosActivos, asamblea);
                            Console.WriteLine("Notificación de nueva votación enviada exitosamente.");
                        }
                        else
                        {
                            Console.WriteLine("No hay usuarios activos para notificar sobre la nueva votación.");
                        }
                        TempData["Success"] = $"Votación '{votacion.Titulo}' creada exitosamente con {opciones.Count} opciones.";
                    }
                    catch (Exception emailEx)
                    {
                        Console.WriteLine($"Error al enviar notificación por correo: {emailEx.Message}");
                        // Log the full exception: Console.WriteLine(emailEx.ToString());
                        TempData["Warning"] = "La votación se creó correctamente, pero hubo un problema al enviar las notificaciones por correo.";
                    }

                    return RedirectToAction(nameof(IndexVotaciones));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Error al crear la votación: {ex.Message}");
                    // Log the full exception: Console.WriteLine(ex.ToString());
                    ModelState.AddModelError("", $"Error al guardar la votación: {ex.InnerException?.Message ?? ex.Message}");
                }
            }

            // If ModelState is invalid or an error occurred, repopulate ViewBag and return view
            ViewBag.AsambleaId = new SelectList(
                _context.Asambleas.Select(a => new { a.Id, a.Titulo }).ToList(),
                "Id", "Titulo", votacion.AsambleaId
            );
            return View("~/Views/Votaciones/CreateVotacion.cshtml", votacion);
        }


        // GET: AsambleasVotaciones/EditVotacion/5
        public async Task<IActionResult> EditVotacion(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var votacion = await _context.Votacions
                                         //.Include(v => v.OpcionVotacions) // Descomenta si necesitas editar opciones en el futuro
                                         .FirstOrDefaultAsync(v => v.Id == id);

            if (votacion == null)
            {
                return NotFound();
            }

            // Para el dropdown de Asambleas
            ViewBag.AsambleaIdList = new SelectList(_context.Asambleas.OrderBy(a => a.Titulo).ToList(), "Id", "Titulo", votacion.AsambleaId);

            // Para el dropdown de Estado
            var estadosPosibles = new List<SelectListItem>
    {
        new SelectListItem { Value = "Pendiente", Text = "Pendiente" },
        new SelectListItem { Value = "Programada", Text = "Programada" },
        new SelectListItem { Value = "En Curso", Text = "En Curso" }, // Asegúrate que coincida con el valor "en_curso" si es diferente
        new SelectListItem { Value = "Finalizada", Text = "Finalizada" },
        new SelectListItem { Value = "Cancelada", Text = "Cancelada" }
    };
            // Si tu estado "en_curso" se guarda como "en_curso" y no "En Curso", ajústalo:
            // new SelectListItem { Value = "en_curso", Text = "En Curso" },
            ViewBag.EstadoList = new SelectList(estadosPosibles, "Value", "Text", votacion.Estado);

            return View("~/Views/Votaciones/EditVotacion.cshtml", votacion);
        }

        // POST: AsambleasVotaciones/EditVotacion/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVotacion(int id,
            [Bind("Id,Titulo,Descripcion,TipoVotacion,FechaInicio,FechaFin,QuorumRequerido,AsambleaId,Estado")] Votacion votacionViewModel)
        {
            if (id != votacionViewModel.Id)
            {
                return NotFound();
            }

            // Estas propiedades de navegación no se están editando directamente en este formulario.
            ModelState.Remove("Asamblea");
            ModelState.Remove("OpcionVotacions");
            ModelState.Remove("Votos");

            if (ModelState.IsValid)
            {
                try
                {
                    var votacionToUpdate = await _context.Votacions.FindAsync(id);
                    if (votacionToUpdate == null)
                    {
                        return NotFound();
                    }

                    // Actualizar las propiedades de la entidad recuperada con los valores del ViewModel
                    votacionToUpdate.Titulo = votacionViewModel.Titulo;
                    votacionToUpdate.Descripcion = votacionViewModel.Descripcion;
                    votacionToUpdate.TipoVotacion = votacionViewModel.TipoVotacion;
                    votacionToUpdate.FechaInicio = votacionViewModel.FechaInicio;
                    votacionToUpdate.FechaFin = votacionViewModel.FechaFin;
                    votacionToUpdate.QuorumRequerido = votacionViewModel.QuorumRequerido;
                    votacionToUpdate.AsambleaId = votacionViewModel.AsambleaId;
                    votacionToUpdate.Estado = votacionViewModel.Estado; // Actualizar el estado

                    // _context.Update(votacionToUpdate); // No es estrictamente necesario si la entidad ya está siendo rastreada
                    // y modificas sus propiedades. SaveChanges detectará los cambios.
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Votación actualizada exitosamente.";
                    return RedirectToAction(nameof(IndexVotaciones));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VotacionExists(votacionViewModel.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        ModelState.AddModelError("", "Los datos fueron modificados por otro usuario. Por favor, recargue la página e intente de nuevo.");
                    }
                }
                catch (Exception ex)
                {
                    // Considera usar ILogger para registrar la excepción completa
                    Console.WriteLine($"Error al actualizar votación: {ex.ToString()}");
                    ModelState.AddModelError("", $"Error al actualizar la votación: {ex.Message}");
                }
            }

            // Si ModelState no es válido o hubo un error, recargar los SelectLists para la vista
            ViewBag.AsambleaIdList = new SelectList(_context.Asambleas.OrderBy(a => a.Titulo).ToList(), "Id", "Titulo", votacionViewModel.AsambleaId);
            var estadosPosibles = new List<SelectListItem>
    {
        new SelectListItem { Value = "Pendiente", Text = "Pendiente" },
        new SelectListItem { Value = "Programada", Text = "Programada" },
        new SelectListItem { Value = "En Curso", Text = "En Curso" },
        new SelectListItem { Value = "Finalizada", Text = "Finalizada" },
        new SelectListItem { Value = "Cancelada", Text = "Cancelada" }
    };
            ViewBag.EstadoList = new SelectList(estadosPosibles, "Value", "Text", votacionViewModel.Estado);

            return View("~/Views/Votaciones/EditVotacion.cshtml", votacionViewModel);
        }

        // GET: Votaciones/Delete/5
        public async Task<IActionResult> DeleteVotacion(int? id)
        {
            if (id == null) return NotFound();
            var votacion = await _context.Votacions
                .Include(v => v.Asamblea)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (votacion == null) return NotFound();
            return View("~/Views/Votaciones/DeleteVotacion.cshtml", votacion);
        }

        // POST: Votaciones/Delete/5
        [HttpPost, ActionName("DeleteVotacion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVotacionConfirmed(int id)
        {
            var votacion = await _context.Votacions.FindAsync(id);
            if (votacion == null) return NotFound();
            _context.Votacions.Remove(votacion);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Votación eliminada exitosamente.";
            return RedirectToAction(nameof(IndexVotaciones));
        }

        private bool AsambleaExists(int id)
        {
            return _context.Asambleas.Any(e => e.Id == id);
        }

        private bool VotacionExists(int id)
        {
            return _context.Votacions.Any(e => e.Id == id);
        }

        //Metodos Para el registro de votos 
        // GET: AsambleasVotaciones/UserVoting/5
        [Authorize] // Asegura que solo usuarios autenticados puedan acceder
        [HttpGet]
        public async Task<IActionResult> UserVoting(int? votacionId)
        {
            if (votacionId == null)
            {
                TempData["ErrorMessage"] = "No se especificó una votación.";
                return RedirectToAction("Index", "Home");
            }

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                TempData["ErrorMessage"] = "No se pudo identificar al usuario.";
                return RedirectToAction("Index", "Account"); // O a donde manejes el login
            }

            var ahora = DateTime.Now;

            // 1. Verificar restricciones del usuario (similar a HomeController)
            var tieneRestriccion = await _context.Restriccions
                .AnyAsync(r => r.UsuarioId == userId &&
                               (r.FechaFin == null || r.FechaFin > ahora) &&
                               r.FechaInicio <= ahora);

            if (tieneRestriccion)
            {
                TempData["ErrorMessage"] = "Usted tiene restricciones activas y no puede votar en este momento.";
                return RedirectToAction("Index", "Home");
            }

            // 2. Obtener la votación, incluyendo sus opciones y la asamblea asociada
            var votacion = await _context.Votacions
                .Include(v => v.Asamblea)
                .Include(v => v.OpcionVotacions) // Asegúrate de que esta propiedad de navegación exista y esté bien configurada
                .FirstOrDefaultAsync(v => v.Id == votacionId);

            if (votacion == null)
            {
                TempData["ErrorMessage"] = "La votación solicitada no fue encontrada.";
                return RedirectToAction("Index", "Home");
            }

            // 3. Verificar si la votación está activa
            if (votacion.Estado != "en_curso" || !(votacion.FechaInicio <= ahora && votacion.FechaFin >= ahora))
            {
                TempData["InfoMessage"] = $"La votación '{votacion.Titulo}' no está activa actualmente.";
                return RedirectToAction("Index", "Home");
            }

            // 4. Verificar si el usuario ya votó en esta votación
            var yaVoto = await _context.Votos
                .AnyAsync(voto => voto.VotacionId == votacionId && voto.UsuarioId == userId);

            if (yaVoto)
            {
                TempData["InfoMessage"] = $"Usted ya ha participado en la votación '{votacion.Titulo}'.";
                return RedirectToAction("Index", "Home");
            }

            // Si todas las validaciones pasan, muestra la vista de votación
            return View("~/Views/Votaciones/UserVoting.cshtml", votacion);
        }

        // POST: AsambleasVotaciones/RegistrarVoto
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarVoto(int votacionId, int opcionId)
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out var userId))
            {
                TempData["ErrorMessage"] = "Error de autenticación. Intente iniciar sesión nuevamente.";
                return RedirectToAction("UserVoting", new { votacionId = votacionId }); // O al login
            }

            var ahora = DateTime.Now;

            // Re-verificar restricciones (seguridad adicional)
            var tieneRestriccion = await _context.Restriccions
                .AnyAsync(r => r.UsuarioId == userId &&
                               (r.FechaFin == null || r.FechaFin > ahora) &&
                               r.FechaInicio <= ahora);
            if (tieneRestriccion)
            {
                TempData["ErrorMessage"] = "Usted tiene restricciones activas y su voto no puede ser procesado.";
                return RedirectToAction("Index", "Home");
            }

            var votacion = await _context.Votacions
                                         .Include(v => v.OpcionVotacions)
                                         .FirstOrDefaultAsync(v => v.Id == votacionId);

            if (votacion == null)
            {
                TempData["ErrorMessage"] = "La votación no fue encontrada.";
                return RedirectToAction("Index", "Home");
            }

            // Re-verificar si la votación está activa (seguridad adicional)
            if (votacion.Estado != "en_curso" || !(votacion.FechaInicio <= ahora && votacion.FechaFin >= ahora))
            {
                TempData["ErrorMessage"] = "Esta votación ya no está activa.";
                return RedirectToAction("UserVoting", new { votacionId = votacionId });
            }

            // Re-verificar si el usuario ya votó (seguridad adicional)
            var yaVoto = await _context.Votos
                .AnyAsync(v => v.VotacionId == votacionId && v.UsuarioId == userId);
            if (yaVoto)
            {
                TempData["ErrorMessage"] = "Usted ya ha votado en esta elección.";
                return RedirectToAction("Index", "Home");
            }

            // Verificar que la opción seleccionada pertenezca a la votación
            var opcionValida = votacion.OpcionVotacions.Any(o => o.Id == opcionId);
            if (!opcionValida)
            {
                TempData["ErrorMessage"] = "La opción seleccionada no es válida para esta votación.";
                return RedirectToAction("UserVoting", new { votacionId = votacionId });
            }

            // Si todo es válido, crear y guardar el voto
            var nuevoVoto = new Voto
            {
                VotacionId = votacionId,
                UsuarioId = userId,
                OpcionId = opcionId,
                Fecha = DateTime.Now,
                IpOrigen = HttpContext.Connection.RemoteIpAddress?.ToString(), // Opcional
                ValorPonderado = 1 // O la lógica que tengas para el valor ponderado
            };

            _context.Votos.Add(nuevoVoto);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"¡Gracias! Su voto para '{votacion.Titulo}' ha sido registrado exitosamente.";
            return RedirectToAction("Index", "Home");
        }



    }
}