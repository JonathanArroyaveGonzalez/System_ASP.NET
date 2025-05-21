using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VotingSystem.Models;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

public class AsambleasController : Controller
{
    private readonly DbAb85acVotacionesdbContext _context;

    public AsambleasController(DbAb85acVotacionesdbContext context)
    {
        _context = context;
    }

    // GET: Asambleas
    public async Task<IActionResult> Index()
    {
        var asambleas = await _context.Asambleas
            .Include(a => a.Creador)
            .OrderByDescending(a => a.Fecha)
            .ToListAsync();
        return View(asambleas);
    }

    // GET: Asambleas/Create
    [Authorize(Roles = "admin")]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Asambleas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Create([Bind("Titulo,Fecha,Descripcion")] Asamblea asamblea)
    {
        if (ModelState.IsValid)
        {
            asamblea.CreadorId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            asamblea.Estado = "programada";
            asamblea.FechaCreacion = DateTime.Now;

            _context.Add(asamblea);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Asamblea creada exitosamente.";
            return RedirectToAction(nameof(Index));
        }
        return View(asamblea);
    }

    // GET: Asambleas/Edit/5
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Edit(int? id)
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
        return View(asamblea);
    }

    // POST: Asambleas/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Titulo,Fecha,Descripcion,Estado,Acta")] Asamblea asamblea)
    {
        if (id != asamblea.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var existingAsamblea = await _context.Asambleas.FindAsync(id);
                existingAsamblea.Titulo = asamblea.Titulo;
                existingAsamblea.Fecha = asamblea.Fecha;
                existingAsamblea.Descripcion = asamblea.Descripcion;
                existingAsamblea.Estado = asamblea.Estado;
                existingAsamblea.Acta = asamblea.Acta;

                _context.Update(existingAsamblea);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Asamblea actualizada exitosamente.";
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
            return RedirectToAction(nameof(Index));
        }
        return View(asamblea);
    }

    // GET: Asambleas/Delete/5
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int? id)
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

        return View(asamblea);
    }

    // POST: Asambleas/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var asamblea = await _context.Asambleas.FindAsync(id);
        _context.Asambleas.Remove(asamblea);
        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Asamblea eliminada exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: Asambleas/Calendar
    public async Task<IActionResult> Calendar()
    {
        var asambleas = await _context.Asambleas
            .Where(a => a.Estado != "finalizada")
            .OrderBy(a => a.Fecha)
            .ToListAsync();
        return View(asambleas);
    }

    // GET: Asambleas/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var asamblea = await _context.Asambleas
            .Include(a => a.Creador)
            .Include(a => a.Votacions)
            .ThenInclude(v => v.OpcionVotacions)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (asamblea == null)
        {
            return NotFound();
        }

        return View(asamblea);
    }

    private bool AsambleaExists(int id)
    {
        return _context.Asambleas.Any(e => e.Id == id);
    }
}