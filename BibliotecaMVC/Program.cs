using BibliotecaMVC.Repositorios;
using BibliotecaMVC.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Configuración de la inyección de dependencias: interceptor para redondeo de números
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
})
.AddEntityFrameworkStores<BibliotecaContext>();

// Configuración de la cookie de autenticación: si no hay sesión, redirige a /Account/Login
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
});

// Configuración de los servicios MVC: controladores y vistas
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IRepositorioLibro, RepositorioMemoria>();


var app = builder.Build();


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
