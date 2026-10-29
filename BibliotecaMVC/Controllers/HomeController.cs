using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace BibliotecaMVC.Controllers
{
    public class HomeController : Controller
    {
        // Semana 15: logger para dejar registro cuando se muestra la página de error
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Libros()
        {
            return View();

        }
        public IActionResult Autores()
        {
            return View();

        }
        public IActionResult Acercade()
        {
            return View();

        }
        public IActionResult Prestamos()
        {
            return View();

        }
        public IActionResult Usuarios()
        {
            return View();

        }

        // Página de error personalizada
        // - UseExceptionHandler la llama cuando hay una excepción no controlada (codigo viene vacío)
        // - UseStatusCodePagesWithReExecute la llama con el código HTTP (ej. 404 cuando no existe el registro)
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? codigo)
        {
            var modelo = new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
                CodigoEstado = codigo
            };

            if (codigo == null)
            {
                // Excepción no controlada: el detalle técnico ya quedó en el LogError del controlador
                _logger.LogError("Se mostró la página de error al usuario. Código de referencia: {RequestId}", modelo.RequestId);
            }
            else
            {
                // Errores como 404 o 400 son advertencias, no fallas del servidor
                _logger.LogWarning("Se mostró la página de error con código {CodigoEstado}", codigo);
            }

            return View(modelo);
        }
    }
}
