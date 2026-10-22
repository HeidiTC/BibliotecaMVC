using BibliotecaMVC.Repositorios;
using BibliotecaMVC.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton<NumericRoundabortConnectionInterceptor>();


builder.Services.AddDbContext<BibliotecaContext>((serviceProvider, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("BibliotecaDB"));
    options.AddInterceptors(serviceProvider.GetRequiredService<NumericRoundabortConnectionInterceptor>());
});


builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    
    options.User.RequireUniqueEmail = true;

    
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.AllowedForNewUsers = true;
})
.AddEntityFrameworkStores<BibliotecaContext>()
.AddErrorDescriber<ErroresIdentityEspanol>(); 


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    
    options.AccessDeniedPath = "/Account/AccessDenied";
});


builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IRepositorioLibro, RepositorioMemoria>();


var app = builder.Build();



using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

    
    string[] roles = { "Admin", "Bibliotecario", "Usuario" };
    foreach (var rol in roles)
    {
        if (!await roleManager.RoleExistsAsync(rol))
        {
            await roleManager.CreateAsync(new IdentityRole(rol));
        }
    }

    
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
            
            if (await userManager.IsInRoleAsync(usuario, "Usuario"))
            {
                await userManager.RemoveFromRoleAsync(usuario, "Usuario");
            }
            await userManager.AddToRoleAsync(usuario, item.Value);
        }
    }

    
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


app.UseAuthentication();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
