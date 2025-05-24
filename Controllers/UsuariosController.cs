using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VotingSystem.Models; // Reemplaza con el namespace correcto de tus modelos y DbContext
using System.Linq;
using System.Threading.Tasks;

[Authorize(Roles = "admin")] // Solo los administradores pueden gestionar usuarios
public class UsuariosController : Controller
{
    private readonly DbAb85acVotacionesdbContext _context; // Asegúrate que el nombre del DbContext sea el correcto
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(DbAb85acVotacionesdbContext context, ILogger<UsuariosController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Usuarios
    public async Task<IActionResult> Index(string searchString, string currentFilter, string sortOrder, int? pageNumber)
    {
        ViewData["CurrentSort"] = sortOrder;
        ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["EmailSortParm"] = sortOrder == "Email" ? "email_desc" : "Email";
        ViewData["RoleSortParm"] = sortOrder == "Role" ? "role_desc" : "Role";
        ViewData["StatusSortParm"] = sortOrder == "Status" ? "status_desc" : "Status";

        if (searchString != null)
        {
            pageNumber = 1;
        }
        else
        {
            searchString = currentFilter;
        }

        ViewData["CurrentFilter"] = searchString;

        var usuarios = from u in _context.Usuarios select u;

        if (!string.IsNullOrEmpty(searchString))
        {
            usuarios = usuarios.Where(s => s.Nombre.Contains(searchString) ||
                                           s.Apellido.Contains(searchString) ||
                                           s.Email.Contains(searchString));
        }

        switch (sortOrder)
        {
            case "name_desc":
                usuarios = usuarios.OrderByDescending(u => u.Apellido).ThenByDescending(u => u.Nombre);
                break;
            case "Email":
                usuarios = usuarios.OrderBy(u => u.Email);
                break;
            case "email_desc":
                usuarios = usuarios.OrderByDescending(u => u.Email);
                break;
            case "Role":
                usuarios = usuarios.OrderBy(u => u.Rol);
                break;
            case "role_desc":
                usuarios = usuarios.OrderByDescending(u => u.Rol);
                break;
            case "Status":
                usuarios = usuarios.OrderBy(u => u.Estado);
                break;
            case "status_desc":
                usuarios = usuarios.OrderByDescending(u => u.Estado);
                break;
            default: // Por defecto ordenar por Apellido ascendente
                usuarios = usuarios.OrderBy(u => u.Apellido).ThenBy(u => u.Nombre);
                break;
        }

        int pageSize = 10; // Puedes hacerlo configurable
        var paginatedList = await PaginatedList<Usuario>.CreateAsync(usuarios.AsNoTracking(), pageNumber ?? 1, pageSize);
        return View(paginatedList);
    }

    // GET: Usuarios/Details/5 (Ejemplo)
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(m => m.Id == id);
        if (usuario == null)
        {
            return NotFound();
        }
        return View(usuario); // Necesitarás una vista Details.cshtml
    }


    // GET: Usuarios/Edit/5 (Ejemplo básico)
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            return NotFound();
        }

        ViewBag.Roles = new SelectList(new List<string> { "admin", "propietario" }, usuario.Rol);
        ViewBag.Estados = new SelectList(new List<string> { "activo", "inactivo" }, usuario.Estado);
        return View(usuario); // Necesitarás una vista Edit.cshtml para Usuario
    }

    // POST: Usuarios/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre,Apellido,Email,Rol,Estado")] Usuario usuarioViewModel)
    {
        if (id != usuarioViewModel.Id)
        {
            return NotFound();
        }

        // No queremos actualizar la contraseña aquí, ni propiedades de navegación complejas directamente
        ModelState.Remove("Password");
        ModelState.Remove("Asambleas");
        ModelState.Remove("Notificacions");
        ModelState.Remove("Reportes");
        ModelState.Remove("RestriccionCreadoPorNavigations");
        ModelState.Remove("RestriccionUsuarios");
        ModelState.Remove("Votos");


        if (ModelState.IsValid)
        {
            try
            {
                var userToUpdate = await _context.Usuarios.FindAsync(id);
                if (userToUpdate == null) { return NotFound(); }

                // Verificar si el email ha cambiado y si ya existe (excepto para el usuario actual)
                if (userToUpdate.Email != usuarioViewModel.Email &&
                    await _context.Usuarios.AnyAsync(u => u.Email == usuarioViewModel.Email && u.Id != id))
                {
                    ModelState.AddModelError("Email", "Este correo electrónico ya está en uso por otro usuario.");
                    // Repopulate ViewBags and return view
                    ViewBag.Roles = new SelectList(new List<string> { "admin", "propietario" }, usuarioViewModel.Rol);
                    ViewBag.Estados = new SelectList(new List<string> { "activo", "inactivo" }, usuarioViewModel.Estado);
                    return View(usuarioViewModel);
                }

                userToUpdate.Nombre = usuarioViewModel.Nombre;
                userToUpdate.Apellido = usuarioViewModel.Apellido;
                userToUpdate.Email = usuarioViewModel.Email;
                userToUpdate.Rol = usuarioViewModel.Rol;
                userToUpdate.Estado = usuarioViewModel.Estado;
                // No actualizamos FechaRegistro ni UltimoAcceso aquí a menos que sea intencional

                _context.Update(userToUpdate);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Usuario actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Usuarios.Any(e => e.Id == usuarioViewModel.Id))
                {
                    return NotFound();
                }
                else
                {
                    _logger.LogWarning("Error de concurrencia al editar usuario ID {UserId}", id);
                    ModelState.AddModelError(string.Empty, "El registro fue modificado por otro usuario. Inténtelo de nuevo.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar usuario ID {UserId}", id);
                ModelState.AddModelError(string.Empty, "Ocurrió un error al guardar los cambios.");
            }
        }
        ViewBag.Roles = new SelectList(new List<string> { "admin", "propietario" }, usuarioViewModel.Rol);
        ViewBag.Estados = new SelectList(new List<string> { "activo", "inactivo" }, usuarioViewModel.Estado);
        return View(usuarioViewModel);
    }
}