using BibliotecaMVC.Repositorios;
using BibliotecaMVC.Services;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<IRepositorioLibro, RepositorioMemoria>();
builder.Services.AddScoped<IAutorService, AutorServicePrueba>();


builder.Services.AddControllersWithViews();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();