
using System.ComponentModel.DataAnnotations;
using BibliotecaMVC.Models;

// Define el espacio de nombres del proyecto de pruebas.
namespace BibliotecaMVC.Tests;

// Agrupa las pruebas relacionadas con las reglas de validación de Libro.
public class LibroValidationTests
{
    // Indica a xUnit que el siguiente método es una prueba.
    [Fact]
    // Comprueba que el título del libro sea obligatorio.
    public void TituloEsObligatorio()
    {
        // Crea un libro con el título vacío para provocar la validación requerida.
        var libro = new Libro { Titulo = string.Empty, Autor = "Autor", Categoria = "Novela", Precio = 10m };

        // Ejecuta las validaciones definidas mediante atributos en Libro.
        var errores = Validate(libro);

        // Comprueba que exista un error asociado a la propiedad Titulo.
        Assert.Contains(errores, error => error.MemberNames.Contains(nameof(Libro.Titulo)));
    }

    // Indica a xUnit que el siguiente método es una prueba.
    [Fact]
    // Comprueba que el autor del libro sea obligatorio.
    public void AutorEsObligatorio()
    {
        // Crea un libro con el autor vacío.
        var libro = new Libro { Titulo = "Libro", Autor = string.Empty, Categoria = "Novela", Precio = 10m };

        // Ejecuta las validaciones del modelo.
        var errores = Validate(libro);

        // Comprueba que exista un error asociado a la propiedad Autor.
        Assert.Contains(errores, error => error.MemberNames.Contains(nameof(Libro.Autor)));
    }

    // Indica a xUnit que el siguiente método es una prueba que se repite con varios datos.
    [Theory]
    // Primer precio inválido.
    [InlineData(-1)]
    // Segundo precio inválido.
    [InlineData(-50)]
    // Comprueba que el precio no pueda ser negativo.
    public void PrecioNegativoNoEsValido(int precio)
    {
        // Crea un libro válido pero con precio negativo.
        var libro = new Libro { Titulo = "Libro", Autor = "Autor", Categoria = "Novela", Precio = precio };

        // Ejecuta las validaciones del modelo.
        var errores = Validate(libro);

        // Comprueba que el error corresponda a la propiedad Precio.
        Assert.Contains(errores, error => error.MemberNames.Contains(nameof(Libro.Precio)));
    }

    // Indica a xUnit que el siguiente método es una prueba.
    [Fact]
    // Comprueba que el límite exacto de precio 0 sea válido.
    public void PrecioCeroEsValido()
    {
        // Crea un libro con precio 0.
        var libro = new Libro { Titulo = "Libro", Autor = "Autor", Categoria = "Novela", Precio = 0m };

        // Ejecuta las validaciones del modelo.
        var errores = Validate(libro);

        // Comprueba que no haya errores para la propiedad Precio.
        Assert.DoesNotContain(errores, error => error.MemberNames.Contains(nameof(Libro.Precio)));
    }

    // Indica a xUnit que el siguiente método es una prueba.
    [Fact]
    // Comprueba que el título acepte caracteres especiales y tildes.
    public void TituloConCaracteresEspecialesEsValido()
    {
        // Crea un libro cuyo título contiene tildes y la letra ñ.
        var libro = new Libro { Titulo = "Cien años de soledad", Autor = "Gabriel García Márquez", Categoria = "Literatura", Precio = 18m };

        // Ejecuta las validaciones del modelo.
        var errores = Validate(libro);

        // Comprueba que el libro no tenga ningún error de validación.
        Assert.Empty(errores);
    }

    // Método auxiliar que concentra la ejecución de las validaciones del modelo.
    private static IReadOnlyCollection<ValidationResult> Validate(Libro libro)
    {
        // Crea el contexto de validación para la instancia recibida.
        var context = new ValidationContext(libro);
        // Prepara la colección donde se almacenarán los errores encontrados.
        var errores = new List<ValidationResult>();
        // Ejecuta todas las validaciones, incluso las propiedades no obligatorias.
        Validator.TryValidateObject(libro, context, errores, validateAllProperties: true);
        // Devuelve los errores para que cada prueba pueda analizarlos.
        return errores;
    }
}
