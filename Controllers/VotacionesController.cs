using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using VotingSystem.Models;

[Authorize]
public class VotacionesController : Controller
{
    private readonly DbAb85acVotacionesdbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private const string BrevoApiKey = "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";
    private readonly ILogger<VotacionesController> _logger;

    public VotacionesController(DbAb85acVotacionesdbContext context, IHttpClientFactory httpClientFactory, ILogger<VotacionesController> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    // GET: Votaciones
    public async Task<IActionResult> Index()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var esAdmin = User.IsInRole("admin");

        var votaciones = await _context.Votacions
            .Include(v => v.Asamblea)
            .OrderByDescending(v => v.FechaInicio)
            .ToListAsync();

        // Marcar si el usuario ya votó en cada votación
        foreach (var votacion in votaciones)
        {
            ViewData[$"YaVoto_{votacion.Id}"] = await _context.Votos
                .AnyAsync(v => v.VotacionId == votacion.Id && v.UsuarioId == userId);
        }

        return View(votaciones);
    }

    // GET: Votaciones/Create con notificacion a usuarios
    [Authorize(Roles = "admin")]
    public IActionResult Create(int asambleaId)
    {
        var asamblea = _context.Asambleas.Find(asambleaId);
        if (asamblea == null)
        {
            return NotFound();
        }

        ViewData["AsambleaTitulo"] = asamblea.Titulo;

        var model = new Votacion
        {
            AsambleaId = asambleaId,
            FechaInicio = DateTime.Now,
            FechaFin = DateTime.Now.AddDays(1)
        };

        return View(model);
    }

    // POST: Votaciones/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([Bind("AsambleaId,Titulo,Descripcion,FechaInicio,FechaFin,TipoVotacion")] Votacion votacion)
    {
        // Validar que FechaFin sea mayor que FechaInicio
        if (votacion.FechaFin <= votacion.FechaInicio)
        {
            ModelState.AddModelError("FechaFin", "La fecha de fin debe ser posterior a la fecha de inicio");
        }

        if (ModelState.IsValid)
        {
            try
            {
                votacion.Estado = "pendiente";
                _context.Add(votacion);
                await _context.SaveChangesAsync();

                await NotificarNuevaVotacion(votacion);

                TempData["SuccessMessage"] = "Votación creada exitosamente. Ahora puede agregar opciones.";
                return RedirectToAction("AddOptions", new { id = votacion.Id });
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Error al crear votación");
                ModelState.AddModelError("", "No se pudo crear la votación. Por favor intente nuevamente.");
            }
        }

        // Si hay errores, recargar los datos necesarios para la vista
        var asamblea = await _context.Asambleas.FindAsync(votacion.AsambleaId);
        ViewData["AsambleaTitulo"] = asamblea?.Titulo;
        return View(votacion);
    }

    private async Task NotificarNuevaVotacion(Votacion votacion)
    {
        var asamblea = await _context.Asambleas.FindAsync(votacion.AsambleaId);
        var usuarios = await _context.Usuarios
            .Where(u => u.Estado == "activo")
            .Select(u => u.Email)
            .ToListAsync();

        if (!usuarios.Any())
            return;

        var client = _httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("api-key", BrevoApiKey);

        var requestData = new
        {
            sender = new { name = "Sistema de Votaciones", email = "no-reply@votaciones.com" },
            to = usuarios.Select(email => new { email }).ToList(),
            subject = $"Nueva votación: {votacion.Titulo}",
            htmlContent = $"<h1>Nueva votación creada</h1>" +
                         $"<p>Se ha creado una nueva votación en la asamblea '{asamblea?.Titulo}'.</p>" +
                         $"<h2>{votacion.Titulo}</h2>" +
                         $"<p>{votacion.Descripcion}</p>" +
                         $"<p>Fecha de inicio: {votacion.FechaInicio:dd/MM/yyyy HH:mm}</p>" +
                         $"<p>Fecha de fin: {votacion.FechaFin:dd/MM/yyyy HH:mm}</p>"
        };

        var response = await client.PostAsync(
            "https://api.brevo.com/v3/smtp/email",
            new StringContent(System.Text.Json.JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError($"Error al enviar notificación de nueva votación: {response.StatusCode}");
        }
    }

    // GET: Votaciones/AddOptions/5
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AddOptions(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var votacion = await _context.Votacions
            .Include(v => v.OpcionVotacions)
            .Include(v => v.Asamblea)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (votacion == null)
        {
            return NotFound();
        }

        return View(votacion);
    }

    // POST: Votaciones/AddOption/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AddOption(int id, [Bind("Texto,Descripcion")] OpcionVotacion opcion)
    {
        var votacion = await _context.Votacions
            .Include(v => v.OpcionVotacions)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (votacion == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            opcion.VotacionId = id;
            opcion.Orden = votacion.OpcionVotacions.Count + 1;
            _context.Add(opcion);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Opción agregada exitosamente.";
            return RedirectToAction(nameof(AddOptions), new { id });
        }

        return View("AddOptions", votacion);
    }

    // GET: Votaciones/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var votacion = await _context.Votacions
            .Include(v => v.Asamblea)
            .Include(v => v.OpcionVotacions.OrderBy(o => o.Orden))
            .FirstOrDefaultAsync(m => m.Id == id);

        if (votacion == null)
        {
            return NotFound();
        }

        // Verificar si el usuario ya votó
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        ViewBag.YaVoto = await _context.Votos
            .AnyAsync(v => v.VotacionId == id && v.UsuarioId == usuarioId);

        // Verificar si el usuario tiene restricciones activas
        var tieneRestriccion = await _context.Restriccions
            .AnyAsync(r => r.UsuarioId == usuarioId &&
                    (r.FechaFin == null || r.FechaFin >= DateTime.Now) &&
                    r.FechaInicio <= DateTime.Now);

        ViewBag.TieneRestriccion = tieneRestriccion;

        return View(votacion);
    }

    // POST: Votaciones/Votar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Votar(int id, int opcionId)
    {
        var votacion = await _context.Votacions.FindAsync(id);
        if (votacion == null)
        {
            return NotFound();
        }

        // Validar que la votación esté en curso
        if (votacion.Estado != "en_curso")
        {
            TempData["ErrorMessage"] = "Esta votación no está disponible para votar en este momento.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Obtener ID del usuario actual
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        // Verificar si el usuario tiene restricciones activas
        var tieneRestriccion = await _context.Restriccions
            .AnyAsync(r => r.UsuarioId == usuarioId &&
                    (r.FechaFin == null || r.FechaFin >= DateTime.Now) &&
                    r.FechaInicio <= DateTime.Now);

        if (tieneRestriccion)
        {
            TempData["ErrorMessage"] = "No puede votar debido a restricciones existentes en su cuenta.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Validar que el usuario no haya votado antes
        if (await _context.Votos.AnyAsync(v => v.VotacionId == id && v.UsuarioId == usuarioId))
        {
            TempData["ErrorMessage"] = "Ya has participado en esta votación.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Registrar el voto
        var voto = new Voto
        {
            VotacionId = id,
            UsuarioId = usuarioId,
            OpcionId = opcionId,
            Fecha = DateTime.Now,
            IpOrigen = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        _context.Add(voto);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Tu voto ha sido registrado exitosamente.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Votaciones/Start/5
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Start(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var votacion = await _context.Votacions
            .Include(v => v.OpcionVotacions)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (votacion == null)
        {
            return NotFound();
        }

        if (votacion.Estado != "pendiente")
        {
            TempData["ErrorMessage"] = "Solo se pueden iniciar votaciones en estado 'pendiente'.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Verificar que tenga al menos dos opciones
        if (votacion.OpcionVotacions.Count < 2)
        {
            TempData["ErrorMessage"] = "La votación debe tener al menos dos opciones para poder iniciarla.";
            return RedirectToAction(nameof(AddOptions), new { id });
        }

        votacion.Estado = "en_curso";
        _context.Update(votacion);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Votación iniciada exitosamente.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Votaciones/Close/5
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Close(int? id)
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

        votacion.Estado = "concluida";
        votacion.FechaFin = DateTime.Now; // Actualizar fecha fin al momento de cierre
        _context.Update(votacion);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Votación concluida exitosamente.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Votaciones/Results/5
    public async Task<IActionResult> Results(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // Obtén primero la votación con sus opciones
        var votacion = await _context.Votacions
            .Include(v => v.OpcionVotacions)
            .Include(v => v.Asamblea)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (votacion == null)
        {
            return NotFound();
        }

        // Pasamos los datos a la vista usando ViewBag
        ViewBag.VotosPorOpcion = await _context.Votos
            .Where(v => v.VotacionId == id)
            .GroupBy(v => v.OpcionId)
            .Select(g => new { OpcionId = g.Key, Cantidad = g.Count() })
            .ToListAsync();

        if (votacion.TipoVotacion == "publica")
        {
            ViewBag.DetalleVotos = await _context.Votos
                .Where(v => v.VotacionId == id)
                .Include(v => v.Usuario)
                .Include(v => v.Opcion)
                .ToListAsync();
        }

        return View(votacion);
    }
}