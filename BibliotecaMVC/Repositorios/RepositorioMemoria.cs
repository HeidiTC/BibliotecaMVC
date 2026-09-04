using BibliotecaMVC.Models;

namespace BibliotecaMVC.Repositorios
{
    public class RepositorioMemoria : IRepositorioLibro
    {
        private List<Libro> libros = new List<Libro>()
        {
            new Libro
            {
                ID = 1,
                Titulo = "Clean Code",
                Autor = "Robert Martin",
                Categoria = "Programación",
                Precio = 35.50M,
                Disponible = true
            },

            new Libro
            {
                ID = 2,
                Titulo = "Cien años de soledad",
                Autor = "Gabriel García Márquez",
                Categoria = "Literatura",
                Precio = 18M,
                Disponible = false
            },

            new Libro
            {
                ID = 3,
                Titulo = "El Principito",
                Autor = "Antoine de Saint-Exupéry",
                Categoria = "Infantil",
                Precio = 12.50M,
                Disponible = true
            },

            new Libro
            {
                ID = 4,
                Titulo = "Don Quijote",
                Autor = "Miguel de Cervantes",
                Categoria = "Novela",
                Precio = 22M,
                Disponible = false
            },

            new Libro
            {
                ID = 5,
                Titulo = "ASP.NET Core MVC",
                Autor = "Microsoft",
                Categoria = "Tecnología",
                Precio = 40M,
                Disponible = true
            }
        };

        public List<Libro> ObtenerTodos()
        {
            return libros;
        }
    }
}