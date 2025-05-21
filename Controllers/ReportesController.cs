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
                .Include(r => r.GeneradoPorNavigation)
                .FirstOrDefault(m => m.Id == id);

            if (reporte == null)
            {
                return NotFound();
            }

            // Relaciona por ID, no por título
            var asamblea = _context.Asambleas
                .Include(a => a.Votacions)
                    .ThenInclude(v => v.OpcionVotacions)
                .Include(a => a.Votacions)
                    .ThenInclude(v => v.Votos)
                        .ThenInclude(vt => vt.Usuario)
                .FirstOrDefault(a => a.Id == reporte.Id); // <-- Relación por ID

            var participantes = new List<dynamic>();
            var votaciones = new List<dynamic>();

            if (asamblea != null && asamblea.Votacions != null)
            {
                foreach (var votacion in asamblea.Votacions)
                {
                    var opciones = votacion.OpcionVotacions.Select(o => new
                    {
                        o.Texto,
                        o.Descripcion,
                        CantidadVotos = votacion.Votos.Count(vt => vt.OpcionId == o.Id)
                    }).ToList();

                    votaciones.Add(new
                    {
                        votacion.Id,
                        votacion.Titulo,
                        Opciones = opciones
                    });

                    participantes.AddRange(
                        votacion.Votos
                            .Where(v => v.Usuario != null)
                            .Select(v => new { Nombre = v.Usuario.Nombre + " " + v.Usuario.Apellido, Email = v.Usuario.Email })
                    );
                }
            }

            var viewModel = new
            {
                Reporte = reporte,
                Asamblea = asamblea,
                Votaciones = votaciones,
                Participantes = participantes.Distinct().ToList()
            };

            return View("~/Views/Reportes/Details.cshtml", viewModel);
        }





        // GET: Reportes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Reportes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Tipo,Titulo,UrlArchivo,Formato")] Reporte reporte)
        {
            var email = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;
            var identityName = User.Identity?.Name;

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Email == email)
                       ?? _context.Usuarios.FirstOrDefault(u => u.Nombre == identityName);

            if (usuario == null)
            {
                ModelState.AddModelError("", "No se pudo identificar el usuario actual.");
                return View(reporte);
            }

            reporte.GeneradoPor = usuario.Id;
            reporte.FechaGeneracion = DateTime.Now;

            // Elimina la validación de la navegación (para que ModelState sea válido)
            ModelState.Remove(nameof(reporte.GeneradoPorNavigation));

            if (ModelState.IsValid)
            {
                _context.Add(reporte);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(reporte);
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
                .Include(a => a.Votacions)
                    .ThenInclude(v => v.Votos)
                        .ThenInclude(vt => vt.Usuario)
                .FirstOrDefault(a => a.Id == asambleaId);

            if (asamblea == null)
            {
                return NotFound();
            }

            // Buscar el reporte asociado a la asamblea (ajusta si tienes otra relación)
            var reporte = _context.Reportes
                .Include(r => r.GeneradoPorNavigation)
                .FirstOrDefault(r => r.Id == asambleaId);

            // Datos generales
            string creador = reporte?.GeneradoPorNavigation != null
                ? reporte.GeneradoPorNavigation.Nombre + " " + reporte.GeneradoPorNavigation.Apellido
                : "Desconocido";

            var participantes = new HashSet<(string Nombre, string Email)>();
            var resultadosPorVotacion = new List<(int VotacionId, string VotacionTitulo, List<(string Opcion, string Descripcion, int Cantidad)>)>();

            foreach (var votacion in asamblea.Votacions)
            {
                var resultados = new List<(string, string, int)>();
                foreach (var opcion in votacion.OpcionVotacions)
                {
                    int cantidad = votacion.Votos.Count(v => v.OpcionId == opcion.Id);
                    resultados.Add((opcion.Texto, opcion.Descripcion ?? "", cantidad));
                }
                resultadosPorVotacion.Add((votacion.Id, votacion.Titulo, resultados));

                foreach (var voto in votacion.Votos)
                {
                    if (voto.Usuario != null)
                        participantes.Add((voto.Usuario.Nombre + " " + voto.Usuario.Apellido, voto.Usuario.Email));
                }
            }

            // Generar PDF usando iTextSharp
            var document = new Document();
            using (var stream = new MemoryStream())
            {
                PdfWriter.GetInstance(document, stream);
                document.Open();

                // Creador
                document.Add(new Paragraph($"Creador: {creador}"));
                document.Add(new Paragraph($"Reporte de Asamblea: {asamblea.Titulo}"));
                document.Add(new Paragraph($"Fecha: {asamblea.Fecha:dd/MM/yyyy}"));
                document.Add(new Paragraph(" "));

                foreach (var votacion in resultadosPorVotacion)
                {
                    document.Add(new Paragraph($"Votación ID: {votacion.VotacionId}"));
                    document.Add(new Paragraph($"Título: {votacion.VotacionTitulo}"));

                    // Opciones y descripciones
                    foreach (var opcion in votacion.Item3)
                    {
                        document.Add(new Paragraph($"\"{opcion.Opcion}\": \"{opcion.Descripcion}\""));
                    }

                    document.Add(new Paragraph(" "));

                    // Gráfica de barras simple (usando PdfPTable como barras)
                    document.Add(new Paragraph("Resultados:"));
                    var maxVotos = votacion.Item3.Max(x => x.Cantidad);
                    var table = new PdfPTable(2) { WidthPercentage = 80 };
                    table.AddCell("Opción");
                    table.AddCell("Votos");

                    foreach (var opcion in votacion.Item3)
                    {
                        table.AddCell(opcion.Opcion);

                        // "Dibuja" una barra con caracteres según la cantidad de votos
                        int barLength = maxVotos > 0 ? (int)Math.Round((opcion.Cantidad / (double)maxVotos) * 30) : 0;
                        string bar = new string('█', barLength) + $" ({opcion.Cantidad})";
                        table.AddCell(bar);
                    }
                    document.Add(table);

                    document.Add(new Paragraph(" "));
                }

                // Participantes
                document.Add(new Paragraph("Participantes:"));
                foreach (var p in participantes)
                {
                    document.Add(new Paragraph($"{p.Nombre} ({p.Email})"));
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


        // GET: Reportes/CrearReportePrueba
        public IActionResult CrearReportePrueba()
        {
            // Usa un ID de usuario válido para GeneradoPor
            var usuarioId = _context.Usuarios.Select(u => u.Id).FirstOrDefault();
            if (usuarioId == 0)
                return Content("No hay usuarios en la base de datos.");

            var nuevoReporte = new Reporte
            {
                Tipo = "Prueba",
                Titulo = "Reporte de prueba manual",
                FechaGeneracion = DateTime.Now,
                GeneradoPor = usuarioId,
                UrlArchivo = "prueba.pdf",
                Formato = "PDF"
            };

            _context.Reportes.Add(nuevoReporte);
            _context.SaveChanges();

            return Content($"Reporte creado con ID: {nuevoReporte.Id}");
        }

    }

}
