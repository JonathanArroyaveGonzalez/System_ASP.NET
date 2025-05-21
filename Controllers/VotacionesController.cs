using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotingSystem.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

[Authorize]
public class VotacionesController : Controller
{
    private readonly DbAb85acVotacionesdbContext _context;

    public VotacionesController(DbAb85acVotacionesdbContext context)
    {
        _context = context;
    }

    // GET: Votaciones/Create
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
        if (ModelState.IsValid)
        {
            votacion.Estado = "pendiente";
            _context.Add(votacion);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Votaci�n creada exitosamente. Ahora puede agregar opciones.";
            return RedirectToAction("AddOptions", new { id = votacion.Id });
        }

        var asamblea = await _context.Asambleas.FindAsync(votacion.AsambleaId);
        ViewData["AsambleaTitulo"] = asamblea?.Titulo;
        return View(votacion);
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
        var votacion = await _context.Votacions.FindAsync(id);
        if (votacion == null)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            opcion.VotacionId = id;
            _context.Add(opcion);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Opci�n agregada exitosamente.";
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
            .Include(v => v.OpcionVotacions)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (votacion == null)
        {
            return NotFound();
        }

        // Verificar si el usuario ya vot�
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        ViewBag.YaVoto = await _context.Votos
            .AnyAsync(v => v.VotacionId == id && v.UsuarioId == usuarioId);

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

        // Validar que la votaci�n est� en curso
        if (votacion.Estado != "en_curso")
        {
            TempData["ErrorMessage"] = "Esta votaci�n no est� disponible para votar en este momento.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Validar que el usuario no haya votado antes
        var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        if (await _context.Votos.AnyAsync(v => v.VotacionId == id && v.UsuarioId == usuarioId))
        {
            TempData["ErrorMessage"] = "Ya has participado en esta votaci�n.";
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

        var votacion = await _context.Votacions.FindAsync(id);
        if (votacion == null)
        {
            return NotFound();
        }

        if (votacion.Estado != "pendiente")
        {
            TempData["ErrorMessage"] = "Solo se pueden iniciar votaciones en estado 'pendiente'.";
            return RedirectToAction(nameof(Details), new { id });
        }

        votacion.Estado = "en_curso";
        _context.Update(votacion);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Votaci�n iniciada exitosamente.";
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

        TempData["SuccessMessage"] = "Votaci�n concluida exitosamente.";
        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Votaciones/Results/5
    public async Task<IActionResult> Results(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        // Obt�n primero la votaci�n con sus opciones
        var votacion = await _context.Votacions
            .Include(v => v.OpcionVotacions)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (votacion == null)
        {
            return NotFound();
        }

        // Ahora carga los votos manualmente para cada opci�n
        foreach (var opcion in votacion.OpcionVotacions)
        {
            // Cargar los votos para cada opci�n
            var votos = await _context.Votos
                .Where(v => v.OpcionId == opcion.Id)
                .Include(v => v.Usuario)  // Incluir usuario si se necesita para votaciones p�blicas
                .ToListAsync();

            // Necesitamos agregar los votos a una propiedad en OpcionVotacion
            // Si no tienes esta propiedad, podemos usar ViewBag para pasarlos a la vista
        }

        // Pasamos los datos a la vista usando ViewBag si no podemos modificar el modelo
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
                .ToListAsync();
        }

        return View(votacion);
    }
}