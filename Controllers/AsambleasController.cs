using Microsoft.AspNetCore.Mvc;

public class AsambleasController : Controller
{
    public IActionResult Index()
    {
        // Listar todas las asambleas
        return View();
    }

    public IActionResult Create()
    {
        // Formulario de creaci�n
        return View();
    }

   

    public IActionResult Calendar()
    {
        // Vista de calendario
        return View();
    }
}