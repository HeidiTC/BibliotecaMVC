using BibliotecaMVC.Data;
using BibliotecaMVC.Models;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BibliotecaMVC.Controllers
{
    
    [Authorize(Roles = "Admin,Bibliotecario")]
    public class ReportesController : Controller
    {
        private readonly BibliotecaContext _context;
        private readonly ILogger<ReportesController> _logger;

        public ReportesController(BibliotecaContext context, ILogger<ReportesController> logger)
        {
            _context = context;
            _logger = logger;
        }

                      public async Task<IActionResult> Index(string? autor, string? categoria)
        {
            _logger.LogInformation("Abriendo la página de reportes de libros");

            try
            {
                // Listas para los filtros (se llenan con los datos reales de la base de datos)
                ViewBag.Autores = await _context.Libros
                    .Select(libro => libro.Autor)
                    .Distinct()
                    .OrderBy(nombre => nombre)
                    .ToListAsync();

                ViewBag.Categorias = await _context.Libros
                    .Select(libro => libro.Categoria)
                    .Distinct()
                    .OrderBy(nombre => nombre)
                    .ToListAsync();

                // Se guardan los filtros elegidos para que el formulario los recuerde
                ViewBag.AutorSeleccionado = autor;
                ViewBag.CategoriaSeleccionada = categoria;

                var libros = await ObtenerLibros(autor, categoria);

                return View(libros);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al cargar la página de reportes");
                throw;
            }
        }

        // =====================================================================
        // PDF: ver el reporte en formato .pdf con QuestPDF
        // =====================================================================
        public async Task<IActionResult> VerPdf(string? autor, string? categoria)
        {
            _logger.LogInformation("Generando el reporte PDF de libros para verlo en el navegador");

            try
            {
                var libros = await ObtenerLibros(autor, categoria);

                return File(GenerarReportePdf(libros, autor, categoria), "application/pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al generar el reporte PDF de libros");
                TempData["ErrorMessage"] = "No se pudo generar el reporte PDF. Intente de nuevo.";
                return RedirectToAction(nameof(Index), new { autor, categoria });
            }
        }


        // =====================================================================
        // PDF: descargar el reporte en formato .pdf con QuestPDF
        // =====================================================================
        public async Task<IActionResult> GenerarPdf(string? autor, string? categoria)
        {
            _logger.LogInformation("Generando el reporte PDF de libros para descargar");

            try
            {
                var libros = await ObtenerLibros(autor, categoria);

                // Nombre del archivo con la fecha actual (para que no se sobreescriba)
                var nombreArchivo = $"ReporteLibros_{DateTime.Now:yyyyMMdd}.pdf";

                return File(GenerarReportePdf(libros, autor, categoria), "application/pdf", nombreArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al descargar el reporte PDF de libros");
                TempData["ErrorMessage"] = "No se pudo generar el reporte PDF. Intente de nuevo.";
                return RedirectToAction(nameof(Index), new { autor, categoria });
            }
        }


        // =====================================================================
        // EXCEL: descargar el reporte en formato .xlsx con ClosedXML
        // =====================================================================


        public async Task<IActionResult> ExportarExcel(string? autor, string? categoria)
        {
            _logger.LogInformation("Generando el reporte Excel de libros");

            try
            {
                var libros = await ObtenerLibros(autor, categoria);

                var nombreArchivo = $"ReporteLibros_{DateTime.Now:yyyyMMdd}.xlsx";

                // Tipo de contenido oficial de los archivos de Excel (.xlsx)
                return File(GenerarReporteExcel(libros, autor, categoria),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    nombreArchivo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al generar el reporte Excel de libros");
                TempData["ErrorMessage"] = "No se pudo generar el reporte Excel. Intente de nuevo.";
                return RedirectToAction(nameof(Index), new { autor, categoria });
            }
        }

        // =====================================================================
        // Obtiene la lista de libros según los filtros elegidos por el usuario
        // =====================================================================
        private async Task<List<Libro>> ObtenerLibros(string? autor, string? categoria)
        {
            var consulta = _context.Libros
                .AsNoTracking()   
                .AsQueryable();

            // Filtro por autor (si se eligió uno)
            if (!string.IsNullOrEmpty(autor))
            {
                consulta = consulta.Where(libro => libro.Autor == autor);
            }

            // Filtro por categoría (si se eligió una)
            if (!string.IsNullOrEmpty(categoria))
            {
                consulta = consulta.Where(libro => libro.Categoria == categoria);
            }

            return await consulta
                .OrderBy(libro => libro.Titulo)
                .ToListAsync();
        }

        // Texto que explica qué filtros se usaron (se muestra en el PDF y en el Excel)
        private static string TextoFiltros(string? autor, string? categoria)
        {
            if (string.IsNullOrEmpty(autor) && string.IsNullOrEmpty(categoria))
            {
                return "Filtros: todos los libros";
            }

            var filtroAutor = string.IsNullOrEmpty(autor) ? "Todos" : autor;
            var filtroCategoria = string.IsNullOrEmpty(categoria) ? "Todas" : categoria;

            return $"Filtros: Autor = {filtroAutor} | Categoría = {filtroCategoria}";
        }

        // =====================================================================
        // DISEÑO DEL PDF CON QUESTPDF
        // =====================================================================
        private static byte[] GenerarReportePdf(IReadOnlyCollection<Libro> libros, string? autor, string? categoria)
        {
            // Datos del resumen
            var disponibles = libros.Count(libro => libro.Disponible);
            var noDisponibles = libros.Count - disponibles;

            return Document.Create(document => document.Page(page =>
            {
                page.Size(PageSizes.Letter);
                page.Margin(42);
                page.PageColor(Colors.Grey.Lighten5);

                // ---------------- ENCABEZADO ----------------
                page.Header().Column(column =>
                {
                    // Nombre del sistema
                    column.Item().Text("BIBLIOTECAMVC - BIBLIOTECA UNIVERSITARIA")
                        .FontSize(11)
                        .Bold()
                        .FontColor(Colors.Blue.Darken2);

                    // Título del reporte
                    column.Item().Text("Reporte de libros")
                        .FontSize(26)
                        .Bold()
                        .FontColor(Colors.Grey.Darken3);

                    // Fecha de generación
                    column.Item().Text($"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Darken1);

                    // Filtros aplicados
                    column.Item().Text(TextoFiltros(autor, categoria))
                        .FontSize(10)
                        .Italic()
                        .FontColor(Colors.Grey.Darken1);

                    column.Item().PaddingTop(14).LineHorizontal(2).LineColor(Colors.Blue.Medium);
                });

                // ---------------- CONTENIDO ----------------
                page.Content().PaddingVertical(22).Column(column =>
                {
                    // Recuadro azul con el total de libros
                    column.Item().Background(Colors.Blue.Darken2).Padding(14).Row(row =>
                    {
                        row.RelativeItem().Text("Libros Registrados")
                            .FontSize(11)
                            .Bold()
                            .FontColor(Colors.White);

                        row.AutoItem().Text(libros.Count.ToString())
                            .FontSize(14)
                            .Bold()
                            .FontColor(Colors.White);
                    });

                    // Resumen: disponibles y no disponibles
                    column.Item().PaddingTop(8).Row(row =>
                    {
                        row.RelativeItem().Text($"Disponibles: {disponibles}")
                            .FontSize(10)
                            .Bold()
                            .FontColor(Colors.Green.Darken2);

                        row.RelativeItem().AlignRight().Text($"No disponibles: {noDisponibles}")
                            .FontSize(10)
                            .Bold()
                            .FontColor(Colors.Red.Darken2);
                    });

                    // Si no hay libros con esos filtros, se muestra un mensaje en lugar de la tabla
                    if (libros.Count == 0)
                    {
                        column.Item().PaddingTop(18).Text("No hay libros registrados para los filtros seleccionados.")
                            .FontSize(11)
                            .FontColor(Colors.Grey.Darken2);
                        return;
                    }

                    // Tabla de libros
                    column.Item().PaddingTop(18).Table(table =>
                    {
                        // Anchos: el ID es fijo y las demás columnas se reparten el espacio
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(36);      
                            columns.RelativeColumn(2.5f);   
                            columns.RelativeColumn(2f);     
                            columns.RelativeColumn(1.5f);   
                            columns.RelativeColumn(1.1f);   
                            columns.RelativeColumn(1.4f);   
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCellStyle).Text("ID");
                            header.Cell().Element(HeaderCellStyle).Text("TÍTULO");
                            header.Cell().Element(HeaderCellStyle).Text("AUTOR");
                            header.Cell().Element(HeaderCellStyle).Text("CATEGORÍA");
                            header.Cell().Element(HeaderCellStyle).AlignRight().Text("PRECIO");
                            header.Cell().Element(HeaderCellStyle).AlignCenter().Text("ESTADO");
                        });

                        foreach (var libro in libros)
                        {
                            table.Cell().Element(BodyCellStyle).Text(libro.ID.ToString());
                            table.Cell().Element(BodyCellStyle).Text(libro.Titulo);
                            table.Cell().Element(BodyCellStyle).Text(libro.Autor);
                            table.Cell().Element(BodyCellStyle).Text(libro.Categoria);
                            table.Cell().Element(BodyCellStyle).AlignRight().Text($"${libro.Precio:N2}");

                            // El estado cambia de color según la disponibilidad
                            table.Cell().Element(BodyCellStyle).AlignCenter()
                                .Text(libro.Disponible ? "Disponible" : "No disponible")
                                .FontColor(libro.Disponible ? Colors.Green.Darken2 : Colors.Red.Darken2);
                        }
                    });
                });

                // ---------------- PIE DE PÁGINA ----------------
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("BibliotecaMVC - Página ");
                    text.CurrentPageNumber();
                    text.Span(" de ");
                    text.TotalPages();
                });
            })).GeneratePdf();   
        }

        // Estilo de las celdas del encabezado de la tabla
        static IContainer HeaderCellStyle(IContainer container) => container
            .Background(Colors.Blue.Darken2)
            .PaddingVertical(9)
            .PaddingHorizontal(6)
            .DefaultTextStyle(text => text.FontSize(9).Bold().FontColor(Colors.White));

        // Estilo de las celdas con los datos de cada libro
        static IContainer BodyCellStyle(IContainer container) => container
            .Background(Colors.White)
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(8)
            .PaddingHorizontal(6)
            .DefaultTextStyle(text => text.FontSize(9).FontColor(Colors.Grey.Darken3));

        // =====================================================================
        // DISEÑO DEL EXCEL CON CLOSEDXML
        // =====================================================================
        private static byte[] GenerarReporteExcel(IReadOnlyCollection<Libro> libros, string? autor, string? categoria)
        {
            // Colores iguales a los del PDF para que los dos reportes se vean parecidos
            var azulOscuro = XLColor.FromHtml("#1565C0");
            var grisClaro = XLColor.FromHtml("#E0E0E0");

            using var libroExcel = new XLWorkbook();
            var hoja = libroExcel.Worksheets.Add("Reporte de Libros");

            // ---------------- ENCABEZADO DEL REPORTE ----------------
            hoja.Cell(1, 1).Value = "BIBLIOTECAMVC - BIBLIOTECA UNIVERSITARIA";
            hoja.Range(1, 1, 1, 6).Merge();
            hoja.Cell(1, 1).Style.Font.Bold = true;
            hoja.Cell(1, 1).Style.Font.FontColor = azulOscuro;

            hoja.Cell(2, 1).Value = "Reporte de libros";
            hoja.Range(2, 1, 2, 6).Merge();
            hoja.Cell(2, 1).Style.Font.Bold = true;
            hoja.Cell(2, 1).Style.Font.FontSize = 18;

            // Fecha de generación
            hoja.Cell(3, 1).Value = $"Generado el {DateTime.Now:dd/MM/yyyy HH:mm}";
            hoja.Range(3, 1, 3, 6).Merge();
            hoja.Cell(3, 1).Style.Font.FontColor = XLColor.Gray;

            // Filtros aplicados
            hoja.Cell(4, 1).Value = TextoFiltros(autor, categoria);
            hoja.Range(4, 1, 4, 6).Merge();
            hoja.Cell(4, 1).Style.Font.Italic = true;
            hoja.Cell(4, 1).Style.Font.FontColor = XLColor.Gray;

            // ---------------- ENCABEZADOS DE LA TABLA (fila 6) ----------------
            int filaEncabezado = 6;
            string[] columnas = { "ID", "Título", "Autor", "Categoría", "Precio", "Estado" };

            for (int i = 0; i < columnas.Length; i++)
            {
                hoja.Cell(filaEncabezado, i + 1).Value = columnas[i];
            }

            var rangoEncabezado = hoja.Range(filaEncabezado, 1, filaEncabezado, columnas.Length);
            rangoEncabezado.Style.Font.Bold = true;
            rangoEncabezado.Style.Font.FontColor = XLColor.White;
            rangoEncabezado.Style.Fill.BackgroundColor = azulOscuro;
            rangoEncabezado.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // ---------------- DATOS DE LOS LIBROS ----------------
            int fila = filaEncabezado + 1;

            foreach (var libro in libros)
            {
                hoja.Cell(fila, 1).Value = libro.ID;
                hoja.Cell(fila, 2).Value = libro.Titulo;
                hoja.Cell(fila, 3).Value = libro.Autor;
                hoja.Cell(fila, 4).Value = libro.Categoria;
                hoja.Cell(fila, 5).Value = libro.Precio;   
                hoja.Cell(fila, 6).Value = libro.Disponible ? "Disponible" : "No disponible";

                // Color del estado según la disponibilidad
                hoja.Cell(fila, 6).Style.Font.FontColor = libro.Disponible ? XLColor.DarkGreen : XLColor.DarkRed;

                fila++;
            }

            int ultimaFila = fila - 1;

            // Formato de los datos (solo si hay libros)
            if (libros.Count > 0)
            {
                var rangoDatos = hoja.Range(filaEncabezado + 1, 1, ultimaFila, columnas.Length);
                rangoDatos.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                rangoDatos.Style.Border.BottomBorderColor = grisClaro;

                // ID y Estado centrados, Precio con formato de moneda
                hoja.Range(filaEncabezado + 1, 1, ultimaFila, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                hoja.Range(filaEncabezado + 1, 6, ultimaFila, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                hoja.Range(filaEncabezado + 1, 5, ultimaFila, 5).Style.NumberFormat.Format = "$#,##0.00";
            }

            // Borde exterior de toda la tabla (encabezado + datos)
            var rangoTabla = hoja.Range(filaEncabezado, 1, Math.Max(ultimaFila, filaEncabezado), columnas.Length);
            rangoTabla.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rangoTabla.Style.Border.OutsideBorderColor = azulOscuro;

            // Filtro de Excel en los encabezados: el usuario puede seguir trabajando los datos
            rangoTabla.SetAutoFilter();

            // ---------------- RESUMEN ----------------
            int filaResumen = Math.Max(ultimaFila, filaEncabezado) + 2;
            var disponibles = libros.Count(libro => libro.Disponible);

            hoja.Cell(filaResumen, 1).Value = "Total de libros:";
            hoja.Cell(filaResumen, 2).Value = libros.Count;
            hoja.Cell(filaResumen + 1, 1).Value = "Disponibles:";
            hoja.Cell(filaResumen + 1, 2).Value = disponibles;
            hoja.Cell(filaResumen + 2, 1).Value = "No disponibles:";
            hoja.Cell(filaResumen + 2, 2).Value = libros.Count - disponibles;

            var rangoResumen = hoja.Range(filaResumen, 1, filaResumen + 2, 2);
            rangoResumen.Style.Font.Bold = true;
            rangoResumen.Style.Fill.BackgroundColor = XLColor.FromHtml("#E3F2FD");
            rangoResumen.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            hoja.Range(filaResumen, 2, filaResumen + 2, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

            // Ancho automático de columnas según el contenido
            hoja.Columns(1, columnas.Length).AdjustToContents(filaEncabezado, filaResumen + 2);

            // Inmovilizar la fila de encabezados al bajar en la hoja
            hoja.SheetView.FreezeRows(filaEncabezado);

            // Se guarda el libro en memoria y se devuelve como byte[] (igual que el PDF)
            using var memoria = new MemoryStream();
            libroExcel.SaveAs(memoria);
            return memoria.ToArray();
        }
    }
}
