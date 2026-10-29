using BibliotecaMVC.Data;
using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BibliotecaMVC.Controllers
{
    
    [Authorize]
    public class LibrosController : Controller
    {
        private readonly BibliotecaContext _context;

        
        private readonly ILogger<LibrosController> _logger;

        public LibrosController(BibliotecaContext context, ILogger<LibrosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        
        [Authorize(Roles = "Admin,Bibliotecario,Usuario")]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Consultando el listado de libros");

            try
            {
                var libros = await _context.Libros.ToListAsync();

                return View(libros);
            }
            catch (Exception ex)
            {
                // Registrar el error en el log
                _logger.LogError(ex, "Se produjo un error al consultar el listado de libros");
                throw;
            }
        }

        // Solo Admin, Bibliotecario y Usuario pueden ver los detalles de un libro
        [Authorize(Roles = "Admin,Bibliotecario,Usuario")]
        public async Task<IActionResult> Details(int id)
        {
            _logger.LogInformation("Consultando los detalles del libro con identificador {LibroId}", id);

            try
            {
                var libro = await _context.Libros.FindAsync(id);

                if (libro == null)
                {
                    // El libro no fue encontrado
                    _logger.LogWarning("No se encontró el libro solicitado con identificador {LibroId}", id);
                    return NotFound();
                }

                return View(libro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al consultar los detalles del libro con identificador {LibroId}", id);
                throw;
            }
        }

        // Solo Admin y Bibliotecario pueden crear libros
        [Authorize(Roles = "Admin,Bibliotecario")]
        public IActionResult Create()
        {
            _logger.LogInformation("Iniciando la operación de creación de un libro");
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Bibliotecario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Libro libro)
        {
            _logger.LogInformation("Iniciando la creación de un libro");

            if (!ModelState.IsValid)
            {
                // Advertencia: el formulario llegó incompleto o con datos incorrectos
                _logger.LogWarning("El libro enviado no cumple con los requerimientos mínimos");
                return View(libro);
            }

            try
            {
                _context.Libros.Add(libro);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Libro creado correctamente con identificador {LibroId}", libro.ID);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al crear el libro");
                throw;
            }

            TempData["SuccessMessage"] = "Libro guardado correctamente";

            return RedirectToAction(nameof(Index));
        }

        // Solo Admin y Bibliotecario pueden editar libros
        [Authorize(Roles = "Admin,Bibliotecario")]
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Iniciando la operación de edición del libro con identificador {LibroId}", id);

            try
            {
                var libro = await _context.Libros.FindAsync(id);

                if (libro == null)
                {
                    _logger.LogWarning("No se encontró el libro solicitado para editar con identificador {LibroId}", id);
                    return NotFound();
                }

                return View(libro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al consultar el libro para editar con identificador {LibroId}", id);
                throw;
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Bibliotecario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Libro libro)
        {
            _logger.LogInformation("Iniciando la edición del libro con identificador {LibroId}", id);

            if (id != libro.ID)
            {
                // Advertencia: el id de la ruta no coincide con el del formulario
                _logger.LogWarning("El identificador {LibroId} no coincide con el libro enviado ({LibroEnviadoId})", id, libro.ID);
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("El libro enviado para editar no cumple con los requerimientos mínimos");
                return View(libro);
            }

            try
            {
                _context.Libros.Update(libro);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Libro con identificador {LibroId} actualizado correctamente", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al editar el libro con identificador {LibroId}", id);
                throw;
            }

            TempData["SuccessMessage"] = "Libro actualizado correctamente";

            return RedirectToAction(nameof(Index));
        }

        // Solo Admin y Bibliotecario pueden eliminar libros
        [HttpPost]
        [Authorize(Roles = "Admin,Bibliotecario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Iniciando la eliminación del libro con identificador {LibroId}", id);

            try
            {
                var libro = await _context.Libros.FindAsync(id);

                if (libro == null)
                {
                    _logger.LogWarning("No se encontró el libro solicitado para eliminar con identificador {LibroId}", id);
                    return NotFound();
                }

                _context.Libros.Remove(libro);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Libro con identificador {LibroId} eliminado correctamente", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al eliminar el libro con identificador {LibroId}", id);
                throw;
            }

            TempData["SuccessMessage"] = "Libro eliminado correctamente";

            return RedirectToAction(nameof(Index));
        }
    }
}
