using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BibliotecaMVC.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace BibliotecaMVC.Controllers
{
    [Authorize] // Requiere que el usuario esté autenticado para acceder a cualquier acción del controlador
    public class CategoriasController : Controller
    {
        private readonly string _connectionString;

        // Inyección de dependencias para el logger
        private readonly ILogger<CategoriasController> _logger;

        public CategoriasController(IConfiguration configuration, ILogger<CategoriasController> logger)
        {
            _connectionString = configuration.GetConnectionString("BibliotecaDB");
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Consultando el listado de categorías");

            var categorias = new List<Categoria>();

            try
            {
                using (var conexion = new SqlConnection(_connectionString))
                {
                    var sql = "SELECT ID, Nombre, Descripcion FROM Categorias";

                    using (var comando = new SqlCommand(sql, conexion))
                    {
                        conexion.Open();

                        using (var lector = comando.ExecuteReader())
                        {
                            while (lector.Read())
                            {
                                categorias.Add(new Categoria
                                {
                                    ID = lector.GetInt32(0),
                                    Nombre = lector.GetString(1),
                                    Descripcion = lector.IsDBNull(2)
                                        ? null
                                        : lector.GetString(2)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Se registra el error y se lanza para que lo atrape el manejo global (página de error)
                // Ojo: no se registra la cadena de conexión porque es información sensible
                _logger.LogError(ex, "Se produjo un error al consultar el listado de categorías");
                throw;
            }

            return View(categorias);
        }

        public IActionResult Create()
        {
            _logger.LogInformation("Iniciando la operación de creación de una categoría");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Categoria categoria)
        {
            _logger.LogInformation("Iniciando la creación de una categoría");

            if (categoria == null || string.IsNullOrWhiteSpace(categoria.Nombre))
            {
                // Advertencia: llegó la categoría sin nombre
                _logger.LogWarning("La categoría enviada no cumple con los requerimientos mínimos (falta el nombre)");
                ModelState.AddModelError("Nombre", "El nombre es obligatorio");
                return View(categoria);
            }

            try
            {
                using (var conexion = new SqlConnection(_connectionString))
                {
                    var sql = "INSERT INTO Categorias (Nombre, Descripcion) VALUES (@Nombre, @Descripcion)";

                    using (var comando = new SqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@Nombre", categoria.Nombre);
                        comando.Parameters.AddWithValue(
                            "@Descripcion",
                            (object)categoria.Descripcion ?? System.DBNull.Value
                        );

                        conexion.Open();
                        comando.ExecuteNonQuery();
                    }
                }

                _logger.LogInformation("Categoría {NombreCategoria} creada correctamente", categoria.Nombre);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al crear la categoría");
                throw;
            }

            TempData["SuccessMessage"] = "Categoria guardada correctamente";

            return RedirectToAction("Index");
        }

        // Mostrar formulario para editar
        public IActionResult Edit(int id)
        {
            _logger.LogInformation("Iniciando la operación de edición de la categoría con identificador {CategoriaId}", id);

            Categoria categoria = null;

            try
            {
                using (var conexion = new SqlConnection(_connectionString))
                {
                    var sql = "SELECT ID, Nombre, Descripcion FROM Categorias WHERE ID = @ID";

                    using (var comando = new SqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@ID", id);

                        conexion.Open();

                        using (var lector = comando.ExecuteReader())
                        {
                            if (lector.Read())
                            {
                                categoria = new Categoria
                                {
                                    ID = lector.GetInt32(0),
                                    Nombre = lector.GetString(1),
                                    Descripcion = lector.IsDBNull(2)
                                        ? null
                                        : lector.GetString(2)
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al consultar la categoría para editar con identificador {CategoriaId}", id);
                throw;
            }

            if (categoria == null)
            {
                // Advertencia: buscaron una categoría que no existe
                _logger.LogWarning("No se encontró la categoría solicitada con identificador {CategoriaId}", id);
                return NotFound();
            }

            return View(categoria);
        }

        // Guardar cambios despues de editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Categoria categoria)
        {
            _logger.LogInformation("Iniciando la edición de la categoría con identificador {CategoriaId}", categoria?.ID);

            if (categoria == null || string.IsNullOrWhiteSpace(categoria.Nombre))
            {
                _logger.LogWarning("La categoría enviada para editar no cumple con los requerimientos mínimos (falta el nombre)");
                ModelState.AddModelError("Nombre", "El nombre es obligatorio");
                return View(categoria);
            }

            try
            {
                using (var conexion = new SqlConnection(_connectionString))
                {
                    var sql = @"UPDATE Categorias
                                SET Nombre = @Nombre,
                                    Descripcion = @Descripcion
                                WHERE ID = @ID";

                    using (var comando = new SqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@ID", categoria.ID);
                        comando.Parameters.AddWithValue("@Nombre", categoria.Nombre);
                        comando.Parameters.AddWithValue(
                            "@Descripcion",
                            (object)categoria.Descripcion ?? System.DBNull.Value
                        );

                        conexion.Open();

                        // ExecuteNonQuery devuelve cuántas filas se modificaron
                        int filas = comando.ExecuteNonQuery();

                        if (filas == 0)
                        {
                            _logger.LogWarning("No se encontró la categoría a editar con identificador {CategoriaId}", categoria.ID);
                        }
                        else
                        {
                            _logger.LogInformation("Categoría con identificador {CategoriaId} actualizada correctamente", categoria.ID);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al editar la categoría con identificador {CategoriaId}", categoria.ID);
                throw;
            }

            TempData["SuccessMessage"] = "Categoría actualizada correctamente";

            return RedirectToAction("Index");
        }

        // Eliminar categoría
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            _logger.LogInformation("Iniciando la eliminación de la categoría con identificador {CategoriaId}", id);

            try
            {
                using (var conexion = new SqlConnection(_connectionString))
                {
                    var sql = "DELETE FROM Categorias WHERE ID = @ID";

                    using (var comando = new SqlCommand(sql, conexion))
                    {
                        comando.Parameters.AddWithValue("@ID", id);

                        conexion.Open();

                        int filas = comando.ExecuteNonQuery();

                        if (filas == 0)
                        {
                            _logger.LogWarning("No se encontró la categoría solicitada para eliminar con identificador {CategoriaId}", id);
                        }
                        else
                        {
                            _logger.LogInformation("Categoría con identificador {CategoriaId} eliminada correctamente", id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Se produjo un error al eliminar la categoría con identificador {CategoriaId}", id);
                throw;
            }

            TempData["SuccessMessage"] = "Categoría eliminada correctamente";

            return RedirectToAction("Index");
        }
    }
}
