using BibliotecaMVC.Repositorios;
using BibliotecaMVC.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

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

    // 1. Se crean los roles "Administrador" y "Usuario"
    string[] roles = { "Administrador", "Usuario" };
    foreach (var rol in roles)
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            await roleManager.CreateAsync(new IdentityRole(rol));
        }
    }

    // 2. Se asigna el rol "Administrador" al usuario "HeidiTorres"
    var admin = await userManager.FindByNameAsync("HeidiTorres");
    if (admin != null && !await userManager.IsInRoleAsync(admin, "Administrador"))
    {
        await userManager.AddToRoleAsync(admin, "Administrador");
    }

    // 3. Se asigna el rol "Usuario" a todos los demás usuarios que no tengan rol
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
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

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
