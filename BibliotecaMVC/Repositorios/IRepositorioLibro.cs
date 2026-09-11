using BibliotecaMVC.Models;

namespace BibliotecaMVC.Repositorios
{
    public interface IRepositorioLibro
    {
        IEnumerable<Libro> ObtenerTodos();
    }
}