using BibliotecaMVC.Models;

namespace BibliotecaMVC.Services
{
    public interface IAutorService
    {
        List<Autor> ObtenerTodos();

        Autor? ObtenerPorId(int id);

        void Crear(Autor autor);

        bool Editar(Autor autor);

        void Eliminar(int id);
    }
}