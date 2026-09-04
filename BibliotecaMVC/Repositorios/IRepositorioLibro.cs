using BibliotecaMVC.Models;

namespace BibliotecaMVC.Repositorios
{
    public interface IRepositorioLibro
    {
        List<Libro> ObtenerTodos();
    }
}