using BibliotecaMVC.Models;

namespace BibliotecaMVC.Services
{
    public class AutorService : IAutorService
    {
        private readonly List<Autor> _autores = new List<Autor>
        {
            new Autor
            {
                ID = 1,
                Nombre = "Gabriel",
                Apellido = "García Márquez",
                Nacionalidad = "Colombiana",
                FechaNacimiento = new DateTime(1927, 3, 6),
                Activo = true
            },

            new Autor
            {
                ID = 2,
                Nombre = "Isabel",
                Apellido = "Allende",
                Nacionalidad = "Chilena",
                FechaNacimiento = new DateTime(1942, 8, 2),
                Activo = true
            },

            new Autor
            {
                ID = 3,
                Nombre = "Mario",
                Apellido = "Vargas Llosa",
                Nacionalidad = "Peruana",
                FechaNacimiento = new DateTime(1936, 3, 28),
                Activo = true
            }
        };

        public List<Autor> ObtenerTodos()
        {
            return _autores;
        }

        public Autor? ObtenerPorId(int id)
        {
            return _autores.FirstOrDefault(x => x.ID == id);
        }

        public void Crear(Autor autor)
        {
            autor.ID = _autores.Max(a => a.ID) + 1;
            _autores.Add(autor);
        }

        public bool Editar(Autor autor)
        {
            Autor? autorExistente =
                _autores.FirstOrDefault(x => x.ID == autor.ID);

            if (autorExistente == null)
            {
                return false;
            }

            autorExistente.Nombre = autor.Nombre;
            autorExistente.Apellido = autor.Apellido;
            autorExistente.Nacionalidad = autor.Nacionalidad;
            autorExistente.FechaNacimiento = autor.FechaNacimiento;
            autorExistente.Activo = autor.Activo;

            return true;
        }

        public void Eliminar(int id)
        {
            Autor? autor = _autores.FirstOrDefault(x => x.ID == id);

            if (autor != null)
            {
                _autores.Remove(autor);
            }
        }
    }
}