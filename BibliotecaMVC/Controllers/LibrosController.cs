using BibliotecaMVC.Models;
using BibliotecaMVC.Repositorios;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers
{
    public class LibrosController : Controller
    {
        private readonly IRepositorioLibro _repositorioLibro;

        public LibrosController(IRepositorioLibro repositorioLibro)
        {
            _repositorioLibro = repositorioLibro;
        }

        public IActionResult Index()
        {
            var libros = _repositorioLibro.ObtenerTodos();

            return View(libros);
        }
    }
}
