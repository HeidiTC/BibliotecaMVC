using BibliotecaMVC.Models;
using BibliotecaMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers
{
    public class AutoresController : Controller
    {
        private readonly IAutorService _autorService;

        public AutoresController(IAutorService autorService)
        {
            _autorService = autorService;
        }

        public IActionResult Index()
        {
            var autores = _autorService.ObtenerTodos();

            return View(autores);
        }

        public IActionResult Details(int id)
        {
            Autor? autor = _autorService.ObtenerPorId(id);

            if (autor == null)
            {
                return NotFound();
            }

            return View(autor);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Autor autor)
        {
            if (ModelState.IsValid)
            {
                _autorService.Crear(autor);

                return RedirectToAction(nameof(Index));
            }

            return View(autor);
        }

        public IActionResult Edit(int id)
        {
            Autor? autor = _autorService.ObtenerPorId(id);

            if (autor == null)
            {
                return NotFound();
            }

            return View(autor);
        }

        [HttpPost]
        public IActionResult Edit(Autor autor)
        {
            if (ModelState.IsValid)
            {
                bool actualizado = _autorService.Editar(autor);

                if (!actualizado)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Index));
            }

            return View(autor);
        }

        public IActionResult Delete(int id)
        {
            Autor? autor = _autorService.ObtenerPorId(id);

            if (autor == null)
            {
                return NotFound();
            }

            return View(autor);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            _autorService.Eliminar(id);

            return RedirectToAction(nameof(Index));
        }
    }
}