using Microsoft.AspNetCore.Mvc;

public class VotacionesController : Controller
{
    public IActionResult Index()
    {
        // Listar todas las votaciones
        return View();
    }

    public IActionResult Active()
    {
        // Votaciones activas
        return View();
    }

    public IActionResult Results()
    {
        // Resultados de votaciones
        return View();
    }
}