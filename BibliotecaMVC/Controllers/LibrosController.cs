using BibliotecaMVC.Data;
using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Controllers
{
    
    [Authorize]
    public class LibrosController : Controller
    {
        private readonly BibliotecaContext _context;

        public LibrosController(BibliotecaContext context)
        {
            _context = context;
        }

        
        [Authorize(Roles = "Admin,Bibliotecario,Usuario")]
        public async Task<IActionResult> Index()
        {
            var libros = await _context.Libros.ToListAsync();

            return View(libros);
        }

        
        [Authorize(Roles = "Admin,Bibliotecario,Usuario")]
        public async Task<IActionResult> Details(int id)
        {
            var libro = await _context.Libros.FindAsync(id);

            if (libro == null)
            {
                return NotFound();
            }

            return View(libro);
        }

        
        [Authorize(Roles = "Admin,Bibliotecario")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Bibliotecario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Libro libro)
        {
            if (!ModelState.IsValid)
            {
                return View(libro);
            }

            _context.Libros.Add(libro);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Libro guardado correctamente";

            return RedirectToAction(nameof(Index));
        }

        
        [Authorize(Roles = "Admin,Bibliotecario")]
        public async Task<IActionResult> Edit(int id)
        {
            var libro = await _context.Libros.FindAsync(id);

            if (libro == null)
            {
                return NotFound();
            }

            return View(libro);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Bibliotecario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Libro libro)
        {
            if (id != libro.ID)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(libro);
            }

            _context.Libros.Update(libro);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Libro actualizado correctamente";

            return RedirectToAction(nameof(Index));
        }

        
        [HttpPost]
        [Authorize(Roles = "Admin,Bibliotecario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var libro = await _context.Libros.FindAsync(id);

            if (libro == null)
            {
                return NotFound();
            }

            _context.Libros.Remove(libro);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Libro eliminado correctamente";

            return RedirectToAction(nameof(Index));
        }
    }
}