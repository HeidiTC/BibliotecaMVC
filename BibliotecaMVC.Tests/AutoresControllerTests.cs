using BibliotecaMVC.Controllers;
using BibliotecaMVC.Models;
using BibliotecaMVC.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BibliotecaMVC.Tests;

public class AutoresControllerTests
{
    [Fact]
    // Comprueba que Index devuelva todos los autores guardados.
    public async Task Index_DevuelveLaListaDeAutores()
    {
        // Crea un contexto EF Core con una base de datos en memoria única.
        await using var context = CreateContext();
        // Inserta dos autores de prueba en el contexto.
        context.Autores.AddRange(
            new Autor { Nombre = "Gabriel García Márquez", Nacionalidad = "Colombiana" },
            new Autor { Nombre = "Isabel Allende", Nacionalidad = "Chilena" });
        // Persiste los autores en la base de datos en memoria.
        await context.SaveChangesAsync();
        // Crea el controlador usando el contexto.
        var controller = CreateController(context);

        // Ejecuta la acción Index del controlador.
        var result = await controller.Index();

        // Comprueba que el resultado sea una vista.
        var view = Assert.IsType<ViewResult>(result);
        // Obtiene el modelo de la vista como una colección de autores.
        var autores = Assert.IsAssignableFrom<IEnumerable<Autor>>(view.Model);
        // Comprueba que la vista contenga los dos autores insertados.
        Assert.Equal(2, autores.Count());
    }

    [Fact]
    // Comprueba que Details devuelva un autor existente.
    public async Task Details_ConAutorExistente_DevuelveElAutor()
    {
        // Crea una base de datos en memoria aislada.
        await using var context = CreateContext();
        // Crea un autor de prueba.
        var autor = new Autor { Nombre = "Julio Cortázar", Nacionalidad = "Argentina" };
        // Añade el autor al contexto.
        context.Autores.Add(autor);
        // Guarda el autor y genera su identificador.
        await context.SaveChangesAsync();
        // Crea el controlador de prueba.
        var controller = CreateController(context);

        // Solicita los detalles usando el identificador generado.
        var result = await controller.Details(autor.ID);

        // Comprueba que la respuesta sea una vista.
        var view = Assert.IsType<ViewResult>(result);
        // Comprueba que la vista reciba exactamente el autor guardado.
        Assert.Same(autor, view.Model);
    }

    [Fact]
    // Comprueba la respuesta cuando el autor no existe.
    public async Task Details_ConAutorInexistente_DevuelveNotFound()
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
    // Comprueba la respuesta cuando no se envía identificador.
    public async Task Details_SinIdentificador_DevuelveNotFound()
    {
        // Crea un contexto vacío.
        await using var context = CreateContext();
        // Crea el controlador de prueba.
        var controller = CreateController(context);

        // Solicita los detalles sin enviar un identificador.
        var result = await controller.Details(null);

        // Comprueba que el controlador responda con NotFound.
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    // Comprueba que Create guarde un autor válido y redirija al listado.
    public async Task Create_ConAutorValido_GuardaElAutorYRedirigeAlIndice()
    {
        // Crea una base de datos en memoria vacía.
        await using var context = CreateContext();
        // Crea el controlador de prueba.
        var controller = CreateController(context);
        // Construye el autor válido que se enviará a la acción.
        var autor = new Autor { Nombre = "Mario Vargas Llosa", Nacionalidad = "Peruana" };

        // Ejecuta la acción Create con el autor.
        var result = await controller.Create(autor);

        // Comprueba que la acción devuelva una redirección.
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        // Comprueba que la redirección apunte a Index.
        Assert.Equal(nameof(AutoresController.Index), redirect.ActionName);
        // Comprueba que se haya guardado un único autor.
        Assert.Equal(1, await context.Autores.CountAsync());
        // Comprueba que el autor guardado tenga el nombre esperado.
        Assert.Equal("Mario Vargas Llosa", (await context.Autores.SingleAsync()).Nombre);
    }

    [Fact]
    // Comprueba que Create no guarde datos cuando el modelo es inválido.
    public async Task Create_ConModeloInvalido_NoGuardaYDevuelveLaVista()
    {
        // Crea una base de datos en memoria vacía.
        await using var context = CreateContext();
        // Crea el controlador de prueba.
        var controller = CreateController(context);
        // Simula un error de validación del nombre.
        controller.ModelState.AddModelError(nameof(Autor.Nombre), "El nombre es obligatorio.");
        // Crea el autor que se enviará con el modelo inválido.
        var autor = new Autor { Nacionalidad = "Chilena" };

        // Ejecuta la acción Create.
        var result = await controller.Create(autor);

        // Comprueba que se devuelva nuevamente la vista.
        var view = Assert.IsType<ViewResult>(result);
        // Comprueba que la vista conserve el modelo enviado.
        Assert.Same(autor, view.Model);
        // Comprueba que no se haya guardado ningún autor.
        Assert.Empty(context.Autores);
    }

    // Crea un controlador usando el contexto indicado.
    // En este proyecto AutoresController solo recibe el contexto (no usa ILogger).
    private static AutoresController CreateController(BibliotecaContext context)
    {
        // Devuelve una instancia del controlador preparada para la prueba.
        return new AutoresController(context);
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
