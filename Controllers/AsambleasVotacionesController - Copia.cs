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

namespace VotingSystem.Controllers
{
    public class AsambleasVotacionesController2 : Controller
    {
        private readonly DbAb85acVotacionesdbContext _context;
        private const string BrevoApiKey = "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";

        public AsambleasVotacionesController2(DbAb85acVotacionesdbContext context)
        {
            _context = context;
        }

        // ASAMBLEAS

        // GET: Asambleas
        public async Task<IActionResult> IndexAsambleas()
        {
            return View("~/Views/Asambleas/IndexAsambleas.cshtml", await _context.Asambleas.Include(a => a.Creador).ToListAsync());
        }

        // GET: Asambleas/Create
        /*
        public IActionResult CreateAsamblea()
        {
            ViewData["CreadorId"] = _context.Usuarios.Select(u => new { u.Id, NombreCompleto = $"{u.Nombre} {u.Apellido}" }).ToList();
            return View("~/Views/Asambleas/CreateAsamblea.cshtml");
        }

        // POST: Asambleas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsamblea([Bind("Id,Titulo,Descripcion,Fecha,Estado,CreadorId,Acta")] Asamblea asamblea)
        {
            if (ModelState.IsValid)
            {
                _context.Add(asamblea);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(IndexAsambleas));
            }
            ViewData["CreadorId"] = _context.Usuarios.Select(u => new { u.Id, NombreCompleto = $"{u.Nombre} {u.Apellido}" }).ToList();
            Console.WriteLine("Modelo invalido asamblea");
            return View("~/Views/Asambleas/CreateAsamblea.cshtml", asamblea);
        }

        **/

        // GET: Asambleas/Create
        public IActionResult CreateAsamblea()
        {
            // Crear ViewBag correctamente para el SelectList
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

        // POST: Asambleas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsamblea([Bind("Titulo,Descripcion,Fecha,Estado,CreadorId,Acta")] Asamblea asamblea)
        {
            // Establecer fecha de creación
            asamblea.FechaCreacion = DateTime.Now;

            // Remover validación de la propiedad de navegación Creador
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
                    // Log del error para debugging
                    Console.WriteLine($"Error al guardar: {ex.Message}");
                    ModelState.AddModelError("", "Error al guardar la asamblea");
                }
            }

            // Debug: Mostrar errores de validación
            foreach (var error in ModelState)
            {
                Console.WriteLine($"Campo: {error.Key}");
                foreach (var err in error.Value.Errors)
                {
                    Console.WriteLine($"Error: {err.ErrorMessage}");
                }
            }

            // Recargar ViewBag en caso de error
            ViewBag.CreadorId = new SelectList(
                _context.Usuarios.Select(u => new {
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
        /*
        // GET: Votaciones/Create
        public IActionResult CreateVotacion()
        {
            ViewData["AsambleaId"] = _context.Asambleas.Select(a => new { a.Id, a.Titulo }).ToList();
            return View("~/Views/Votaciones/CreateVotacion.cshtml");
        }

        // POST: Votaciones/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVotacion([Bind("Id,Titulo,Descripcion,TipoVotacion,FechaInicio,FechaFin,QuorumRequerido,AsambleaId")] Votacion votacion)
        {
            if (ModelState.IsValid)
            {
                _context.Add(votacion);
                await _context.SaveChangesAsync();

                // Enviar notificación por correo
                await EnviarCorreoVotacionCreada(votacion);
                
                return RedirectToAction(nameof(IndexVotaciones));
            }
            ViewData["AsambleaId"] = _context.Asambleas.Select(a => new { a.Id, a.Titulo }).ToList();
            
           
            return View("~/Views/Votaciones/CreateVotacion.cshtml",votacion);
        }
        **/

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
            // Remover validación de las propiedades de navegación
            ModelState.Remove("Asamblea");
            ModelState.Remove("OpcionVotacions");
            ModelState.Remove("Votos");

            // Validar que existan opciones
            if (OpcionTexto == null || !OpcionTexto.Any(x => !string.IsNullOrWhiteSpace(x)))
            {
                ModelState.AddModelError("", "Debe agregar al menos una opción de votación");
            }

            // Validar que haya al menos 2 opciones válidas
            var opcionesValidas = OpcionTexto?.Where(x => !string.IsNullOrWhiteSpace(x)).Count() ?? 0;
            if (opcionesValidas < 2)
            {
                ModelState.AddModelError("", "Debe agregar al menos 2 opciones de votación válidas");
            }

            if (ModelState.IsValid)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. CREAR LA VOTACIÓN PRIMERO
                    _context.Add(votacion);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"Votación creada con ID: {votacion.Id}");

                    // 2. CREAR LAS OPCIONES DE VOTACIÓN CON EL ID OBTENIDO
                    var opciones = new List<OpcionVotacion>();
                    for (int i = 0; i < OpcionTexto.Count; i++)
                    {
                        if (!string.IsNullOrWhiteSpace(OpcionTexto[i]))
                        {
                            var opcion = new OpcionVotacion
                            {
                                VotacionId = votacion.Id, // Ahora tenemos el ID real
                                Texto = OpcionTexto[i].Trim(),
                                Descripcion = (OpcionDescripcion != null && i < OpcionDescripcion.Count)
                                    ? OpcionDescripcion[i]?.Trim()
                                    : null,
                                Orden = opciones.Count + 1 // Orden secuencial de opciones válidas
                            };
                            opciones.Add(opcion);
                            _context.OpcionVotacions.Add(opcion);
                        }
                    }

                    await _context.SaveChangesAsync();
                    Console.WriteLine($"Se crearon {opciones.Count} opciones para la votación {votacion.Id}");

                    // 3. CONFIRMAR LA TRANSACCIÓN
                    await transaction.CommitAsync();

                    // 4. ENVIAR NOTIFICACIÓN POR CORREO (DESPUÉS DE QUE TODO ESTÉ GUARDADO)
                    try
                    {
                        await EnviarCorreoVotacionCreada(votacion);
                        Console.WriteLine("Notificación enviada exitosamente");
                    }
                    catch (Exception emailEx)
                    {
                        // Si falla el email, no afecta la creación de la votación
                        Console.WriteLine($"Error al enviar notificación: {emailEx.Message}");
                        // Opcional: Agregar mensaje de advertencia para el usuario
                        TempData["Warning"] = "La votación se creó correctamente, pero hubo un problema al enviar las notificaciones.";
                    }

                    TempData["Success"] = $"Votación '{votacion.Titulo}' creada exitosamente con {opciones.Count} opciones.";
                    return RedirectToAction(nameof(IndexVotaciones));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine($"Error al crear la votación: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                    ModelState.AddModelError("", $"Error al guardar la votación: {ex.Message}");
                }
            }

            // Debug: Mostrar errores de validación
            foreach (var error in ModelState)
            {
                Console.WriteLine($"Campo: {error.Key}");
                foreach (var err in error.Value.Errors)
                {
                    Console.WriteLine($"Error: {err.ErrorMessage}");
                }
            }

            // Recargar ViewBag en caso de error
            ViewBag.AsambleaId = new SelectList(
                _context.Asambleas.Select(a => new {
                    Id = a.Id,
                    Titulo = a.Titulo
                }).ToList(),
                "Id",
                "Titulo",
                votacion.AsambleaId
            );

            return View("~/Views/Votaciones/CreateVotacion.cshtml", votacion);
        }


        // Método auxiliar para enviar email individual
        private async Task EnviarEmailIndividual(Usuario usuario, Votacion votacion)
        {
            // Implementa aquí tu lógica de envío de email
            // Ejemplo con información de la votación y sus opciones

            var asunto = $"Nueva Votación: {votacion.Titulo}";

            var mensaje = $@"
        Estimado/a {usuario.Nombre} {usuario.Apellido},
        
        Se ha creado una nueva votación:
        
        Título: {votacion.Titulo}
        Descripción: {votacion.Descripcion}
        Tipo: {votacion.TipoVotacion}
        Estado: {votacion.Estado}
        Fecha de inicio: {votacion.FechaInicio:dd/MM/yyyy HH:mm}
        Fecha de fin: {votacion.FechaFin:dd/MM/yyyy HH:mm}
        Quórum requerido: {votacion.QuorumRequerido}%
        
        
        Asamblea: {votacion.Asamblea?.Titulo}
        
        Por favor, participe en la votación durante el período establecido.
        
        Saludos cordiales,
        Sistema de Votaciones
    ";

            // Aquí llamarías a tu servicio de email real
            // await _emailService.EnviarEmail(usuario.Email, asunto, mensaje);

            Console.WriteLine($"Email enviado a {usuario.Email}: {asunto}");

            // Simular delay para no sobrecargar el servidor de email
            await Task.Delay(100);
        }

        // GET: Votaciones/Edit/5
        public async Task<IActionResult> EditVotacion(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var votacion = await _context.Votacions.FindAsync(id);
            if (votacion == null)
            {
                return NotFound();
            }
            ViewData["AsambleaId"] = _context.Asambleas.Select(a => new { a.Id, a.Titulo }).ToList();
            return View("~/Views/Votaciones/EditVotacion.cshtml",votacion);
        }

        // POST: Votaciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVotacion(int id, [Bind("Id,Titulo,Descripcion,TipoVotacion,FechaInicio,FechaFin,QuorumRequerido,AsambleaId")] Votacion votacion)
        {
            if (id != votacion.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(votacion);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VotacionExists(votacion.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(IndexVotaciones));
            }
            ViewData["AsambleaId"] = _context.Asambleas.Select(a => new { a.Id, a.Titulo }).ToList();
            return View("~/Views/Votaciones/EditVotacion.cshtml", votacion);
        }

        // GET: Votaciones/Delete/5
        public async Task<IActionResult> DeleteVotacion(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var votacion = await _context.Votacions
                .Include(v => v.Asamblea)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (votacion == null)
            {
                return NotFound();
            }

            return View("~/Views/Votaciones/DeleteVotacion.cshtml", votacion);
        }

        // POST: Votaciones/Delete/5
        [HttpPost, ActionName("DeleteVotacion")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVotacionConfirmed(int id)
        {
            var votacion = await _context.Votacions.FindAsync(id);
            _context.Votacions.Remove(votacion);
            await _context.SaveChangesAsync();
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

        private async Task EnviarCorreoVotacionCreada(Votacion votacion)
        {
            var asamblea = await _context.Asambleas.FindAsync(votacion.AsambleaId);
            var usuarios = await _context.Usuarios.Where(u => u.Estado == "activo").ToListAsync();

            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("api-key", BrevoApiKey);

            foreach (var usuario in usuarios)
            {
                var emailData = new
                {
                    sender = new { email = "juliancam24708@gmail.com", name = "Sistema de Votaciones" },
                    to = new[] { new { email = usuario.Email, name = $"{usuario.Nombre} {usuario.Apellido}" } },
                    subject = $"Nueva Votación: {votacion.Titulo}",
                    htmlContent = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            background-color: #f5f7fa;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 30px auto;
            background-color: #ffffff;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
        }}
        .header img {{
            width: 100%;
            border-radius: 10px 10px 0 0;
        }}
        h1 {{
            color: #2c3e50;
        }}
        h2 {{
            color: #34495e;
        }}
        p {{
            color: #555;
            font-size: 15px;
            line-height: 1.6;
        }}
        .button {{
            display: inline-block;
            margin-top: 20px;
            padding: 12px 24px;
            background-color: #3498db;
            color: #ffffff !important;
            text-decoration: none;
            font-weight: bold;
            border-radius: 5px;
            transition: background-color 0.3s ease;
        }}
        .button:hover {{
            background-color: #2980b9;
        }}
        .footer {{
            margin-top: 30px;
            font-size: 12px;
            color: #aaa;
            text-align: center;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <img src='https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcR96ZrZnc6TWTJAgRU20uzS1k4a9LwrOWM-9g&s' alt='Imagen Votación' />
        </div>
        <h1>Nueva Votación Creada</h1>
        <p>Se ha creado una nueva votación en el sistema:</p>
        <h2>{votacion.Titulo}</h2>
        <p><strong>Asamblea:</strong> {asamblea?.Titulo}</p>
        <p><strong>Descripción:</strong> {votacion.Descripcion}</p>
        <p><strong>Tipo:</strong> {votacion.TipoVotacion}</p>
        <p><strong>Fecha de inicio:</strong> {votacion.FechaInicio:dd/MM/yyyy HH:mm}</p>
        <p><strong>Fecha de fin:</strong> {votacion.FechaFin:dd/MM/yyyy HH:mm}</p>

        <a href='https://tusistema.com/votaciones' class='button'>Ir al sistema y votar</a>

        <div class='footer'>
            © {DateTime.Now.Year} Sistema de Votaciones - Propiedad Horizontal
        </div>
    </div>
</body>
</html>"
            }
            ;

                var content = new StringContent(JsonSerializer.Serialize(emailData), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://api.brevo.com/v3/smtp/email", content);

                if (!response.IsSuccessStatusCode)
                {
                    // Registrar error
                    Console.WriteLine($"Error enviando correo a {usuario.Email}: {await response.Content.ReadAsStringAsync()}");
                }
            }
        }
    }
}