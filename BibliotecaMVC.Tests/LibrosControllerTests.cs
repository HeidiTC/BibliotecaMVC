using BibliotecaMVC.Controllers;
using BibliotecaMVC.Data;
using BibliotecaMVC.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace BibliotecaMVC.Tests;

public class LibrosControllerTests
{
    [Fact]
    // Comprueba que Index devuelva todos los libros guardados.
    public async Task Index_DevuelveLaListaDeLibros()
    {
        // Crea un contexto en memoria para la prueba.      
        await using var context = CreateContext();
        //Justifica la inserción de libros de prueba en la base de datos en memoria.
        context.Libros.AddRange(
            new Libro { Titulo = "Clean Code", Autor = "Robert Martin", Categoria = "Programación", Precio = 35.50m, Disponible = true },
            new Libro { Titulo = "El Principito", Autor = "Antoine de Saint-Exupéry", Categoria = "Infantil", Precio = 12.50m, Disponible = true });
        
        await context.SaveChangesAsync();
        // Crea el controlador usando el contexto y un logger simulado.
        var controller = CreateController(context);

        // Ejecuta la acción Index del controlador.
        var result = await controller.Index();

        // Comprueba que el resultado sea una vista.
        var view = Assert.IsType<ViewResult>(result);
        // Obtiene el modelo de la vista como una colección de libros.
        var libros = Assert.IsAssignableFrom<IEnumerable<Libro>>(view.Model);
        // Comprueba que la vista contenga los dos libros insertados.
        Assert.Equal(2, libros.Count());
    }

    [Fact]
    // Comprueba que Index funcione cuando no hay libros guardados.
    public async Task Index_SinLibros_DevuelveListaVacia()
    {
        // Crea un contexto vacío.
        await using var context = CreateContext();
        // Crea el controlador de prueba.
        var controller = CreateController(context);

        // Ejecuta la acción Index del controlador.
        var result = await controller.Index();

        // Comprueba que el resultado sea una vista.
        var view = Assert.IsType<ViewResult>(result);
        // Obtiene el modelo de la vista como una colección de libros.
        var libros = Assert.IsAssignableFrom<IEnumerable<Libro>>(view.Model);
        // Comprueba que la lista esté vacía.
        Assert.Empty(libros);
    }

    [Fact]
    // Comprueba que Details devuelva un libro existente.
    public async Task Details_ConLibroExistente_DevuelveElLibro()
    {
        // Crea una base de datos en memoria aislada.
        await using var context = CreateContext();
        // Crea un libro de prueba.
        var libro = new Libro { Titulo = "Don Quijote", Autor = "Miguel de Cervantes", Categoria = "Novela", Precio = 22m, Disponible = true };
        // Añade el libro al contexto.
        context.Libros.Add(libro);
        // Guarda el libro y genera su identificador.
        await context.SaveChangesAsync();
        // Crea el controlador de prueba.
        var controller = CreateController(context);

        // Solicita los detalles usando el identificador generado.
        var result = await controller.Details(libro.ID);

        // Comprueba que la respuesta sea una vista.
        var view = Assert.IsType<ViewResult>(result);
        // Comprueba que la vista tenga un modelo.
        Assert.NotNull(view.Model);
        // Comprueba que la vista reciba exactamente el libro guardado.
        Assert.Same(libro, view.Model);
    }

    [Fact]
    // Comprueba la respuesta cuando el libro no existe.
    public async Task Details_ConLibroInexistente_DevuelveNotFound()
    {
        // Crea un contexto vacío.
        await using var context = CreateContext();
        // Crea el controlador de prueba.
        var controller = CreateController(context);

        // Solicita los detalles de un identificador inexistente.
        var result = await controller.Details(999);

        // Comprueba que el controlador responda con NotFound.
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    // Comprueba que Create guarde un libro válido y redirija al listado.
    public async Task Create_ConLibroValido_GuardaElLibroYRedirigeAlIndice()
    {
        // Crea una base de datos en memoria vacía.
        await using var context = CreateContext();
        // Crea el controlador de prueba.
        var controller = CreateController(context);
        // Construye el libro válido que se enviará a la acción.
        var libro = new Libro { Titulo = "Cien años de soledad", Autor = "Gabriel García Márquez", Categoria = "Literatura", Precio = 18m, Disponible = true };

        // Ejecuta la acción Create con el libro.
        var result = await controller.Create(libro);

        // Comprueba que la acción devuelva una redirección.
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        // Comprueba que la redirección apunte a Index.
        Assert.Equal(nameof(LibrosController.Index), redirect.ActionName);
        // Comprueba que se haya guardado un único libro.
        Assert.Equal(1, await context.Libros.CountAsync());
        // Comprueba que el libro guardado tenga el título esperado.
        Assert.Equal("Cien años de soledad", (await context.Libros.SingleAsync()).Titulo);
    }

    [Fact]
    // Comprueba que Create no guarde datos cuando el modelo es inválido.
    public async Task Create_ConModeloInvalido_NoGuardaYDevuelveLaVista()
    {
        // Crea una base de datos en memoria vacía.
        await using var context = CreateContext();
        // Crea el controlador de prueba.
        var controller = CreateController(context);
        // Simula un error de validación del título.
        controller.ModelState.AddModelError(nameof(Libro.Titulo), "El título es obligatorio.");
        // Crea el libro que se enviará con el modelo inválido.
        var libro = new Libro { Titulo = "", Autor = "Autor", Categoria = "Novela", Precio = 10m };

        // Ejecuta la acción Create.
        var result = await controller.Create(libro);

        // Comprueba que se devuelva nuevamente la vista.
        var view = Assert.IsType<ViewResult>(result);
        // Comprueba que la vista conserve el modelo enviado.
        Assert.Same(libro, view.Model);
        // Comprueba que no se haya guardado ningún libro.
        Assert.Empty(context.Libros);
    }

    [Fact]
    // Comprueba que Edit actualice los datos de un libro existente.
    public async Task Edit_ConLibroValido_ActualizaElLibro()
    {
        // Genera un nombre único para la base de datos de esta prueba.
        var databaseName = Guid.NewGuid().ToString();
        // Declara la variable donde se almacenará el identificador del libro.
        int libroId;
        // Crea un contexto separado para preparar los datos iniciales.
        await using (var seedContext = CreateContext(databaseName))
        {
            // Crea el libro original.
            var libro = new Libro { Titulo = "Título original", Autor = "Autor", Categoria = "Novela", Precio = 15m, Disponible = true };
            // Añade el libro al contexto inicial.
            seedContext.Libros.Add(libro);
            // Guarda el libro en la base de datos en memoria.
            await seedContext.SaveChangesAsync();
            // Conserva el identificador generado.
            libroId = libro.ID;
        }

        // Crea otro contexto para simular una nueva petición HTTP.
        await using var context = CreateContext(databaseName);
        // Crea el controlador con el contexto de la petición.
        var controller = CreateController(context);
        // Crea la versión modificada del libro.
        var libroEditado = new Libro
        {
            // Mantiene el identificador original.
            ID = libroId,
            // Define el nuevo título.
            Titulo = "Título actualizado",
            // Mantiene el autor.
            Autor = "Autor",
            // Mantiene la categoría.
            Categoria = "Novela",
            // Define el nuevo precio.
            Precio = 20m,
            // Marca el libro como no disponible.
            Disponible = false
        };

        // Ejecuta la acción Edit.
        var result = await controller.Edit(libroId, libroEditado);

        // Comprueba que la acción redirija después de actualizar.
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        // Comprueba que la redirección vaya al listado.
        Assert.Equal(nameof(LibrosController.Index), redirect.ActionName);
        // Obtiene el libro sin seguimiento para comprobar el valor persistido.
        var libroGuardado = await context.Libros.AsNoTracking().SingleAsync();
        // Comprueba que el título se haya actualizado.
        Assert.Equal("Título actualizado", libroGuardado.Titulo);
        // Comprueba que el libro ya no esté disponible.
        Assert.False(libroGuardado.Disponible);
    }

    [Fact]
    // Comprueba que Edit rechace un identificador distinto al del modelo.
    public async Task Edit_ConIdentificadorDiferente_DevuelveBadRequest()
    {
        // Crea un contexto en memoria.
        await using var context = CreateContext();
        // Crea el controlador de prueba.
        var controller = CreateController(context);
        // Crea un libro cuyo Id no coincide con el parámetro de la acción.
        var libro = new Libro { ID = 1, Titulo = "Libro", Autor = "Autor", Categoria = "Novela", Precio = 10m };

        // Ejecuta Edit con dos identificadores diferentes.
        var result = await controller.Edit(2, libro);

        // Comprueba que la respuesta sea BadRequest.
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    // Comprueba que Delete elimine un libro existente y redirija al listado.
    public async Task Delete_ConLibroExistente_EliminaElLibroYRedirigeAlIndice()
    {
        // Crea una base de datos en memoria.
        await using var context = CreateContext();
        // Crea el libro que se eliminará.
        var libro = new Libro { Titulo = "Libro para eliminar", Autor = "Autor", Categoria = "Novela", Precio = 9.99m, Disponible = true };
        // Añade el libro al contexto.
        context.Libros.Add(libro);
        // Persiste el libro y genera su identificador.
        await context.SaveChangesAsync();
        // Crea el controlador de prueba.
        var controller = CreateController(context);

        // Ejecuta Delete con el identificador del libro.
        var result = await controller.Delete(libro.ID);

        // Comprueba que la acción redirija después de eliminar.
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        // Comprueba que la redirección apunte a Index.
        Assert.Equal(nameof(LibrosController.Index), redirect.ActionName);
        // Comprueba que ya no queden libros en la base de datos.
        Assert.Empty(context.Libros);
    }

    [Fact]
    // Comprueba que Delete devuelva NotFound si el libro no existe.
    public async Task Delete_ConLibroInexistente_DevuelveNotFound()
    {
        // Crea un contexto vacío.
        await using var context = CreateContext();
        // Crea el controlador de prueba.
        var controller = CreateController(context);

        // Ejecuta Delete con un identificador inexistente.
        var result = await controller.Delete(999);

        // Comprueba que la respuesta sea NotFound.
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    // Comprueba con Setup, Returns y Verify que el controlador use el TempData simulado al crear un libro.
    public async Task Create_ConLibroValido_GuardaElMensajeEnTempData()
    {
        // Crea una base de datos en memoria vacía.
        await using var context = CreateContext();
        // Crea un mock para no depender de un sistema de logs real.
        var logger = new Mock<ILogger<LibrosController>>();
        // Crea el mock del proveedor de TempData.
        var tempDataProvider = new Mock<ITempDataProvider>();
        //   Configura el mock para que devuelva un diccionario vacío cuando se cargue el TempData.
        tempDataProvider
            .Setup(p => p.LoadTempData(It.IsAny<HttpContext>()))
            .Returns(new Dictionary<string, object>());

        // Crea el controlador y le asigna el TempData simulado.
        var controller = new LibrosController(context, logger.Object);
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), tempDataProvider.Object);
        // Construye el libro válido que se enviará a la acción.
        var libro = new Libro { Titulo = "Rayuela", Autor = "Julio Cortázar", Categoria = "Literatura", Precio = 22m, Disponible = true };

        // Ejecuta la acción Create con el libro.
        var result = await controller.Create(libro);

        // Comprueba que la acción redirija al índice.
        Assert.IsType<RedirectToActionResult>(result);
        // Comprueba que el mensaje de confirmación sea el esperado.
        Assert.Equal("Libro guardado correctamente", controller.TempData["SuccessMessage"]);
        // Verify: comprueba que el controlador haya usado el TempData simulado una sola vez.
        tempDataProvider.Verify(p => p.LoadTempData(It.IsAny<HttpContext>()), Times.Once());
    }

    // Crea un controlador usando el contexto indicado y un logger simulado.
    private static LibrosController CreateController(BibliotecaContext context)
    {
        // Crea un mock para no depender de un sistema de logs real.
        var logger = new Mock<ILogger<LibrosController>>();
        // Crea una instancia del controlador preparada para la prueba.
        var controller = new LibrosController(context, logger.Object);
        // Crea un mock del TempData, porque LibrosController guarda ahí el mensaje "Libro guardado correctamente".
        var tempDataProvider = new Mock<ITempDataProvider>();
        // Asigna el TempData al controlador para que no sea null durante la prueba.
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), tempDataProvider.Object);
        // Devuelve el controlador listo.
        return controller;
    }

    // Crea un contexto conectado a una base de datos EF Core en memoria.
    private static BibliotecaContext CreateContext(string? databaseName = null)
    {
        // Configura las opciones del contexto.
        var options = new DbContextOptionsBuilder<BibliotecaContext>()
            // Usa el nombre recibido o genera uno nuevo para aislar la prueba.
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            // Finaliza la construcción de opciones.
            .Options;

        // Devuelve el contexto configurado.
        return new BibliotecaContext(options);
    }
}
