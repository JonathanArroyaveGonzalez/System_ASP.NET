using Microsoft.AspNetCore.Mvc;
using VotingSystem.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
// Add the following using directive at the top of the file to resolve the 'iTextSharp' namespace issue.
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
// Add the following using directive at the top of the file to resolve the 'OfficeOpenXml' namespace issue.
using OfficeOpenXml;

// Ensure that the EPPlus library is installed in your project. You can do this by adding the NuGet package:
// Run the following command in the Package Manager Console or use the NuGet Package Manager in Visual Studio:
// Install-Package EPPlus


namespace VotingSystem.Controllers
{
    public class ReportesController : Controller
    {
        private readonly DbAb85acVotacionesdbContext _context;

        public ReportesController(DbAb85acVotacionesdbContext context)
        {
            _context = context;
        }

        // GET: Reportes
        public IActionResult Index()
        {
            var reportes = _context.Reportes
                .Include(r => r.GeneradoPorNavigation) // Incluye el usuario creador
                .ToList();
            return View("~/Views/Reportes/Index.cshtml", reportes);
        }


        // GET: Reportes/Details/5
        public IActionResult Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reporte = _context.Reportes
                .Include(r => r.GeneradoPorNavigation) // Incluye el usuario creador
                .FirstOrDefault(m => m.Id == id);
            if (reporte == null)
            {
                return NotFound();
            }

            return View("~/Views/Reportes/Details.cshtml", reporte);
        }


        // GET: Reportes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Reportes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id,Tipo,Titulo,FechaGeneracion,GeneradoPor,UrlArchivo,Formato")] Reporte reporte)
        {
            if (ModelState.IsValid)
            {
                _context.Add(reporte);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View("~/Views/Reportes/ReporteVotacion.cshtml", reporte);
        }

        // GET: Reportes/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reporte = _context.Reportes.Find(id);
            if (reporte == null)
            {
                return NotFound();
            }
            return View("~/Views/Reportes/ReporteVotacion.cshtml", reporte);
        }

        // POST: Reportes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id,Tipo,Titulo,FechaGeneracion,GeneradoPor,UrlArchivo,Formato")] Reporte reporte)
        {
            if (id != reporte.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(reporte);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReporteExists(reporte.Id))
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
            return View("~/Views/Reportes/ReporteVotacion.cshtml", reporte);
        }

        // GET: Reportes/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reporte = _context.Reportes
                .FirstOrDefault(m => m.Id == id);
            if (reporte == null)
            {
                return NotFound();
            }

            return View("~/Views/Reportes/ReporteVotacion.cshtml", reporte);
        }

        // POST: Reportes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var reporte = _context.Reportes.Find(id);
            if (reporte != null)
            {
                _context.Reportes.Remove(reporte);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool ReporteExists(int id)
        {
            return _context.Reportes.Any(e => e.Id == id);
        }

        // GET: Reportes/GenerarReporteVotacion/5
        public IActionResult GenerarReporteVotacion(int? asambleaId)
        {
            if (asambleaId == null)
            {
                return NotFound();
            }

            var asamblea = _context.Asambleas
                .Include(a => a.Votacions)
                    .ThenInclude(v => v.OpcionVotacions)
                .Include(a => a.Votacions)
                    .ThenInclude(v => v.Votos)
                .FirstOrDefault(a => a.Id == asambleaId);

            if (asamblea == null)
            {
                return NotFound();
            }

            var reporte = new
            {
                Asamblea = asamblea.Titulo,
                Fecha = asamblea.Fecha,
                Votaciones = asamblea.Votacions.Select(v => new
                {
                    v.Id,
                    v.Titulo,
                    Opciones = v.OpcionVotacions.Select(o => new
                    {
                        o.Texto,
                        o.Descripcion,
                        // Conteo de votos para esta opción
                        CantidadVotos = v.Votos.Count(vt => vt.OpcionId == o.Id)
                    })
                })
            };

            return View("~/Views/Reportes/ReporteVotacion.cshtml", reporte);
        }



        // GET: Reportes/ExportarPDF/5
        public IActionResult ExportarPDF(int? asambleaId)
        {
            if (asambleaId == null)
            {
                return NotFound();
            }

            var asamblea = _context.Asambleas
                .Include(a => a.Votacions)
                .ThenInclude(v => v.OpcionVotacions)
                .FirstOrDefault(a => a.Id == asambleaId);

            if (asamblea == null)
            {
                return NotFound();
            }

            // Generar PDF usando iTextSharp
            var document = new Document();
            using (var stream = new MemoryStream())
            {
                PdfWriter.GetInstance(document, stream);
                document.Open();

                document.Add(new Paragraph($"Reporte de Asamblea: {asamblea.Titulo}"));
                document.Add(new Paragraph($"Fecha: {asamblea.Fecha}"));

                foreach (var votacion in asamblea.Votacions)
                {
                    document.Add(new Paragraph($"Votación ID: {votacion.Id}"));
                    foreach (var opcion in votacion.OpcionVotacions)
                    {
                        document.Add(new Paragraph($"- {opcion.Texto}: {opcion.Descripcion}"));
                    }
                }

                document.Close();

                return File(stream.ToArray(), "application/pdf", "Reporte.pdf");
            }
        }
        // GET: Reportes/ExportarExcel/5
        public IActionResult ExportarExcel(int? asambleaId)
        {
            if (asambleaId == null)
            {
                return NotFound();
            }

            var asamblea = _context.Asambleas
                .Include(a => a.Votacions)
                .ThenInclude(v => v.OpcionVotacions)
                .FirstOrDefault(a => a.Id == asambleaId);

            if (asamblea == null)
            {
                return NotFound();
            }

            using (var package = new OfficeOpenXml.ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Reporte");
                worksheet.Cells[1, 1].Value = "Reporte de Asamblea";
                worksheet.Cells[2, 1].Value = asamblea.Titulo;
                worksheet.Cells[3, 1].Value = "Fecha";
                worksheet.Cells[3, 2].Value = asamblea.Fecha;

                int row = 5;
                foreach (var votacion in asamblea.Votacions)
                {
                    worksheet.Cells[row, 1].Value = $"Votación ID: {votacion.Id}";
                    row++;
                    foreach (var opcion in votacion.OpcionVotacions)
                    {
                        worksheet.Cells[row, 2].Value = opcion.Texto;
                        worksheet.Cells[row, 3].Value = opcion.Descripcion;
                        row++;
                    }
                }

                return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Reporte.xlsx");
            }
        }

        // GET: Reportes/Historico
        public IActionResult Historico()
        {
            var historico = _context.Votos
                .Include(v => v.Usuario)
                .Include(v => v.Votacion)
                .ThenInclude(v => v.Asamblea)
                .Select(v => new
                {
                    Usuario = v.Usuario.Nombre + " " + v.Usuario.Apellido,
                    Asamblea = v.Votacion.Asamblea.Titulo,
                    Fecha = v.Votacion.Asamblea.Fecha
                })
                .ToList();

            return View("~/Views/Reportes/historico.cshtml", historico);

        }



    }
}
