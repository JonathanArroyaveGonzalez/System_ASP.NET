using Microsoft.AspNetCore.Mvc;

public class ReportesController : Controller
{
    public IActionResult Index()
    {
        // Reportes principales
        return View();
    }
}