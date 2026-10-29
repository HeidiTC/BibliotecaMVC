using BibliotecaMVC.Repositorios;
using BibliotecaMVC.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Semana 15: carga el CSS y Bootstrap también cuando probamos el perfil "https (Produccion)"
// desde Visual Studio (sin esta línea, en Producción local la página sale sin estilos)
builder.WebHost.UseStaticWebAssets();

// Configuración de la inyección de dependencias: repositorios y servicios
builder.Services.AddSingleton<NumericRoundabortConnectionInterceptor>();

// Configuración de Entity Framework Core: conexión a la base de datos
builder.Services.AddDbContext<BibliotecaContext>((serviceProvider, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("BibliotecaDB"));
    options.AddInterceptors(serviceProvider.GetRequiredService<NumericRoundabortConnectionInterceptor>());
});

// Configuración de Identity: usuarios y roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Configuración de la contraseña
    options.User.RequireUniqueEmail = true;

    // Bloqueo de la cuenta: 5 intentos fallidos = 15 minutos bloqueado
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<BibliotecaContext>()
.AddErrorDescriber<ErroresIdentityEspanol>(); // mensajes de Identity en español

// Configuración de la cookie de autenticación: si no hay sesión, redirige a /Account/Login
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    // Si inició sesión pero no tiene el rol necesario, lo mandamos a Acceso denegado
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Configuración de los servicios MVC: controladores y vistas
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IRepositorioLibro, RepositorioMemoria>();


var app = builder.Build();


// Se crean los roles y se asignan a los usuarios
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    // 1. Se crean los roles "Admin", "Bibliotecario" y "Usuario"
    //    Antes de crear cada rol se verifica que no exista, así no hay duplicados
    string[] roles = { "Admin", "Bibliotecario", "Usuario" };
    foreach (var rol in roles)
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            await roleManager.CreateAsync(new IdentityRole(rol));
        }
    }

    // 2. En la semana 13 el rol se llamaba "Administrador".
    //    Los usuarios que lo tengan pasan a "Admin" y luego se borra el rol viejo
    if (await roleManager.RoleExistsAsync("Administrador"))
    {
        var adminsViejos = await userManager.GetUsersInRoleAsync("Administrador");
        foreach (var usuarioViejo in adminsViejos)
        {
            await userManager.RemoveFromRoleAsync(usuarioViejo, "Administrador");
            if (!await userManager.IsInRoleAsync(usuarioViejo, "Admin"))
            {
                await userManager.AddToRoleAsync(usuarioViejo, "Admin");
            }
        }

        var rolViejo = await roleManager.FindByNameAsync("Administrador");
        if (rolViejo != null)
        {
            await roleManager.DeleteAsync(rolViejo);
        }
    }

    // 3. Usuarios de prueba y el rol que le toca a cada uno
    //    (los usuarios deben existir: se registran desde la pantalla de Registro)
    var usuariosYRoles = new Dictionary<string, string>
    {
        { "HeidiTorres", "Admin" },
        { "Bibliotecario1", "Bibliotecario" }
    };

    foreach (var item in usuariosYRoles)
    {
        var usuario = await userManager.FindByNameAsync(item.Key);
        if (usuario != null && !await userManager.IsInRoleAsync(usuario, item.Value))
        {
            // Si se registró antes, tiene el rol "Usuario"; se le quita para que tenga un solo rol
            if (await userManager.IsInRoleAsync(usuario, "Usuario"))
            {
                await userManager.RemoveFromRoleAsync(usuario, "Usuario");
            }
            await userManager.AddToRoleAsync(usuario, item.Value);
        }
    }

    // 4. Se asigna el rol "Usuario" a todos los demás usuarios que no tengan rol
    var usuarios = userManager.Users.ToList();
    foreach (var usuario in usuarios)
    {
        var rolesDelUsuario = await userManager.GetRolesAsync(usuario);
        if (rolesDelUsuario.Count == 0)
        {
            await userManager.AddToRoleAsync(usuario, "Usuario");
        }
    }
}
// Semana 15: manejo global de errores
// En desarrollo se ve la página de detalle del error (útil para nosotros).
// En producción se usa UseExceptionHandler: atrapa cualquier excepción no controlada
// y muestra la página de error personalizada, sin enseñar detalles técnicos al usuario.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Semana 15: para códigos como 404 (registro que no existe) o 400 también se muestra
// la página de error personalizada en lugar de la página en blanco del navegador
app.UseStatusCodePagesWithReExecute("/Home/Error", "?codigo={0}");

app.UseHttpsRedirection();
app.UseRouting();

// Se revisa si hay sesión activa y se crea la cookie de autenticación
app.UseAuthentication();
// Se revisa si el usuario tiene permiso para acceder a la página
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
