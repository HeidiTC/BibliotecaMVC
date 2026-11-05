using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.Models
{
    public class Libro
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        public string Titulo { get; set; } = "";

        [Required(ErrorMessage = "El autor es obligatorio")]
        public string Autor { get; set; } = "";

        [Required(ErrorMessage = "La categoría es obligatoria")]
        public string Categoria { get; set; } = "";

        [Range(0, 10000, ErrorMessage = "El precio debe estar entre 0 y 10,000")]
        public decimal Precio { get; set; }

        public bool Disponible { get; set; }
    }
}